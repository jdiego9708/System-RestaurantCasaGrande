-- ATENCION: elimina la numeracion guardada. Hacer copia de seguridad antes de revertir.
DROP TRIGGER IF EXISTS dbo.tr_Ventas_AssignReceiptNumber;
DROP TABLE IF EXISTS dbo.SaleReceiptNumbers;
DROP TABLE IF EXISTS dbo.SaleReceiptCounters;
GO
