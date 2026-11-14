-- 09_pedidos_forma_pago.revert.sql
-- Quita Pedidos.Payment_method. Se pierde la forma de pago guardada en los domicilios ya creados;
-- las ventas ya registradas no se tocan.
SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.Pedidos', 'Payment_method') IS NOT NULL
    ALTER TABLE dbo.Pedidos DROP COLUMN Payment_method;
GO
