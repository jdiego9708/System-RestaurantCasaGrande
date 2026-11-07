"""Pruebas con documentos sinteticos; sin red, datos reales ni impresoras."""
import contextlib
import importlib.util
import io
import json
import tempfile
import unittest
from pathlib import Path

import pymupdf
from reportlab.pdfgen import canvas

spec = importlib.util.spec_from_file_location('renderer', Path(__file__).with_name('Render-AlegraFiscalRepresentation.py'))
renderer = importlib.util.module_from_spec(spec)
spec.loader.exec_module(renderer)


class FiscalRendererTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        self.cufe = 'a' * 96
        party = '<a:Party><a:PartyTaxScheme><c:RegistrationName>EMPRESA DE PRUEBA</c:RegistrationName><c:CompanyID>123</c:CompanyID></a:PartyTaxScheme></a:Party>'
        self.xml = f'''<Invoice xmlns:c="{renderer.NS['c']}" xmlns:a="{renderer.NS['a']}">
          <c:ID>FE-TEST-1</c:ID><c:UUID>{self.cufe}</c:UUID><c:IssueDate>2026-01-01</c:IssueDate><c:IssueTime>12:00:00-05:00</c:IssueTime>
          <QRCode>https://example.invalid/?cufe={self.cufe}</QRCode>
          <a:AccountingSupplierParty>{party}</a:AccountingSupplierParty><a:AccountingCustomerParty>{party}</a:AccountingCustomerParty>
          <a:InvoiceLine><c:ID>1</c:ID><c:InvoicedQuantity unitCode="EA">1</c:InvoicedQuantity><c:LineExtensionAmount>100</c:LineExtensionAmount>
          <a:Item><c:Description>PRODUCTO DE PRUEBA</c:Description></a:Item><a:Price><c:PriceAmount>100</c:PriceAmount></a:Price></a:InvoiceLine>
          <a:LegalMonetaryTotal><c:PayableAmount>100</c:PayableAmount></a:LegalMonetaryTotal></Invoice>'''
        self.data = {'id': 'synthetic-1', 'numberTemplate': {'fullNumber': 'FE-TEST-1'}, 'total': 100,
                     'stamp': {'cufe': self.cufe, 'legalStatus': 'STAMPED_AND_ACCEPTED'}}
        self.official = self.root / 'official.pdf'
        document = canvas.Canvas(str(self.official))
        document.setFont('Helvetica', 8)
        for index, line in enumerate(['FE-TEST-1', self.cufe, 'Validacion DIAN: 2026-01-01 12:00:01-05:00',
                                      'Software: SOFTWARE DE PRUEBA - NIT 123', 'Proveedor tecnologico: PROVEEDOR DE PRUEBA']):
            document.drawString(20, 800 - index * 20, line)
        document.save()

    def render(self, official=True):
        xml, data, output = (self.root / name for name in ('invoice.xml', 'detail.json', 'receipt.pdf'))
        xml.write_text(self.xml, encoding='utf-8')
        data.write_text(json.dumps(self.data), encoding='utf-8')
        with contextlib.redirect_stdout(io.StringIO()):
            renderer.render(xml, data, output, receipt=True, official_pdf=self.official if official else None)
        return output

    def test_receipt_preserves_number_validation_and_width(self):
        output = self.render()
        with pymupdf.open(output) as document:
            self.assertEqual(len(document), 1)
            self.assertAlmostEqual(document[0].rect.width * 25.4 / 72, 80, places=2)
            text = document[0].get_text()
            self.assertIn('FE-TEST-1', text)
            self.assertIn('2026-01-01 12:00:01-05:00', text)
            self.assertNotIn('PARA REVISI', text)
            for word in document[0].get_text('words'):
                self.assertTrue(document[0].rect.contains(pymupdf.Rect(word[:4])), word)

    def test_reject_cufe_mismatch(self):
        self.data['stamp']['cufe'] = 'b' * 96
        with self.assertRaisesRegex(ValueError, 'CUFE'):
            self.render()

    def test_reject_total_mismatch(self):
        self.data['total'] = 101
        with self.assertRaisesRegex(ValueError, 'Total'):
            self.render()

    def test_reject_number_mismatch(self):
        self.data['numberTemplate']['fullNumber'] = 'OTHER'
        with self.assertRaisesRegex(ValueError, 'Numero'):
            self.render()

    def test_reject_pending_invoice(self):
        self.data['stamp']['legalStatus'] = 'STAMPED_AND_WAITING_RESPONSE'
        with self.assertRaisesRegex(ValueError, 'aceptacion'):
            self.render()

    def test_reject_qr_for_another_invoice(self):
        self.xml = self.xml.replace('https://example.invalid/?cufe=' + self.cufe, 'https://example.invalid/?cufe=wrong')
        with self.assertRaisesRegex(ValueError, 'QR'):
            self.render()

    def test_reject_missing_official_validation_metadata(self):
        with self.assertRaisesRegex(ValueError, 'validacion/software'):
            self.render(official=False)

    def test_reject_official_pdf_for_another_invoice(self):
        document = canvas.Canvas(str(self.official))
        document.drawString(20, 800, 'OTHER INVOICE')
        document.save()
        with self.assertRaisesRegex(ValueError, 'PDF oficial'):
            self.render()


if __name__ == '__main__':
    unittest.main()
