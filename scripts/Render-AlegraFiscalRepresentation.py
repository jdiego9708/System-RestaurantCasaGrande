"""Representacion de 80 mm desde XML UBL y PDF oficial de Alegra; no emite facturas.

Uso: python Render-AlegraFiscalRepresentation.py factura.xml detalle.json salida.pdf
Dependencia de soporte: reportlab. Los datos fiscales se mantienen fuera del repositorio.
"""
import hashlib
import json
import re
import sys
from decimal import Decimal
from pathlib import Path
from xml.etree import ElementTree as ET
from xml.sax.saxutils import escape

from reportlab.graphics.barcode.qr import QrCodeWidget
from reportlab.graphics.shapes import Drawing
from reportlab.lib import colors
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from reportlab.platypus import Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle

NS = {'c': 'urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2',
      'a': 'urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2'}


def render(xml_path, json_path, output, receipt=False, official_pdf=None):
    raw = Path(xml_path).read_bytes()
    if b'<!DOCTYPE' in raw.upper() or b'<!ENTITY' in raw.upper():
        raise ValueError('Declaraciones externas no permitidas')
    root = ET.fromstring(raw)
    api = json.loads(Path(json_path).read_text(encoding='utf-8-sig'))

    def get(path, parent=root):
        return (parent.findtext(path, default='', namespaces=NS) or '').strip()

    def local(name):
        return next((n.text.strip() for n in root.iter() if n.tag.split('}')[-1] == name and n.text), '')

    def money(s):
        return f'{Decimal(s):,.2f}'.replace(',', '_').replace('.', ',').replace('_', '.') + ' COP'

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
    body = ParagraphStyle('body', fontName='Helvetica', fontSize=8, leading=11, spaceAfter=4, splitLongWords=True)
    heading = ParagraphStyle('heading', parent=body, fontName='Helvetica-Bold', fontSize=10, leading=13, spaceBefore=7)
    small = ParagraphStyle('small', parent=body, fontSize=6.5, leading=9)

    def paragraph(value, style=body):
        return Paragraph(escape(str(value)).replace('\n', '<br/>'), style)

    def grid(rows, widths=None, emphasized=False):
        table = Table([[paragraph(cell, body) for cell in row] for row in rows],
                      colWidths=widths or [35*mm, 35*mm], hAlign='LEFT')
        table.setStyle(TableStyle([
            ('BOX', (0, 0), (-1, -1), .45, colors.HexColor('#94a3b8')),
            ('INNERGRID', (0, 0), (-1, -1), .25, colors.HexColor('#cbd5e1')),
            ('ROWBACKGROUNDS', (0, 0), (-1, -1), [colors.white, colors.HexColor('#f1f5f9')]),
            ('BACKGROUND', (0, 0), (-1, -1), colors.HexColor('#e2e8f0') if emphasized else colors.white),
            ('VALIGN', (0, 0), (-1, -1), 'TOP'),
            ('LEFTPADDING', (0, 0), (-1, -1), 5), ('RIGHTPADDING', (0, 0), (-1, -1), 5),
            ('TOPPADDING', (0, 0), (-1, -1), 5), ('BOTTOMPADDING', (0, 0), (-1, -1), 4)]))
        story.append(table)
        story.append(Spacer(1, 2*mm))

    sections = {'EMISOR', 'ADQUIRENTE', 'DETALLE', 'TOTALES', 'AUTORIZACIÓN DE NUMERACIÓN',
                'SOFTWARE Y TRAZABILIDAD', 'CUFE', 'OBSERVACIONES PARA EL CONTADOR'}

    def add(value, style=body):
        if style is heading and value in sections:
            story.append(Spacer(1, 3*mm))
            table = Table([[paragraph(value, heading)]], colWidths=[70*mm])
            table.setStyle(TableStyle([('BACKGROUND', (0,0),(-1,-1),colors.HexColor('#e2e8f0')),
                                      ('LINEBELOW',(0,0),(-1,-1),.8,colors.HexColor('#475569')),
                                      ('LEFTPADDING',(0,0),(-1,-1),5), ('BOTTOMPADDING',(0,0),(-1,-1),5)]))
            story.extend([table, Spacer(1, 2*mm)])
        else:
            story.append(paragraph(value, style))

    add('CASA GRANDE', heading)
    add('FACTURA ELECTRÓNICA DE VENTA ' + number, heading)
    if not receipt:
        add('REPRESENTACIÓN GRÁFICA PARA REVISIÓN DEL CONTADOR', heading)
    else:
        add('Representación gráfica del XML electrónico', small)
    add('Fecha y hora de generación: ' + get('c:IssueDate') + ' ' + get('c:IssueTime'))
    if official.get('fecha_validacion'):
        add('Validación DIAN: ' + official['fecha_validacion'])
    for tag, label in [('AccountingSupplierParty', 'EMISOR'), ('AccountingCustomerParty', 'ADQUIRENTE')]:
        party = root.find('a:' + tag + '/a:Party', NS)
        if party is None:
            raise ValueError('Falta parte fiscal ' + tag)
        add(label, heading)
        add(get('a:PartyTaxScheme/c:RegistrationName', party) or get('a:PartyLegalEntity/c:RegistrationName', party))
        node = party.find('a:PartyTaxScheme/c:CompanyID', NS)
        if node is not None:
            identifier = node.text or ''
            if node.get('schemeName') == '31' and node.get('schemeID'):
                identifier += '-' + node.get('schemeID')
            add('Identificación: ' + identifier)
        address = party.find('a:PhysicalLocation/a:Address', NS)
        if address is None:
            address = party.find('a:PartyTaxScheme/a:RegistrationAddress', NS)
        if address is not None:
            add(', '.join(filter(None, [get('a:AddressLine/c:Line', address), get('c:CityName', address), get('c:CountrySubentity', address), get('a:Country/c:Name', address)])))
        for path, label2 in [('a:Contact/c:Telephone', 'Teléfono'), ('a:Contact/c:ElectronicMail', 'Correo'), ('a:PartyTaxScheme/c:TaxLevelCode', 'Responsabilidades fiscales (código XML)'), ('a:PartyTaxScheme/a:TaxScheme/c:Name', 'Tributo')]:
            if tag == 'AccountingCustomerParty':
                continue
            if tag == 'AccountingSupplierParty' and label2 != 'Teléfono':
                continue
            value = get(path, party)
            if value:
                add(label2 + ': ' + value)
        if tag == 'AccountingSupplierParty':
            fiscal_labels = {'O-13': 'Gran contribuyente', 'O-15': 'Autorretenedor',
                             'O-23': 'Agente de retención de IVA', 'O-47': 'Régimen SIMPLE'}
            for code in get('a:PartyTaxScheme/c:TaxLevelCode', party).split(';'):
                if code.strip() in fiscal_labels:
                    add(fiscal_labels[code.strip()])
    add('DETALLE', heading)
    add('Valores en COP · Precios unitarios sin impuestos', small)
    product_title = ParagraphStyle('product_title', parent=body, fontName='Helvetica-Bold', spaceBefore=3, spaceAfter=2)
    compact = ParagraphStyle('compact', parent=small, spaceAfter=1)
    for line in root.findall('a:InvoiceLine', NS):
        add(get('c:ID', line) + '. ' + get('a:Item/c:Description', line), product_title)
        quantity = line.find('c:InvoicedQuantity', NS)
        add('Cant. ' + get('c:InvoicedQuantity', line) + ' · Und. ' +
            (quantity.get('unitCode', '') if quantity is not None else '') +
            ' · P. unit. $' + money(get('a:Price/c:PriceAmount', line)).replace(' COP', ''), compact)
        add('Valor sin imp.: $' + money(get('c:LineExtensionAmount', line)).replace(' COP', ''), compact)
        for code in line.findall('a:Item/a:StandardItemIdentification/c:ID', NS):
            add('Código producto: ' + (code.text or '') + ' · Esquema: ' + code.get('schemeID', ''), small)
        for allowance in line.findall('a:AllowanceCharge', NS):
            add(('Cargo' if get('c:ChargeIndicator', allowance) == 'true' else 'Descuento') + ': ' + money(get('c:Amount', allowance)))
        for tax in line.findall('a:TaxTotal/a:TaxSubtotal', NS):
            add(get('a:TaxCategory/a:TaxScheme/c:Name', tax) + ' ' + get('a:TaxCategory/c:Percent', tax) + '% · Base $' + money(get('c:TaxableAmount', tax)).replace(' COP', '') + ' · Imp. $' + money(get('c:TaxAmount', tax)).replace(' COP', ''), compact)
    add('TOTALES', heading)
    for key, label in [('LineExtensionAmount', 'Subtotal de líneas'), ('TaxExclusiveAmount', 'Base gravable'), ('AllowanceTotalAmount', 'Descuentos globales'), ('ChargeTotalAmount', 'Cargos globales'), ('TaxInclusiveAmount', 'Total con impuestos'), ('PrepaidAmount', 'Anticipos'), ('PayableRoundingAmount', 'Redondeo'), ('PayableAmount', 'TOTAL A PAGAR')]:
        value = get('a:LegalMonetaryTotal/c:' + key)
        if value:
            grid([[label, money(value)]], emphasized=key == 'PayableAmount')
    for tax in root.findall('a:TaxTotal/a:TaxSubtotal', NS):
        add('Impuesto ' + get('a:TaxCategory/a:TaxScheme/c:Name', tax) + ' (' + get('a:TaxCategory/a:TaxScheme/c:ID', tax) + ') ' + get('a:TaxCategory/c:Percent', tax) + '%: ' + money(get('c:TaxAmount', tax)))
    for pay in root.findall('a:PaymentMeans', NS):
        add('Forma de pago: ' + {'1': 'Contado', '2': 'Crédito'}.get(get('c:ID', pay), get('c:ID', pay)))
        add('Medio de pago: ' + get('c:PaymentID', pay) + ' (código ' + get('c:PaymentMeansCode', pay) + ')')
        add('Vencimiento: ' + get('c:PaymentDueDate', pay))
    for note in root.findall('c:Note', NS):
        add(note.text or '', small)
    add('AUTORIZACIÓN DE NUMERACIÓN', heading)
    add('Resolución DIAN: ' + local('InvoiceAuthorization'))
    add('Vigencia: ' + local('StartDate') + ' a ' + local('EndDate'))
    add('Prefijo ' + local('Prefix') + ' · Desde ' + local('From') + ' hasta ' + local('To'))
    add('SOFTWARE Y TRAZABILIDAD', heading)
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
        add('Fecha de emisión reportada por Alegra (no verificada con ApplicationResponse): ' + str(api['stamp']['date']), small)
    if not receipt:
        add('No se descargó el ApplicationResponse DIAN ni se verificó criptográficamente la firma del XML. Revisar las observaciones de validación en Alegra.', small)
    add('CUFE', heading)
    add('\n'.join(cufe[i:i+32] for i in range(0, len(cufe), 32)), small)
    qr = QrCodeWidget(qr_value)
    x0, y0, x1, y1 = qr.getBounds()
    side = 53 * mm
    drawing = Drawing(side, side, transform=[side/(x1-x0), 0, 0, side/(y1-y0), 0, 0])
    drawing.add(qr)
    story.extend([Spacer(1, 2*mm), drawing])
    add('Consulta DIAN: escanear el QR original incluido en el XML.', small)
    if receipt:
        add('Representación gráfica generada a partir del XML de la factura electrónica de Alegra.', small)
    else:
        add('Representación generada localmente del XML firmado descargado desde Alegra. No es una descarga del PDF de Alegra. Conservar el XML original y obtener el contenedor con la validación para entrega electrónica.', small)
    warnings = api.get('stamp', {}).get('warnings') or []
    if warnings:
        add('OBSERVACIONES DE VALIDACIÓN' if receipt else 'OBSERVACIONES PARA EL CONTADOR', heading)
        for warning in warnings:
            add(str(warning), small)
    width = 70 * mm
    height = sum(item.wrap(width, 14000)[1] + item.getSpaceBefore() + item.getSpaceAfter() for item in story) + 20*mm
    if height > 14000:
        raise ValueError('Documento excede altura segura de PDF')
    SimpleDocTemplate(str(output), pagesize=(80*mm, height), leftMargin=5*mm, rightMargin=5*mm, topMargin=5*mm, bottomMargin=5*mm, title=number + (' - representación gráfica' if receipt else ' - revisión contable'), author='Casa Grande').build(story)
    report = {'factura': number, 'xml_sha256': hashlib.sha256(raw).hexdigest(), 'cufe_coincide': True, 'total_coincide': True, 'qr_coincide': True, 'total_xml': total, 'estado_alegra': api.get('stamp', {}).get('legalStatus'), 'modo': 'recibo' if receipt else 'revision', 'id_alegra': str(api['id']), 'listo_para_cliente': False, 'pendientes': ['Confirmar fabricante y nombre del software con soporte Alegra.', 'Confirmar condición de agente retenedor de IVA con contador.', 'Obtener ApplicationResponse / contenedor de validación DIAN y revisar observaciones.', 'Revisar requisitos de representación y entrega: Resolución DIAN 227/2025, art. 1.5.1.2.2.1 y sección de entrega, más anexo técnico vigente.', 'Aprobar diseño de 80 mm y legibilidad del QR.'], 'firma_criptografica_verificada': False}
    report.update(proveedor_fuente=provider_source, observaciones_alegra=warnings, datos_pdf_oficial=official, validacion_de_datos_completa=bool(official), numero_coincide=True)
    if receipt:
        report.pop('listo_para_cliente', None)
        report['revision_contable_pendiente'] = True
        report['pendientes'] = ['Revision contable del diseno y leyendas aplicables.', 'Conservar el XML original y el contenedor DIAN para entrega electronica.']
    Path(output).with_suffix('.revision.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print('PDF de 80 mm generado; numero, CUFE, QR y total verificados.' if receipt else 'PDF de revision generado; pendiente de revision contable.')


def self_test():
    import tempfile
    import pymupdf
    from reportlab.pdfgen import canvas
    with tempfile.TemporaryDirectory(prefix='CasaGrande-pdf-check-') as folder:
        file = Path(folder) / 'renderer-check.pdf'
        document = canvas.Canvas(str(file), pagesize=(80 * mm, 100 * mm))
        document.drawString(5 * mm, 90 * mm, 'CasaGrande PDF self-test')
        qr = QrCodeWidget('CasaGrande portable renderer')
        x0, y0, x1, y1 = qr.getBounds()
        side = 50 * mm
        drawing = Drawing(side, side, transform=[side/(x1-x0), 0, 0, side/(y1-y0), 0, 0])
        drawing.add(qr)
        drawing.drawOn(document, 5 * mm, 10 * mm)
        document.save()
        with pymupdf.open(file) as generated:
            if len(generated) != 1 or abs(generated[0].rect.width * 25.4 / 72 - 80) > .01 or 'CasaGrande' not in generated[0].get_text():
                raise ValueError('El renderer portable no genera PDF de 80 mm valido')
    print('OK: renderer portable, texto, QR y lectura PDF de 80 mm.')


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
    receipt = '--receipt' in args
    args = [arg for arg in args if arg != '--receipt']
    if len(args) != 3:
        raise SystemExit('Uso: factura.xml detalle.json salida.pdf [--receipt --official-pdf oficial.pdf]')
    render(*args, receipt=receipt, official_pdf=official_pdf)
