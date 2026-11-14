-- 09 - Domicilios: forma de pago elegida al tomar el pedido.
--
-- Motivo (2026-11-14): al cumplirse el tiempo de preparacion un domicilio pasa solo a FACTURADO y el
-- escritorio registra la venta. Para eso necesita saber como se paga: se pregunta al crear el pedido
-- (EFECTIVO, TRANSFERENCIA o TARJETA) y se guarda en Pedidos.Payment_method.
--
-- Que hace (idempotente: repetirlo no cambia nada):
--   Agrega Pedidos.Payment_method varchar(20) NULL. NULL en pedidos de mesa y en domicilios anteriores
--   a este cambio; esos no se facturan solos y siguen con el boton Facturar.
--
-- Reversion: 09_pedidos_forma_pago.revert.sql
--
-- Uso (primero contra la copia local):
--   cd db
--   .\Invoke-Sql.ps1 -File .\09_pedidos_forma_pago.sql

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.Pedidos', 'Payment_method') IS NULL
    ALTER TABLE dbo.Pedidos ADD Payment_method varchar(20) NULL;
GO

PRINT '09: Pedidos.Payment_method listo.';
GO
