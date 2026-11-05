"""Copia de consulta de datos REST existentes. No crea documentos en Alegra ni sustituye su PDF original.

Uso: python Render-AlegraConsultation.py datos.json salida.pdf
Dependencia de soporte: reportlab. No forma parte del runtime Windows del restaurante.
"""
import json
import sys
from decimal import Decimal
from pathlib import Path
from xml.sax.saxutils import escape

from reportlab.graphics.barcode.qr import QrCodeWidget
from reportlab.graphics.shapes import Drawing
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_RIGHT
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import mm
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle


def render(source, target):
    invoice = json.loads(Path(source).read_text(encoding="utf-8-sig"))
    styles = {
        "body": ParagraphStyle("body", fontName="Helvetica", fontSize=8, leading=11, spaceAfter=4),
        "title": ParagraphStyle("title", fontName="Helvetica-Bold", fontSize=12, leading=15, alignment=TA_CENTER, spaceAfter=6),
        "note": ParagraphStyle("note", fontName="Helvetica", fontSize=7, leading=9, textColor=colors.HexColor("#64748b"), spaceAfter=6),
        "amount": ParagraphStyle("amount", fontName="Helvetica-Bold", fontSize=10, leading=13, alignment=TA_RIGHT, spaceAfter=7),
    }
    story = []

    def text(value, style="body"):
        return Paragraph(escape(str(value)).replace("\n", "<br/>"), styles[style])

    def add(value, style="body"):
        story.append(text(value, style))

    def money(value):
        return "$ " + f"{Decimal(str(value or 0)):,.0f}".replace(",", ".")

    number = invoice.get("numberTemplate", {}).get("fullNumber") or str(invoice["id"])
    add("CASA GRANDE", "title")
    add("Factura " + number, "title")
    add("COPIA DE CONSULTA GENERADA LOCALMENTE", "note")
    add("Construida con los datos consultados en Alegra. No es el PDF original de Alegra ni reemplaza el XML electrónico. No se emitió un documento nuevo.", "note")
    add("ID Alegra: " + str(invoice["id"]))
    add("Fecha: " + str(invoice.get("date", "")))
    add("Estado comercial: " + str(invoice.get("status", "")))
    stamp = invoice.get("stamp") or {}
    add("Estado electrónico: " + str(stamp.get("legalStatus", "No informado")))
    client = invoice.get("client") or {}
    add("Cliente: " + str(client.get("name", "")))
    identification = client.get("identificationObject") or {}
    if identification:
        add("Identificación: " + str(identification.get("type", "")) + " " + str(identification.get("number", "")))
    elif client.get("identification"):
        add("Identificación: " + str(client["identification"]))
    story.append(Spacer(1, 3 * mm))
    add("DETALLE DE PRODUCTOS", "title")
    for item in invoice.get("items", []):
        add(item.get("name") or item.get("description") or "Producto")
        add(str(item.get("quantity", "")) + " × " + money(item.get("price")) + " (precio base)")
        if item.get("discount"):
            add("Descuento: " + str(item["discount"]) + "%")
        if item.get("total") is not None:
            add("Valor registrado: " + money(item["total"]))
        for tax in item.get("tax") or []:
            add("Impuesto: " + str(tax.get("name", "")) + " " + str(tax.get("percentage", "")) + "%", "note")
        story.append(Spacer(1, 2 * mm))
    totals = []
    for key, label in [("subtotal", "Subtotal"), ("discount", "Descuento registrado"), ("tax", "Impuestos"), ("total", "TOTAL"), ("totalPaid", "Pagado"), ("balance", "Saldo")]:
        value = invoice.get(key)
        if isinstance(value, (int, float, str)):
            totals.append([text(label), text(money(value), "amount" if key == "total" else "body")])
    table = Table(totals, colWidths=[40 * mm, 30 * mm])
    table.setStyle(TableStyle([("ALIGN", (1, 0), (1, -1), "RIGHT"), ("LINEABOVE", (0, 0), (-1, 0), 0.5, colors.grey), ("LEFTPADDING", (0, 0), (-1, -1), 0), ("RIGHTPADDING", (0, 0), (-1, -1), 0)]))
    story.append(table)
    add("PAGOS", "title")
    for payment in invoice.get("payments") or []:
        add(str(payment.get("paymentMethod", "")) + " · " + money(payment.get("amount")))
    if invoice.get("anotation"):
        add("Nota: " + str(invoice["anotation"]))
    if invoice.get("observations"):
        add("Observaciones: " + str(invoice["observations"]))
    if stamp.get("cufe"):
        add("CUFE", "title")
        cufe = str(stamp["cufe"])
        add("\n".join(cufe[i:i + 35] for i in range(0, len(cufe), 35)), "note")
    if stamp.get("barCodeContent"):
        qr = QrCodeWidget(str(stamp["barCodeContent"]))
        x0, y0, x1, y1 = qr.getBounds()
        side = 52 * mm
        drawing = Drawing(side, side, transform=[side / (x1 - x0), 0, 0, side / (y1 - y0), 0, 0])
        drawing.add(qr)
        story.append(drawing)
        add("QR construido con el contenido devuelto por Alegra.", "note")
    add("Para el documento original: descargar esta factura desde Alegra web. El emisor fiscal no se reconstruye ni se inventa a partir del nombre comercial.", "note")
    document = SimpleDocTemplate(str(target), pagesize=(80 * mm, 420 * mm), rightMargin=5 * mm, leftMargin=5 * mm, topMargin=5 * mm, bottomMargin=5 * mm, title="Copia de consulta " + number, author="Casa Grande - consulta local")
    document.build(story)
    print("PDF de consulta generado: " + str(target))


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit("Uso: Render-AlegraConsultation.py entrada.json salida.pdf")
    render(sys.argv[1], sys.argv[2])
