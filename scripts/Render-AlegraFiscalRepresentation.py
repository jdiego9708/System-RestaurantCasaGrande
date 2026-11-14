"""Representacion de 80 mm o 58 mm desde XML UBL y PDF oficial de Alegra; no emite facturas.

Uso: python Render-AlegraFiscalRepresentation.py factura.xml detalle.json salida.pdf [--paper-mm 72|48]
El ancho imprimible (--paper-mm) lo detecta la aplicacion segun la impresora conectada; por defecto 72 mm.
Dependencia de soporte: reportlab (y Pillow para el logo y el QR). Los datos fiscales se
mantienen fuera del repositorio.

Diseno (todo centrado salvo la tabla de detalle y los totales): logo, emisor, adquirente,
datos de la factura, detalle, totales, QR + CUFE, autorizacion de numeracion y software.
"""
import base64
import hashlib
import io
import json
import re
import sys
from decimal import ROUND_HALF_UP, Decimal
from pathlib import Path
from xml.etree import ElementTree as ET
from xml.sax.saxutils import escape

from reportlab.graphics.barcode import qrencoder
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_RIGHT
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from reportlab.lib.utils import ImageReader
from reportlab.platypus import (BaseDocTemplate, Frame, HRFlowable, Image, PageTemplate,
                                Paragraph, Spacer, Table, TableStyle)

NS = {'c': 'urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2',
      'a': 'urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2'}

# La termica de 203 dpi imprime 8 puntos por mm: el logo y el QR se generan como imagenes de 1 bit con
# tamano en puntos enteros para que ningun modulo caiga a mitad de punto (el vectorial se suaviza y el QR
# deja de leerse). Valores del papel de 80 mm (72 mm imprimibles); Layout los adapta al ancho real.
PAGE_WIDTH = 80 * mm
MARGIN = 4 * mm
CONTENT_WIDTH = PAGE_WIDTH - 2 * MARGIN
PRINTER_DPI = 203
QR_MAX_DOTS = 560      # ancho maximo del QR con zona de silencio, en puntos de impresora
QR_MAX_MODULE_DOTS = 8  # modulo de hasta 1 mm: mas grande no mejora la lectura
LOGO_WIDTH_DOTS = 264   # ancho del logo embebido (33 mm)
DEFAULT_PRINTABLE_MM = 72
NARROW_LIMIT_MM = 70    # por debajo de este ancho imprimible se usa el diseno estrecho (papel de 58 mm)


class Layout:
    """Medidas del comprobante segun el ancho imprimible (mm) que informa la impresora en tiempo de ejecucion.

    72 mm (rollo de 80 mm): diseno historico, pagina de 80 mm con margen de 4 mm.
    48 mm (rollo de 58 mm): la pagina mide lo imprimible, con margen minimo, y todo se reparte en ese ancho.
    """

    def __init__(self, printable_mm=DEFAULT_PRINTABLE_MM):
        printable_mm = float(printable_mm)
        if not 30 <= printable_mm <= 120:
            raise ValueError('Ancho imprimible fuera de rango: ' + str(printable_mm) + ' mm')
        self.narrow = printable_mm < NARROW_LIMIT_MM
        if self.narrow:
            self.page_width = printable_mm * mm
            # Con 1 mm de margen el QR (53 + 8 de silencio = 61 modulos) cabe con modulos de 6 puntos (0,75 mm).
            self.margin = 1 * mm
            self.logo_dots = 176          # 22 mm
            self.cufe_chunk = 24
            self.quantity_header = 'Cant.'
            self.summary_value_width = 54
        else:
            self.page_width = PAGE_WIDTH
            self.margin = MARGIN
            self.logo_dots = LOGO_WIDTH_DOTS
            self.cufe_chunk = 32
            self.quantity_header = 'Cantidad'
            self.summary_value_width = 62
        self.content_width = self.page_width - 2 * self.margin
        self.qr_max_dots = int(self.content_width / mm * 8) if self.narrow else QR_MAX_DOTS
        scale = self.content_width / CONTENT_WIDTH
        # Columnas de la tabla de detalle: #, producto, cantidad, precio, impuesto.
        self.columns = [13 * scale, 79 * scale, 34 * scale, 46 * scale, 32 * scale] if self.narrow else [13, 79, 34, 46, 32]

INK = colors.black
HAIRLINE = colors.HexColor('#555555')

ID_TYPES = {'11': 'RC', '12': 'TI', '13': 'CC', '21': 'TE', '22': 'CE', '31': 'NIT', '41': 'Pasaporte',
            '42': 'Doc. extranjero', '47': 'PEP', '50': 'NIT extranjero', '91': 'NUIP'}
# Nombre de cada tributo por su codigo DIAN; si no esta, se usa el nombre que trae el XML.
TAX_NAMES = {'01': 'IVA', '02': 'impuesto al consumo de licores', '03': 'ICA',
             '04': 'impuesto nacional al consumo'}


def dots_to_points(dots):
    """Convierte puntos de impresora (203 dpi) a puntos tipograficos del PDF (72 por pulgada)."""
    return dots * 72.0 / PRINTER_DPI


def timestamp(value):
    """Fecha y hora en 12 h con segundos: 2026-11-06 05:45:58 PM. Sin hora o sin formato conocido, se deja igual."""
    match = re.match(r'\s*(\d{4}-\d{2}-\d{2})[ T](\d{2}):(\d{2}):(\d{2})', value or '')
    if not match:
        return value
    hour = int(match.group(2))
    suffix = 'AM' if hour < 12 else 'PM'
    return f'{match.group(1)} {hour % 12 or 12:02d}:{match.group(3)}:{match.group(4)} {suffix}'


def logo_flowable(layout=None):
    """Logo en blanco y negro centrado; imagen de 1 bit embebida, reescalada a puntos enteros si el papel es estrecho."""
    layout = layout or Layout()
    data = base64.b64decode(''.join(LOGO_PNG_BASE64.split()))
    pixel_width, pixel_height = ImageReader(io.BytesIO(data)).getSize()
    if layout.logo_dots != pixel_width:
        from PIL import Image as PilImage
        source = PilImage.open(io.BytesIO(data)).convert('L')
        target_height = max(1, round(pixel_height * layout.logo_dots / pixel_width))
        resized = source.resize((layout.logo_dots, target_height), PilImage.LANCZOS).point(lambda v: 255 if v > 140 else 0).convert('1')
        buffer = io.BytesIO()
        resized.save(buffer, 'PNG')
        data, pixel_width, pixel_height = buffer.getvalue(), layout.logo_dots, target_height
    flowable = Image(io.BytesIO(data), width=dots_to_points(pixel_width), height=dots_to_points(pixel_height))
    flowable.hAlign = 'CENTER'
    return flowable


def qr_flowable(value, layout=None):
    """QR como imagen de 1 bit con modulos de un numero entero de puntos de impresora."""
    from PIL import Image as PilImage, ImageDraw
    layout = layout or Layout()
    qr = qrencoder.QRCode(None, qrencoder.QRErrorCorrectLevel.M)
    qr.addData(value)
    qr.make()
    count, quiet = qr.getModuleCount(), 4
    module = max(1, min(QR_MAX_MODULE_DOTS, layout.qr_max_dots // (count + 2 * quiet)))
    side = (count + 2 * quiet) * module
    picture = PilImage.new('L', (side, side), 255)
    painter = ImageDraw.Draw(picture)
    for row in range(count):
        for col in range(count):
            if qr.modules[row][col]:
                x, y = (col + quiet) * module, (row + quiet) * module
                painter.rectangle([x, y, x + module - 1, y + module - 1], fill=0)
    buffer = io.BytesIO()
    picture.convert('1', dither=PilImage.NONE).save(buffer, 'PNG')
    buffer.seek(0)
    flowable = Image(buffer, width=dots_to_points(side), height=dots_to_points(side))
    flowable.hAlign = 'CENTER'
    return flowable


def render(xml_path, json_path, output, receipt=False, official_pdf=None, paper_mm=DEFAULT_PRINTABLE_MM):
    layout = Layout(paper_mm)
    raw = Path(xml_path).read_bytes()
    if b'<!DOCTYPE' in raw.upper() or b'<!ENTITY' in raw.upper():
        raise ValueError('Declaraciones externas no permitidas')
    root = ET.fromstring(raw)
    api = json.loads(Path(json_path).read_text(encoding='utf-8-sig'))

    def get(path, parent=root):
        return (parent.findtext(path, default='', namespaces=NS) or '').strip()

    def local(name):
        return next((n.text.strip() for n in root.iter() if n.tag.split('}')[-1] == name and n.text), '')

    def amount(value):
        """Importe en pesos enteros con punto de miles (COP no usa decimales en pantalla)."""
        whole = Decimal(value).quantize(Decimal(1), rounding=ROUND_HALF_UP)
        return f'{whole:,}'.replace(',', '.')

    def plain(value):
        """Cantidad o porcentaje sin ceros de relleno: 1.000000 -> 1; 8.00 -> 8."""
        return format(Decimal(value).normalize(), 'f')

    number, cufe = get('c:ID'), get('c:UUID')
    if root.tag.split('}')[-1] != 'Invoice':
        raise ValueError('El XML no es una factura UBL')
    if number != api.get('numberTemplate', {}).get('fullNumber'):
        raise ValueError('Numero XML y REST no coinciden')
    if api.get('stamp', {}).get('legalStatus') not in ('ACCEPTED', 'STAMPED_AND_ACCEPTED', 'STAMPED_AND_ACCEPTED_WITH_OBSERVATIONS'):
        raise ValueError('Factura sin aceptacion electronica confirmada')
    if not cufe or not api.get('id'):
        raise ValueError('Falta CUFE o ID remoto')
    total = get('a:LegalMonetaryTotal/c:PayableAmount')
    if cufe != api.get('stamp', {}).get('cufe'):
        raise ValueError('CUFE XML y REST no coinciden')
    if Decimal(total) != Decimal(str(api['total'])):
        raise ValueError('Total XML y REST no coinciden')
    official = {}
    if official_pdf:
        import pymupdf
        with pymupdf.open(official_pdf) as document:
            official_text = '\n'.join(page.get_text() for page in document)
        if cufe not in re.sub(r'\s+', '', official_text) or number not in official_text:
            raise ValueError('El PDF oficial no corresponde al CUFE/numero del XML')
        validation = re.search(r'Validaci.n DIAN:\s*(\d{4}-\d{2}-\d{2}[^\r\n]+)', official_text)
        software = re.search(r'Software:\s*([^\r\n]+)', official_text)
        provider = re.search(r'Proveedor tecnol.gico:\s*([^\r\n]+)', official_text)
        official = {'fecha_validacion': validation.group(1).strip() if validation else '',
                    'software': software.group(1).strip() if software else '',
                    'proveedor': provider.group(1).strip() if provider else '',
                    'pdf_sha256': hashlib.sha256(Path(official_pdf).read_bytes()).hexdigest()}
    if receipt and not all(official.get(key) for key in ('fecha_validacion', 'software', 'proveedor')):
        raise ValueError('Faltan datos de validacion/software del PDF oficial; no imprimir incompleto')
    qr_value = local('QRCode')
    if not qr_value or cufe not in qr_value:
        raise ValueError('QR XML no corresponde al CUFE')

    story = []
    body = ParagraphStyle('body', fontName='Helvetica', fontSize=7.8, leading=10, spaceAfter=1.5,
                          alignment=TA_CENTER, splitLongWords=True)
    strong = ParagraphStyle('strong', parent=body, fontName='Helvetica-Bold', fontSize=9, leading=11.5)
    name_style = ParagraphStyle('name', parent=strong, fontSize=10, leading=12.5)
    number_style = ParagraphStyle('number', parent=strong, fontSize=14, leading=17, spaceBefore=1, spaceAfter=3)
    heading = ParagraphStyle('heading', parent=strong, fontSize=8.6, leading=11, spaceAfter=3)
    small = ParagraphStyle('small', parent=body, fontSize=6.6, leading=8.6)
    mono = ParagraphStyle('mono', parent=small, fontName='Courier', fontSize=7.4, leading=9.4)
    cell = ParagraphStyle('cell', parent=body, alignment=TA_CENTER, spaceAfter=0, fontSize=7.4, leading=9)
    cell_name = ParagraphStyle('cell_name', parent=cell, alignment=0)
    cell_price = ParagraphStyle('cell_price', parent=cell, alignment=TA_RIGHT)
    cell_head = ParagraphStyle('cell_head', parent=cell, fontName='Helvetica-Bold', fontSize=6.6, leading=8)
    cell_head_name = ParagraphStyle('cell_head_name', parent=cell_head, alignment=0)
    cell_head_price = ParagraphStyle('cell_head_price', parent=cell_head, alignment=TA_RIGHT)
    cell_note = ParagraphStyle('cell_note', parent=cell_name, fontSize=6.2, leading=7.6)
    total_label = ParagraphStyle('total_label', parent=body, alignment=0, spaceAfter=0)
    total_value = ParagraphStyle('total_value', parent=body, alignment=TA_RIGHT, spaceAfter=0)
    grand_label = ParagraphStyle('grand_label', parent=total_label, fontName='Helvetica-Bold', fontSize=9, leading=11.5)
    grand_value = ParagraphStyle('grand_value', parent=total_value, fontName='Helvetica-Bold', fontSize=10.5, leading=13)

    def paragraph(value, style=body):
        return Paragraph(escape(str(value)).replace('\n', '<br/>'), style)

    def add(value, style=body):
        story.append(paragraph(value, style))

    def rule(thick=False):
        """Separador sutil (fino) o marcado (grueso) entre secciones."""
        story.append(HRFlowable(width='100%', thickness=1.7 if thick else .4, color=INK if thick else HAIRLINE,
                                spaceBefore=4 if thick else 3, spaceAfter=5 if thick else 4, lineCap='butt'))

    def section(title):
        story.append(Spacer(1, 1))
        add(title, heading)

    def party_of(tag):
        party = root.find('a:' + tag + '/a:Party', NS)
        if party is None:
            raise ValueError('Falta parte fiscal ' + tag)
        return party

    def party_name(party):
        return get('a:PartyTaxScheme/c:RegistrationName', party) or get('a:PartyLegalEntity/c:RegistrationName', party)

    def party_identification(party):
        node = party.find('a:PartyTaxScheme/c:CompanyID', NS)
        if node is None:
            node = party.find('a:PartyLegalEntity/c:CompanyID', NS)
        if node is None or not (node.text or '').strip():
            return ''
        scheme = node.get('schemeName', '')
        value = node.text.strip()
        if scheme == '31' and node.get('schemeID'):
            value += '-' + node.get('schemeID')
        return ID_TYPES.get(scheme, 'ID') + ': ' + value

    def party_address(party):
        address = party.find('a:PhysicalLocation/a:Address', NS)
        if address is None:
            address = party.find('a:PartyTaxScheme/a:RegistrationAddress', NS)
        if address is None:
            return ''
        return ', '.join(filter(None, [get('a:AddressLine/c:Line', address), get('c:CityName', address)]))

    # --- Cabecera: logo, emisor y regimen ---------------------------------------------------
    story.append(logo_flowable(layout))
    rule()
    supplier = party_of('AccountingSupplierParty')
    add(party_name(supplier), name_style)
    add(party_identification(supplier))
    add(party_address(supplier))
    add(get('a:Contact/c:ElectronicMail', supplier))
    add('Régimen: Impuesto Nacional al Consumo - INC')
    fiscal_labels = {'O-13': 'Gran contribuyente', 'O-15': 'Autorretenedor',
                     'O-23': 'Agente de retención de IVA', 'O-47': 'Régimen SIMPLE'}
    for code in get('a:PartyTaxScheme/c:TaxLevelCode', supplier).split(';'):
        if code.strip() in fiscal_labels:
            add(fiscal_labels[code.strip()])
    rule(thick=True)

    # --- Adquirente ------------------------------------------------------------------------
    customer = party_of('AccountingCustomerParty')
    section('ADQUIRIENTE')
    add(party_name(customer), strong)
    add(party_identification(customer))
    rule()

    # --- Datos de la factura ---------------------------------------------------------------
    add('FACTURA ELECTRÓNICA DE VENTA', heading)
    add(number, number_style)
    if not receipt:
        add('REPRESENTACIÓN GRÁFICA PARA REVISIÓN DEL CONTADOR', small)
    if official.get('fecha_validacion'):
        add('Emisión DIAN: ' + timestamp(official['fecha_validacion']))
    for pay in root.findall('a:PaymentMeans', NS):
        add('Forma de pago: ' + {'1': 'Contado', '2': 'Crédito'}.get(get('c:ID', pay), get('c:ID', pay)))
        add('Método de pago: ' + (get('c:PaymentID', pay) or 'código ' + get('c:PaymentMeansCode', pay)))
        add('Vencimiento: ' + get('c:PaymentDueDate', pay))
    rule()

    # --- Detalle: tabla # / producto / cantidad / precio / impuesto ---------------------------
    section('DETALLE')
    rows = [[paragraph('#', cell_head), paragraph('Producto', cell_head_name), paragraph(layout.quantity_header, cell_head),
             paragraph('Precio', cell_head_price), paragraph('Imp', cell_head)]]
    for position, line in enumerate(root.findall('a:InvoiceLine', NS), start=1):
        name = get('a:Item/c:Description', line) or get('c:Note', line)
        notes = [paragraph(name, cell_name)]
        for allowance in line.findall('a:AllowanceCharge', NS):
            kind = 'Cargo' if get('c:ChargeIndicator', allowance) == 'true' else 'Descuento'
            notes.append(paragraph(kind + ': $' + amount(get('c:Amount', allowance)), cell_note))
        percents = []
        for tax in line.findall('a:TaxTotal/a:TaxSubtotal', NS):
            percent = get('a:TaxCategory/c:Percent', tax)
            if percent:
                percents.append(plain(percent) + '%')
        rows.append([paragraph(position, cell), notes,
                     paragraph(plain(get('c:InvoicedQuantity', line) or '0'), cell),
                     paragraph(amount(get('a:Price/c:PriceAmount', line) or '0'), cell_price),
                     paragraph('/'.join(percents) or '0%', cell)])
    table = Table(rows, colWidths=layout.columns, hAlign='CENTER', repeatRows=1)
    table.setStyle(TableStyle([
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('LEFTPADDING', (0, 0), (-1, -1), 2), ('RIGHTPADDING', (0, 0), (-1, -1), 2),
        ('TOPPADDING', (0, 0), (-1, -1), 3), ('BOTTOMPADDING', (0, 0), (-1, -1), 3),
        ('LINEBELOW', (0, 0), (-1, 0), .7, INK),
        ('LINEBELOW', (0, 1), (-1, -2), .25, colors.HexColor('#999999')),
        ('LINEBELOW', (0, -1), (-1, -1), .7, INK)]))
    story.append(table)
    add('Valores en COP · Precio unitario sin impuestos', small)

    # --- Totales ---------------------------------------------------------------------------
    def money_row(label, value, label_style=total_label, value_style=total_value):
        return [paragraph(label, label_style), paragraph('$' + amount(value), value_style)]

    def monetary(key):
        return get('a:LegalMonetaryTotal/c:' + key)

    subtotal = monetary('LineExtensionAmount') or str(sum(
        (Decimal(get('c:LineExtensionAmount', line) or '0') for line in root.findall('a:InvoiceLine', NS)), Decimal(0)))
    with_taxes = monetary('TaxInclusiveAmount') or total
    totals = [money_row('Subtotal', subtotal)]
    for key, label in [('AllowanceTotalAmount', 'Descuentos globales'), ('ChargeTotalAmount', 'Cargos globales')]:
        if monetary(key) and Decimal(monetary(key)) != 0:
            totals.append(money_row(label, monetary(key)))
    for tax in root.findall('a:TaxTotal/a:TaxSubtotal', NS):
        scheme_id = get('a:TaxCategory/a:TaxScheme/c:ID', tax)
        tax_name = TAX_NAMES.get(scheme_id) or get('a:TaxCategory/a:TaxScheme/c:Name', tax) or 'impuestos'
        percent = get('a:TaxCategory/c:Percent', tax)
        totals.append(money_row('Total ' + tax_name + (' (' + plain(percent) + '%)' if percent else ''),
                                get('c:TaxAmount', tax) or '0'))
    emphasized = [len(totals)]
    totals.append(money_row('TOTAL CON IMPUESTOS', with_taxes, grand_label, grand_value))
    if Decimal(total) != Decimal(with_taxes):
        # Solo si hay anticipos, redondeo u otros ajustes: el total a pagar difiere del total con impuestos.
        for key, label in [('PrepaidAmount', 'Anticipos'), ('PayableRoundingAmount', 'Redondeo')]:
            if monetary(key) and Decimal(monetary(key)) != 0:
                totals.append(money_row(label, monetary(key)))
        emphasized.append(len(totals))
        totals.append(money_row('TOTAL A PAGAR', total, grand_label, grand_value))
    summary = Table(totals, colWidths=[layout.content_width - layout.summary_value_width, layout.summary_value_width], hAlign='CENTER')
    summary.setStyle(TableStyle([
        ('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
        ('LEFTPADDING', (0, 0), (-1, -1), 2), ('RIGHTPADDING', (0, 0), (-1, -1), 2),
        ('TOPPADDING', (0, 0), (-1, -1), 1.5), ('BOTTOMPADDING', (0, 0), (-1, -1), 1.5)] +
        [('LINEABOVE', (0, row), (-1, row), .9, INK) for row in emphasized] +
        [('TOPPADDING', (0, row), (-1, row), 4) for row in emphasized]))
    story.append(Spacer(1, 3))
    story.append(summary)
    for note in root.findall('c:Note', NS):
        if note.text:
            story.append(Spacer(1, 2))
            add(note.text, small)
    rule()

    # --- QR, CUFE y fechas ------------------------------------------------------------------
    story.append(qr_flowable(qr_value, layout))
    story.append(Spacer(1, 2))
    add('Consulta DIAN: escanear el QR original incluido en el XML.', small)
    add('CUFE', heading)
    add('\n'.join(cufe[i:i + layout.cufe_chunk] for i in range(0, len(cufe), layout.cufe_chunk)), mono)
    add('Fecha de generación: ' + timestamp(get('c:IssueDate') + ' ' + get('c:IssueTime')))
    if official.get('fecha_validacion'):
        add('Fecha de validación: ' + timestamp(official['fecha_validacion']))
    rule()

    # --- Autorizacion de numeracion -----------------------------------------------------------
    section('AUTORIZACIÓN DE NUMERACIÓN')
    add('Resolución DIAN: ' + local('InvoiceAuthorization'))
    add('Vigencia: ' + local('StartDate') + ' a ' + local('EndDate'))
    add('Prefijo ' + local('Prefix') + ' · Desde ' + local('From') + ' hasta ' + local('To'))
    rule()

    # --- Software y trazabilidad ------------------------------------------------------------
    section('SOFTWARE Y TRAZABILIDAD')
    add('Proveedor de software, NIT XML: ' + local('ProviderID'))
    provider_source = None
    if local('ProviderID') == '900559088' and not official.get('proveedor'):
        add('Proveedor tecnológico: SOLUCIONES ALEGRA S.A.S')
        if not receipt:
            add('Razón social contrastada con el listado oficial de proveedores tecnológicos DIAN.', small)
        provider_source = 'https://micrositios.dian.gov.co/sistema-de-facturacion-electronica/proveedores-tecnologicos/'
    if official.get('software'):
        add('Software: ' + official['software'], small)
    if official.get('proveedor'):
        add('Proveedor tecnologico: ' + official['proveedor'], small)
    add('ID software: ' + local('SoftwareID'), small)
    if not receipt:
        add('Pendiente: confirmar fabricante y nombre registrado del software, condición de agente retenedor de IVA y fecha/hora de validación DIAN mediante ApplicationResponse. Identificar el proveedor no acredita por sí solo quién es el fabricante.', small)
    state = str(api.get('stamp', {}).get('legalStatus', 'No informado'))
    add('Estado: ' + ('Aceptada con observaciones' if 'WITH_OBSERVATIONS' in state else 'Aceptada') if receipt else 'Estado reportado por Alegra: ' + state, small)
    if not receipt and api.get('stamp', {}).get('date'):
        add('Fecha de emisión reportada por Alegra (no verificada con ApplicationResponse): ' + timestamp(str(api['stamp']['date'])), small)
    if not receipt:
        add('No se descargó el ApplicationResponse DIAN ni se verificó criptográficamente la firma del XML. Revisar las observaciones de validación en Alegra.', small)
    if receipt:
        add('Representación gráfica generada a partir del XML de la factura electrónica de Alegra.', small)
    else:
        add('Representación generada localmente del XML firmado descargado desde Alegra. No es una descarga del PDF de Alegra. Conservar el XML original y obtener el contenedor con la validación para entrega electrónica.', small)
    warnings = api.get('stamp', {}).get('warnings') or []
    if warnings:
        rule()
        section('OBSERVACIONES DE VALIDACIÓN' if receipt else 'OBSERVACIONES PARA EL CONTADOR')
        for warning in warnings:
            add(str(warning), small)

    # --- Pagina unica con el ancho del papel y la altura exacta del contenido --------------------------
    height, previous_after = 0, 0
    for index, item in enumerate(story):
        item_height = item.wrap(layout.content_width, 14000)[1]
        height += item_height + (0 if index == 0 else max(item.getSpaceBefore(), previous_after))
        previous_after = item.getSpaceAfter()
    height += 2 * layout.margin + previous_after + 4 * mm
    if height > 14000:
        raise ValueError('Documento excede altura segura de PDF')
    title = number + (' - representación gráfica' if receipt else ' - revisión contable')
    document = BaseDocTemplate(str(output), pagesize=(layout.page_width, height), leftMargin=layout.margin, rightMargin=layout.margin,
                               topMargin=layout.margin, bottomMargin=layout.margin, title=title, author='Casa Grande')
    document.addPageTemplates([PageTemplate(id='receipt', frames=[Frame(
        layout.margin, layout.margin, layout.content_width, height - 2 * layout.margin,
        leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)])])
    document.build(story)
    if document.page != 1:
        raise ValueError('El comprobante no cabe en una sola pagina de ' + str(round(layout.page_width / mm)) + ' mm')
    report = {'factura': number, 'xml_sha256': hashlib.sha256(raw).hexdigest(), 'cufe_coincide': True, 'total_coincide': True, 'qr_coincide': True, 'total_xml': total, 'estado_alegra': api.get('stamp', {}).get('legalStatus'), 'modo': 'recibo' if receipt else 'revision', 'id_alegra': str(api['id']), 'listo_para_cliente': False, 'pendientes': ['Confirmar fabricante y nombre del software con soporte Alegra.', 'Confirmar condición de agente retenedor de IVA con contador.', 'Obtener ApplicationResponse / contenedor de validación DIAN y revisar observaciones.', 'Revisar requisitos de representación y entrega: Resolución DIAN 227/2025, art. 1.5.1.2.2.1 y sección de entrega, más anexo técnico vigente.', 'Aprobar diseño de 80 mm y legibilidad del QR.'], 'firma_criptografica_verificada': False}
    report.update(proveedor_fuente=provider_source, observaciones_alegra=warnings, datos_pdf_oficial=official, validacion_de_datos_completa=bool(official), numero_coincide=True)
    if receipt:
        report.pop('listo_para_cliente', None)
        report['revision_contable_pendiente'] = True
        report['pendientes'] = ['Revision contable del diseno y leyendas aplicables.', 'Conservar el XML original y el contenedor DIAN para entrega electronica.']
    Path(output).with_suffix('.revision.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(('PDF de ' + str(round(layout.page_width / mm)) + ' mm generado; numero, CUFE, QR y total verificados.') if receipt
          else 'PDF de revision generado; pendiente de revision contable.')


def self_test():
    import tempfile
    import pymupdf
    from reportlab.pdfgen import canvas
    with tempfile.TemporaryDirectory(prefix='CasaGrande-pdf-check-') as folder:
        file = Path(folder) / 'renderer-check.pdf'
        # Se comprueban los dos anchos de papel: 72 mm imprimibles (rollo de 80) y 48 mm (rollo de 58).
        for printable in (72, 48):
            layout = Layout(printable)
            story = [Paragraph('CasaGrande PDF self-test', ParagraphStyle('t', fontName='Helvetica', fontSize=8)),
                     logo_flowable(layout), qr_flowable('CasaGrande portable renderer', layout)]
            file = Path(folder) / ('renderer-check-' + str(printable) + '.pdf')
            document = BaseDocTemplate(str(file), pagesize=(layout.page_width, 200 * mm), leftMargin=layout.margin, rightMargin=layout.margin,
                                       topMargin=layout.margin, bottomMargin=layout.margin)
            document.addPageTemplates([PageTemplate(id='check', frames=[Frame(
                layout.margin, layout.margin, layout.content_width, 200 * mm - 2 * layout.margin,
                leftPadding=0, rightPadding=0, topPadding=0, bottomPadding=0)])])
            document.build(story)
            expected = 80 if printable == 72 else printable
            with pymupdf.open(file) as generated:
                if len(generated) != 1 or abs(generated[0].rect.width * 25.4 / 72 - expected) > .01 or 'CasaGrande' not in generated[0].get_text():
                    raise ValueError('El renderer portable no genera un PDF valido de ' + str(expected) + ' mm')
                if len(generated[0].get_images()) != 2:
                    raise ValueError('El renderer portable no incrusta el logo y el QR')
    print('OK: renderer portable, texto, logo, QR y lectura PDF de 80 y 58 mm.')


# PNG de 1 bit del logo (recortado y reducido a 264 puntos de ancho para termica de 203 dpi).
# Origen: Logo Casa Grande Restaurante en blanco y negro.png
LOGO_PNG_BASE64 = '''
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
'''


if __name__ == '__main__':
    if sys.argv[1:] == ['--self-test']:
        self_test()
        raise SystemExit(0)
    args = sys.argv[1:]
    official_pdf = None
    if '--official-pdf' in args:
        index = args.index('--official-pdf')
        official_pdf = args[index + 1]
        del args[index:index + 2]
    # Ancho imprimible en mm que la aplicacion detecta en tiempo de ejecucion (72 = rollo de 80 mm, 48 = rollo de 58 mm).
    paper_mm = DEFAULT_PRINTABLE_MM
    if '--paper-mm' in args:
        index = args.index('--paper-mm')
        paper_mm = float(args[index + 1].replace(',', '.'))
        del args[index:index + 2]
    receipt = '--receipt' in args
    args = [arg for arg in args if arg != '--receipt']
    if len(args) != 3:
        raise SystemExit('Uso: factura.xml detalle.json salida.pdf [--receipt --official-pdf oficial.pdf] [--paper-mm 72|48]')
    render(*args, receipt=receipt, official_pdf=official_pdf, paper_mm=paper_mm)
