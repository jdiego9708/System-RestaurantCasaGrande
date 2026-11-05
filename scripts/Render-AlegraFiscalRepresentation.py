"""Representación para revisión contable desde XML UBL original; no emite facturas.

Uso: python Render-AlegraFiscalRepresentation.py factura.xml detalle.json salida.pdf
Dependencia de soporte: reportlab. Los datos fiscales se mantienen fuera del repositorio.
"""
import hashlib
import json
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


def render(xml_path, json_path, output):
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
    total = get('a:LegalMonetaryTotal/c:PayableAmount')
    if cufe != api.get('stamp', {}).get('cufe'):
        raise ValueError('CUFE XML y REST no coinciden')
    if Decimal(total) != Decimal(str(api['total'])):
        raise ValueError('Total XML y REST no coinciden')
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
    add('REPRESENTACIÓN GRÁFICA PARA REVISIÓN DEL CONTADOR', heading)
    add('Fecha y hora de generación: ' + get('c:IssueDate') + ' ' + get('c:IssueTime'))
    for tag, label in [('AccountingSupplierParty', 'EMISOR'), ('AccountingCustomerParty', 'ADQUIRENTE')]:
        party = root.find('a:' + tag + '/a:Party', NS)
        if party is None:
            raise ValueError('Falta parte fiscal ' + tag)
        add(label, heading)
        add(get('a:PartyTaxScheme/c:RegistrationName', party) or get('a:PartyLegalEntity/c:RegistrationName', party))
        node = party.find('a:PartyTaxScheme/c:CompanyID', NS)
        if node is not None:
            add('Identificación: ' + (node.text or ''))
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
    if local('ProviderID') == '900559088':
        add('Proveedor tecnológico: SOLUCIONES ALEGRA S.A.S')
        add('Razón social contrastada con el listado oficial de proveedores tecnológicos DIAN.', small)
        provider_source = 'https://micrositios.dian.gov.co/sistema-de-facturacion-electronica/proveedores-tecnologicos/'
    add('ID software: ' + local('SoftwareID'), small)
    add('Pendiente: confirmar fabricante y nombre registrado del software, condición de agente retenedor de IVA y fecha/hora de validación DIAN mediante ApplicationResponse. Identificar el proveedor no acredita por sí solo quién es el fabricante.', small)
    add('Estado reportado por Alegra: ' + str(api.get('stamp', {}).get('legalStatus', 'No informado')), small)
    if api.get('stamp', {}).get('date'):
        add('Fecha de emisión reportada por Alegra (no verificada con ApplicationResponse): ' + str(api['stamp']['date']), small)
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
    add('Representación generada localmente del XML firmado descargado desde Alegra. No es una descarga del PDF de Alegra. Conservar el XML original y obtener el contenedor con la validación para entrega electrónica.', small)
    warnings = api.get('stamp', {}).get('warnings') or []
    if warnings:
        add('OBSERVACIONES PARA EL CONTADOR', heading)
        for warning in warnings:
            add(str(warning), small)
    width = 70 * mm
    height = sum(item.wrap(width, 14000)[1] + item.getSpaceBefore() + item.getSpaceAfter() for item in story) + 20*mm
    if height > 14000:
        raise ValueError('Documento excede altura segura de PDF')
    SimpleDocTemplate(str(output), pagesize=(80*mm, height), leftMargin=5*mm, rightMargin=5*mm, topMargin=5*mm, bottomMargin=5*mm, title=number + ' - revisión contable', author='Casa Grande').build(story)
    report = {'factura': number, 'xml_sha256': hashlib.sha256(raw).hexdigest(), 'cufe_coincide': True, 'total_coincide': True, 'qr_coincide': True, 'total_xml': total, 'estado_alegra': api.get('stamp', {}).get('legalStatus'), 'listo_para_cliente': False, 'pendientes': ['Confirmar fabricante y nombre del software con soporte Alegra.', 'Confirmar condición de agente retenedor de IVA con contador.', 'Obtener ApplicationResponse / contenedor de validación DIAN y revisar observaciones.', 'Revisar requisitos de representación y entrega: Resolución DIAN 227/2025, art. 1.5.1.2.2.1 y sección de entrega, más anexo técnico vigente.', 'Aprobar diseño de 80 mm y legibilidad del QR.'], 'firma_criptografica_verificada': False}
    report.update(proveedor_fuente=provider_source, observaciones_alegra=warnings)
    Path(output).with_suffix('.revision.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print('PDF generado; CUFE, QR y total XML/REST coinciden. Pendiente de revisión contable.')


if __name__ == '__main__':
    if len(sys.argv) != 4:
        raise SystemExit('Uso: factura.xml detalle.json salida.pdf')
    render(*sys.argv[1:])
