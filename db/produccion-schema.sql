USE [CasaGrandeBD]
GO
/****** Object:  UserDefinedTableType [dbo].[tp_Detalle_pago]    Script Date: 24/10/2026 7:36:27 p. m. ******/
CREATE TYPE [dbo].[tp_Detalle_pago] AS TABLE(
	[Metodo_pago] [varchar](50) NULL,
	[Valor_pago] [int] NULL,
	[Vaucher] [varchar](50) NULL,
	[Observaciones] [varchar](150) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[tp_Detalle_pedido_modificado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
CREATE TYPE [dbo].[tp_Detalle_pedido_modificado] AS TABLE(
	[Id_tipo] [int] NULL,
	[Tipo] [varchar](50) NULL,
	[Precio] [int] NULL,
	[Cantidad] [int] NULL,
	[Observaciones] [varchar](100) NULL
)
GO
/****** Object:  Table [dbo].[Bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bebidas](
	[Id_bebida] [int] IDENTITY(500,2) NOT NULL,
	[Nombre_bebida] [varchar](200) NULL,
	[Descripcion_bebida] [varchar](200) NULL,
	[Precio_bebida] [decimal](19, 2) NULL,
	[Precio_trago] [decimal](19, 2) NULL,
	[Precio_trago_doble] [decimal](19, 2) NULL,
	[Precio_proveedor] [decimal](19, 2) NULL,
	[Id_proveedor] [int] NULL,
	[Imagen] [varchar](max) NOT NULL,
	[Id_tipo_bebida] [int] NULL,
	[Cantidad_unidades] [int] NULL,
	[Cantidad_x_unidad] [int] NULL,
	[Cantidad_total] [int] NULL,
	[Estado] [varchar](50) NULL,
 CONSTRAINT [PK__Bebidas__9BC494176A506F21] PRIMARY KEY CLUSTERED 
(
	[Id_bebida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Clientes]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Clientes](
	[Id_cliente] [int] IDENTITY(101010,2) NOT NULL,
	[Nombre_cliente] [varchar](500) NOT NULL,
	[Telefono_cliente] [varchar](50) NULL,
	[Correo_electronico] [varchar](100) NULL,
	[Direccion_cliente] [varchar](100) NULL,
	[Referencia_ubicacion] [varchar](200) NULL,
	[Otras_observaciones] [varchar](200) NULL,
	[Estado_cliente] [varchar](50) NULL,
 CONSTRAINT [PK__Clientes__FCE0399296FF6CFC] PRIMARY KEY CLUSTERED 
(
	[Id_cliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Delete_pedidos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Delete_pedidos](
	[Id_pedido] [int] NOT NULL,
	[Id_usuario_clave_maestra] [int] NOT NULL,
	[Id_usuario_sesion] [int] NOT NULL,
	[Id_tipo] [int] NOT NULL,
	[Tipo] [varchar](50) NOT NULL,
	[Fecha] [date] NOT NULL,
	[Hora] [time](2) NOT NULL,
	[Motivo] [varchar](500) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Detalle_bebida]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Detalle_bebida](
	[Id_detalle] [int] IDENTITY(1,10) NOT NULL,
	[Id_bebida] [int] NOT NULL,
	[Tipo] [varchar](50) NOT NULL,
	[Id_tipo] [int] NOT NULL,
	[Tipo_cantidad] [varchar](50) NOT NULL,
	[Cantidad_usada] [int] NOT NULL,
 CONSTRAINT [PK_Detalle_bebida] PRIMARY KEY CLUSTERED 
(
	[Id_detalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Detalle_ingredientes_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Detalle_ingredientes_pedido](
	[Id_detalle_ingrediente_pedido] [int] IDENTITY(1,2) NOT NULL,
	[Id_pedido] [int] NOT NULL,
	[Id_tipo] [int] NOT NULL,
	[Id_ingrediente] [int] NOT NULL,
	[Id_detalle_pedido] [int] NULL,
	[Observaciones] [varchar](200) NULL,
 CONSTRAINT [PK_Detalle_ingredientes_pedido] PRIMARY KEY CLUSTERED 
(
	[Id_detalle_ingrediente_pedido] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Detalle_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Detalle_pedido](
	[Id_detalle_pedido] [int] IDENTITY(1,1) NOT NULL,
	[Id_pedido] [int] NOT NULL,
	[Id_tipo] [int] NOT NULL,
	[Tipo] [varchar](50) NOT NULL,
	[Precio] [decimal](19, 2) NOT NULL,
	[Cantidad] [int] NOT NULL,
	[Observaciones] [varchar](100) NULL,
 CONSTRAINT [PK_Detalle_pedido] PRIMARY KEY CLUSTERED 
(
	[Id_detalle_pedido] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Detalle_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Detalle_platos](
	[Id_detalle] [int] IDENTITY(1,2) NOT NULL,
	[Id_plato] [int] NOT NULL,
	[Id_insumo] [int] NOT NULL,
	[Cantidad_insumo] [int] NOT NULL,
 CONSTRAINT [PK_Detalle_platos_bebidas] PRIMARY KEY CLUSTERED 
(
	[Id_detalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Detalle_venta]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Detalle_venta](
	[Id_venta] [int] NOT NULL,
	[Metodo_pago] [varchar](50) NOT NULL,
	[Valor_pago] [int] NOT NULL,
	[Vaucher] [varchar](50) NULL,
	[Observaciones] [varchar](150) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Egresos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Egresos](
	[Id_egreso] [int] IDENTITY(7070,2) NOT NULL,
	[Id_empleado] [int] NOT NULL,
	[Fecha_egreso] [date] NOT NULL,
	[Valor_egreso] [decimal](19, 2) NOT NULL,
	[Descripcion_egreso] [varchar](200) NULL,
	[Estado_egreso] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Egresos] PRIMARY KEY CLUSTERED 
(
	[Id_egreso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Empleado_propinas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empleado_propinas](
	[Id_propina] [int] IDENTITY(1,1) NOT NULL,
	[Id_venta] [int] NOT NULL,
	[Estado_propina] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Empleado_propinas] PRIMARY KEY CLUSTERED 
(
	[Id_propina] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Empleados]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Empleados](
	[Id_empleado] [int] IDENTITY(6060,5) NOT NULL,
	[Nombre_empleado] [varchar](500) NOT NULL,
	[Telefono_empleado] [varchar](50) NULL,
	[Identificacion_empleado] [varchar](50) NULL,
	[Correo_electronico] [varchar](500) NULL,
	[Cargo_empleado] [varchar](50) NULL,
	[Password] [varchar](50) NULL,
	[Codigo_maestro] [int] NULL,
	[Estado] [varchar](50) NULL,
 CONSTRAINT [PK__Empleado__01AC2829F7657700] PRIMARY KEY CLUSTERED 
(
	[Id_empleado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ingredientes]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ingredientes](
	[Id_ingrediente] [int] IDENTITY(1010,2) NOT NULL,
	[Tipo_ingrediente] [varchar](50) NOT NULL,
	[Nombre_ingrediente] [varchar](50) NOT NULL,
	[Descripcion_ingrediente] [varchar](200) NULL,
	[Estado_ingrediente] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Ingredientes_almuerzo] PRIMARY KEY CLUSTERED 
(
	[Id_ingrediente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ingresos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ingresos](
	[Id_ingreso] [int] IDENTITY(6060,2) NOT NULL,
	[Id_empleado] [int] NOT NULL,
	[Fecha_ingreso] [date] NOT NULL,
	[Valor_ingreso] [decimal](19, 2) NOT NULL,
	[Descripcion_ingreso] [varchar](200) NULL,
	[Estado_ingreso] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Ingresos] PRIMARY KEY CLUSTERED 
(
	[Id_ingreso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Insumos](
	[Id_insumo] [int] IDENTITY(1500,2) NOT NULL,
	[Nombre_insumo] [varchar](50) NOT NULL,
	[Id_tipo_insumo] [int] NOT NULL,
	[Cantidad_existente] [int] NOT NULL,
	[Tipo_medida] [varchar](50) NOT NULL,
	[Observaciones] [varchar](500) NULL,
 CONSTRAINT [PK_Insumos] PRIMARY KEY CLUSTERED 
(
	[Id_insumo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Mesas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Mesas](
	[Id_mesa] [int] IDENTITY(5050,5) NOT NULL,
	[Num_mesa] [int] NOT NULL,
	[Descripcion_mesa] [varchar](500) NULL,
 CONSTRAINT [PK__Mesas__E1D8BC6660E5395C] PRIMARY KEY CLUSTERED 
(
	[Id_mesa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Nomina_empleado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Nomina_empleado](
	[Id_nomina_empleado] [int] IDENTITY(5050,2) NOT NULL,
	[Id_empleado] [int] NOT NULL,
	[Fecha_nomina] [date] NOT NULL,
	[Turno] [decimal](19, 2) NOT NULL,
	[Servicios] [decimal](19, 2) NOT NULL,
	[Platos] [decimal](19, 2) NULL,
	[Otros_ingresos] [decimal](19, 2) NULL,
	[Egresos] [decimal](19, 2) NOT NULL,
	[Total_nomina] [decimal](19, 2) NOT NULL,
	[Estado_nomina] [varchar](50) NOT NULL,
	[Observaciones] [varchar](500) NULL,
 CONSTRAINT [PK_Nomina_empleado] PRIMARY KEY CLUSTERED 
(
	[Id_nomina_empleado] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notas_empleado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notas_empleado](
	[Id_nota] [int] IDENTITY(3030,2) NOT NULL,
	[Id_empleado] [int] NOT NULL,
	[Fecha_nota] [date] NOT NULL,
	[Hora_nota] [time](2) NOT NULL,
	[Titulo_nota] [varchar](50) NULL,
	[Descripcion_nota] [varchar](200) NULL,
 CONSTRAINT [PK_Notas_empleado] PRIMARY KEY CLUSTERED 
(
	[Id_nota] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pedidos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pedidos](
	[Id_pedido] [int] IDENTITY(1010,2) NOT NULL,
	[Id_mesa] [int] NOT NULL,
	[Id_empleado] [int] NOT NULL,
	[Id_cliente] [int] NOT NULL,
	[Estado_pedido] [varchar](50) NOT NULL,
	[Fecha_pedido] [date] NOT NULL,
	[Hora_pedido] [time](2) NOT NULL,
	[Tipo_pedido] [varchar](50) NULL,
	[Observaciones_pedido] [varchar](200) NULL,
	[CantidadClientes] [int] NULL,
 CONSTRAINT [PK__Pedidos__1E657E9ECAE702AA] PRIMARY KEY CLUSTERED 
(
	[Id_pedido] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Platos](
	[Id_plato] [int] IDENTITY(80808,1) NOT NULL,
	[Nombre_plato] [varchar](200) NULL,
	[Id_tipo_plato] [int] NULL,
	[Precio_plato] [decimal](19, 2) NULL,
	[Imagen_plato] [varchar](max) NULL,
	[Descripcion_plato] [varchar](500) NULL,
	[Estado] [varchar](50) NULL,
	[Plato_detallado] [varchar](50) NULL,
	[Plato_carta] [varchar](50) NULL,
 CONSTRAINT [PK__Platos__E3B629D9B6BB8CDA] PRIMARY KEY CLUSTERED 
(
	[Id_plato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reservas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reservas](
	[Id_reserva] [int] IDENTITY(6060,2) NOT NULL,
	[Id_mesa] [int] NOT NULL,
	[Id_cliente] [int] NOT NULL,
	[Fecha] [date] NOT NULL,
	[Hora] [time](2) NOT NULL,
	[Observaciones] [varchar](50) NULL,
	[Estado] [varchar](50) NULL,
 CONSTRAINT [PK_Reservas] PRIMARY KEY CLUSTERED 
(
	[Id_reserva] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tipo_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tipo_bebidas](
	[Id_tipo_bebida] [int] IDENTITY(2020,2) NOT NULL,
	[Tipo_bebida] [varchar](50) NULL,
 CONSTRAINT [PK_Tipo_bebidas] PRIMARY KEY CLUSTERED 
(
	[Id_tipo_bebida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tipo_insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tipo_insumos](
	[Id_tipo_insumo] [int] IDENTITY(50,10) NOT NULL,
	[Tipo_insumo] [varchar](50) NULL,
 CONSTRAINT [PK_Tipo_insumos] PRIMARY KEY CLUSTERED 
(
	[Id_tipo_insumo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tipo_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tipo_platos](
	[Id_tipo_plato] [int] IDENTITY(8080,2) NOT NULL,
	[Tipo_plato] [varchar](500) NOT NULL,
 CONSTRAINT [PK_Tipo_platos] PRIMARY KEY CLUSTERED 
(
	[Id_tipo_plato] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Turnos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Turnos](
	[Id_turno] [int] IDENTITY(2020,2) NOT NULL,
	[Fecha_turno] [date] NOT NULL,
	[Valor_inicial] [decimal](19, 2) NOT NULL,
	[Total_ingresos] [decimal](19, 2) NOT NULL,
	[Total_egresos] [decimal](19, 2) NULL,
	[Total_ventas] [decimal](19, 2) NULL,
	[Total_nomina] [decimal](19, 2) NULL,
	[Total_turno] [decimal](19, 2) NOT NULL,
	[Estado_turno] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Turnos] PRIMARY KEY CLUSTERED 
(
	[Id_turno] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Ventas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ventas](
	[Id_venta] [int] IDENTITY(6060,2) NOT NULL,
	[Id_pedido] [int] NOT NULL,
	[Fecha_venta] [date] NOT NULL,
	[Hora_venta] [time](2) NOT NULL,
	[Total_parcial] [int] NOT NULL,
	[Propina] [int] NOT NULL,
	[Subtotal] [int] NOT NULL,
	[Descuento] [int] NULL,
	[Bono_cupon] [varchar](50) NULL,
	[Desechables] [decimal](19, 2) NOT NULL,
	[Domicilio] [decimal](19, 2) NOT NULL,
	[Total_final] [int] NOT NULL,
	[Observaciones] [varchar](500) NULL,
	[Estado] [varchar](50) NULL,
 CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED 
(
	[Id_venta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Ventas] ADD  CONSTRAINT [DF_Ventas_Estado]  DEFAULT ('ACTIVO') FOR [Estado]
GO
ALTER TABLE [dbo].[Egresos]  WITH CHECK ADD  CONSTRAINT [FK_Egresos_Empleados] FOREIGN KEY([Id_empleado])
REFERENCES [dbo].[Empleados] ([Id_empleado])
GO
ALTER TABLE [dbo].[Egresos] CHECK CONSTRAINT [FK_Egresos_Empleados]
GO
ALTER TABLE [dbo].[Ingresos]  WITH CHECK ADD  CONSTRAINT [FK_Ingresos_Empleados] FOREIGN KEY([Id_empleado])
REFERENCES [dbo].[Empleados] ([Id_empleado])
GO
ALTER TABLE [dbo].[Ingresos] CHECK CONSTRAINT [FK_Ingresos_Empleados]
GO
ALTER TABLE [dbo].[Nomina_empleado]  WITH CHECK ADD  CONSTRAINT [FK_Nomina_empleado_Nomina_empleado] FOREIGN KEY([Id_empleado])
REFERENCES [dbo].[Empleados] ([Id_empleado])
GO
ALTER TABLE [dbo].[Nomina_empleado] CHECK CONSTRAINT [FK_Nomina_empleado_Nomina_empleado]
GO
/****** Object:  StoredProcedure [dbo].[sp_Actualizar_acompanante]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[sp_Actualizar_acompanante]
@Id_ingrediente int,
@Nombre_ingrediente varchar(50)
AS
BEGIN
	UPDATE Ingredientes SET
	Nombre_ingrediente = @Nombre_ingrediente
	WHERE Id_ingrediente = @Id_ingrediente
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Actualizar_detalle_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Actualizar_detalle_pedido]
@Id_detalle_pedidoSalida int output,
@Id_detalle_pedido int,
@Id_pedido int,
@Id_tipo int,
@Tipo varchar(50),
@Precio int,
@Cantidad int,
@Observaciones varchar(500),
@Tipo_update varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION ActualizarPedido
BEGIN

IF (@Id_detalle_pedido > 0)
BEGIN
IF @Cantidad = 0
	BEGIN
	DELETE Detalle_pedido
	WHERE Id_detalle_pedido = @Id_detalle_pedido
	END
ELSE
	DECLARE @Cantidad_anterior int
	SELECT @Cantidad_anterior = Cantidad
	FROM Detalle_pedido 
	WHERE Id_detalle_pedido = @Id_detalle_pedido

	IF (@Cantidad_anterior > @Cantidad)
	BEGIN
		SET @Tipo_update = 'DELETE'
	END
	ELSE
	BEGIN
		SET @Tipo_update = 'ADD'
	END

	IF @Tipo_update = 'DELETE'
	BEGIN
		UPDATE Detalle_pedido
		SET
		Tipo = @Tipo,
		Precio = @Precio,
		Cantidad -= 1,
		Observaciones = @Observaciones
		WHERE Id_detalle_pedido = @Id_detalle_pedido
	END
	ELSE
	BEGIN
		UPDATE Detalle_pedido
		SET
		Tipo = @Tipo,
		Precio = @Precio,
		Cantidad += @Cantidad,
		Observaciones = @Observaciones
		WHERE Id_detalle_pedido = @Id_detalle_pedido
	END
END
ELSE
BEGIN
	IF @Tipo_update != 'DELETE'
	BEGIN
		INSERT INTO Detalle_pedido
		VALUES (@Id_pedido, @Id_tipo, @Tipo,@Precio, @Cantidad, @Observaciones)

		SET @Id_detalle_pedidoSalida = SCOPE_IDENTITY();
	END
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Base_turno_u]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE      PROC [dbo].[sp_Base_turno_u]
@Id_turno int,
@Base_nueva decimal(19,2)
AS
BEGIN
	UPDATE Turnos SET
	Valor_inicial = @Base_nueva
	WHERE Id_turno = @Id_turno
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_bebidas]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 Tipo_bebidas.Tipo_bebida, Bebidas.*
FROM Bebidas, Tipo_bebidas
WHERE Bebidas.Id_tipo_bebida = Tipo_bebidas.Id_tipo_bebida
and Bebidas.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'ID BEBIDA'
BEGIN
SELECT Tipo_bebidas.Tipo_bebida, Bebidas.*
FROM Bebidas, Tipo_bebidas
WHERE Bebidas.Id_tipo_bebida = Tipo_bebidas.Id_tipo_bebida and
Bebidas.Id_bebida = @Texto_busqueda
and Bebidas.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'TIPO BEBIDA'
BEGIN
SELECT Tipo_bebidas.Tipo_bebida, Bebidas.*
FROM Bebidas, Tipo_bebidas
WHERE Bebidas.Id_tipo_bebida = Tipo_bebidas.Id_tipo_bebida
and Tipo_bebidas.Tipo_bebida = @Texto_busqueda
and Bebidas.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'NOMBRE'
BEGIN
SELECT Tipo_bebidas.Tipo_bebida, Bebidas.*
FROM Bebidas, Tipo_bebidas
WHERE Bebidas.Id_tipo_bebida = Tipo_bebidas.Id_tipo_bebida
and Bebidas.Nombre_bebida like '%'+@Texto_busqueda+'%'
and Bebidas.Estado = 'ACTIVO'
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_clientes]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Buscar_clientes]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 * FROM Clientes
END
ELSE IF @Tipo_busqueda = 'BUSQUEDA COMPLETA'
BEGIN
SELECT * FROM Clientes
WHERE Nombre_cliente like '%' + @Texto_busqueda + '%' OR
Telefono_cliente like '%' + @Texto_busqueda + '%'
END
ELSE IF @Tipo_busqueda = 'ID CLIENTE'
BEGIN
SELECT * FROM Clientes
WHERE Id_cliente = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'ID CLIENTE Y VENTA'
BEGIN
--OBTENER CANTIDAD DE PEDIDOS
DECLARE @Cantidad_pedidos int
SELECT @Cantidad_pedidos = COUNT (pedidos.Id_pedido)
FROM Pedidos pedidos
WHERE pedidos.Id_cliente = @Texto_busqueda and
pedidos.Estado_pedido = 'TERMINADO'

SELECT TOP 1 clientes.*, @Cantidad_pedidos as CantidadPedidos
FROM Clientes clientes
WHERE clientes.Id_cliente = @Texto_busqueda
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_delete_pedidos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_delete_pedidos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50),
@Fecha date
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'FECHA'
	BEGIN
		SELECT DISTINCT dp.Id_pedido, dp.Id_usuario_clave_maestra, dp.Id_tipo, dp.Tipo,
		dp.Fecha, CONVERT(varchar(5), dp.Hora) as Hora, dp.Motivo,
		 (CASE dp.Tipo WHEN 'PLATO' THEN pl.Nombre_plato 
		WHEN 'BEBIDA' THEN be.Nombre_bebida ELSE '' END) AS Nombre,
		(CASE dp.Tipo WHEN 'PLATO' THEN pl.Precio_plato 
		WHEN 'BEBIDA' THEN be.Precio_bebida ELSE '' END) AS Precio,
		em.Id_empleado, em.Nombre_empleado, em.Cargo_empleado,
		em.Telefono_empleado
		FROM Delete_pedidos dp INNER JOIN Empleados em
			ON dp.Id_usuario_clave_maestra = em.Id_empleado,
			Platos pl, Bebidas be
		WHERE
		(dp.Id_tipo = pl.Id_plato OR
		dp.Id_tipo = be.Id_bebida) and
		 Fecha = CONVERT(date, @Fecha) 

		union

		SELECT DISTINCT dp.Id_pedido, dp.Id_usuario_sesion, dp.Id_tipo, dp.Tipo,
		dp.Fecha, CONVERT(varchar(5), dp.Hora) as Hora, dp.Motivo,
		 (CASE dp.Tipo WHEN 'PLATO' THEN pl.Nombre_plato 
		WHEN 'BEBIDA' THEN be.Nombre_bebida ELSE '' END) AS Nombre,
		(CASE dp.Tipo WHEN 'PLATO' THEN pl.Precio_plato 
		WHEN 'BEBIDA' THEN be.Precio_bebida ELSE '' END) AS Precio,
		em.Id_empleado, em.Nombre_empleado, em.Cargo_empleado,
		em.Telefono_empleado
		FROM Delete_pedidos dp INNER JOIN Empleados em
			ON dp.Id_usuario_sesion = em.Id_empleado,
			Platos pl, Bebidas be
		WHERE
		(dp.Id_tipo = pl.Id_plato OR
		dp.Id_tipo = be.Id_bebida) and
		 Fecha = CONVERT(date, @Fecha) 
		ORDER BY Fecha DESC
	END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_detalle_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_detalle_platos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'ID PLATO'
BEGIN
SELECT det.Id_detalle, det.Id_plato, det.Id_insumo, ins.Nombre_insumo, det.Cantidad_insumo,
(Convert(varchar(50),det.Cantidad_insumo) + ' ' + ins.Tipo_medida) as Cantidad, ins.Id_tipo_insumo, tipo.Tipo_insumo
FROM Detalle_platos det, Platos pl, Insumos ins, Tipo_insumos tipo
WHERE det.Id_plato = pl.Id_plato and
det.Id_insumo = ins.Id_insumo and
ins.Id_tipo_insumo = tipo.Id_tipo_insumo and
det.Id_plato = @Texto_busqueda
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_egreso]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_egreso]
@Tipo_busqueda varchar(50),
@Texto_busqueda1 varchar(50),
@Texto_busqueda2 varchar(50)
AS
BEGIN TRY
IF (@Tipo_busqueda = 'FECHA')
	BEGIN
		SELECT *
		FROM Egresos eg 
		INNER JOIN Empleados em ON eg.Id_empleado = em.Id_empleado
		WHERE eg.Fecha_egreso = @Texto_busqueda1
	END
ELSE IF (@Tipo_busqueda = 'RANGO FECHAS')
	BEGIN
		SELECT *
		FROM Egresos eg 
		INNER JOIN Empleados em ON eg.Id_empleado = em.Id_empleado
		WHERE eg.Fecha_egreso BETWEEN @Texto_busqueda1 AND @Texto_busqueda2
	END
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_empleado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROC [dbo].[sp_Buscar_empleado]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 * 
FROM Empleados
ORDER BY Id_empleado DESC
END
ELSE IF @Tipo_busqueda = 'BUSQUEDA COMPLETA'
BEGIN
SELECT TOP 50 * 
FROM Empleados
WHERE Nombre_empleado like '%' + @Texto_busqueda + '%' OR
Telefono_empleado like '%' + @Texto_busqueda + '%'
ORDER BY Id_empleado DESC
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_ingredientes]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_ingredientes]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
	IF @Tipo_busqueda = 'ID INGREDIENTE'
	BEGIN
		SELECT *
		FROM Ingredientes
		WHERE Id_ingrediente = CONVERT(int, @Texto_busqueda)
	END
	ELSE IF @Tipo_busqueda = 'TIPO'
	BEGIN
		SELECT *
		FROM Ingredientes
		WHERE Tipo_ingrediente = @Texto_busqueda
	END
	ELSE IF @Tipo_busqueda = 'COMPLETO'
	BEGIN
		SELECT *
		FROM Ingredientes
	END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_ingreso]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_ingreso]
@Tipo_busqueda varchar(50),
@Texto_busqueda1 varchar(50),
@Texto_busqueda2 varchar(50)
AS
BEGIN TRY
IF (@Tipo_busqueda = 'FECHA')
	BEGIN
		SELECT *
		FROM Ingresos eg 
		INNER JOIN Empleados em ON eg.Id_empleado = em.Id_empleado
		WHERE eg.Fecha_ingreso = CONVERT(date, @Texto_busqueda1)
	END
ELSE IF (@Tipo_busqueda = 'RANGO FECHAS')
	BEGIN
		SELECT *
		FROM Ingresos eg 
		INNER JOIN Empleados em ON eg.Id_empleado = em.Id_empleado
		WHERE eg.Fecha_ingreso BETWEEN CONVERT(date, @Texto_busqueda1)
		and CONVERT(date, @Texto_busqueda2)
	END
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_insumos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 insumos.Id_insumo, insumos.Nombre_insumo, 
insumos.Id_tipo_insumo, tipo.Tipo_insumo,
insumos.Cantidad_existente, insumos.Tipo_medida, insumos.Observaciones
FROM Insumos insumos, Tipo_insumos tipo
WHERE insumos.Id_tipo_insumo = tipo.Id_tipo_insumo
END
ELSE IF @Tipo_busqueda = 'NOMBRE'
BEGIN
SELECT TOP 50 insumos.Id_insumo, insumos.Nombre_insumo, 
insumos.Id_tipo_insumo, tipo.Tipo_insumo,
insumos.Cantidad_existente, insumos.Tipo_medida, insumos.Observaciones
FROM Insumos insumos, Tipo_insumos tipo
WHERE insumos.Id_tipo_insumo = tipo.Id_tipo_insumo and
insumos.Nombre_insumo like '%' + @Texto_busqueda + '%'
END
ELSE IF @Tipo_busqueda = 'TIPO'
BEGIN
SELECT TOP 50 insumos.Id_insumo, insumos.Nombre_insumo, 
insumos.Id_tipo_insumo, tipo.Tipo_insumo,
insumos.Cantidad_existente, insumos.Tipo_medida, insumos.Observaciones
FROM Insumos insumos, Tipo_insumos tipo
WHERE insumos.Id_tipo_insumo = tipo.Id_tipo_insumo and
tipo.Tipo_insumo = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'ID'
BEGIN
SELECT TOP 50 insumos.Id_insumo, insumos.Nombre_insumo, 
insumos.Id_tipo_insumo, tipo.Tipo_insumo,
insumos.Cantidad_existente, insumos.Tipo_medida, insumos.Observaciones
FROM Insumos insumos, Tipo_insumos tipo
WHERE insumos.Id_tipo_insumo = tipo.Id_tipo_insumo and
insumos.Id_insumo = @Texto_busqueda
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_mesas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_mesas]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 * FROM Mesas
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_pedidos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_pedidos]
@Tipo_busqueda varchar(50),
@Texto_busqueda1 varchar(50),
@Texto_busqueda2 varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'ID PEDIDO'
BEGIN
SELECT TOP 50 pedidos.Id_pedido, pedidos.Fecha_pedido, pedidos.Hora_pedido, 
pedidos.Id_mesa, 
(CASE WHEN mesas.Num_mesa = 0
THEN 'DOMICILIO' ELSE 'MESA: '+ CONVERT(varchar(50),mesas.Num_mesa) END) as Mesa,
pedidos.Id_empleado, empleados.Nombre_empleado, pedidos.Id_cliente, 
(clientes.Nombre_cliente + ' - Teléfono: ' + clientes.Telefono_cliente +
CASE WHEN clientes.Direccion_cliente IS NOT NULL 
THEN ' - Dirección: ' + clientes.Direccion_cliente ELSE '' END +
CASE WHEN clientes.Referencia_ubicacion IS NOT NULL 
THEN ' - Referencia: ' + clientes.Referencia_ubicacion ELSE '' END) as Cliente, mesas.Num_mesa,
pedidos.Observaciones_pedido
FROM Pedidos pedidos, Mesas mesas, 
Empleados empleados, Clientes clientes
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Id_empleado = empleados.Id_empleado and
pedidos.Id_cliente = clientes.Id_cliente and
pedidos.Id_pedido = @Texto_busqueda1
END
ELSE IF @Tipo_busqueda = 'ID PEDIDO Y DETALLE'
BEGIN
SELECT pedidos.Id_pedido, pedidos.Fecha_pedido, pedidos.Hora_pedido, 
pedidos.Id_mesa, 
(CASE WHEN mesas.Num_mesa = 0
THEN 'DOMICILIO' ELSE 'MESA: '+ CONVERT(varchar(50),mesas.Num_mesa) END) as Mesa,
pedidos.Id_empleado, empleados.Nombre_empleado, pedidos.Id_cliente, 
(clientes.Nombre_cliente + ' - Teléfono: ' + clientes.Telefono_cliente +
CASE WHEN clientes.Direccion_cliente IS NOT NULL 
THEN ' - Dirección: ' + clientes.Direccion_cliente ELSE '' END +
CASE WHEN clientes.Referencia_ubicacion IS NOT NULL 
THEN ' - Referencia: ' + clientes.Referencia_ubicacion ELSE '' END) as Cliente, mesas.Num_mesa,
pedidos.Observaciones_pedido
FROM Pedidos pedidos, Mesas mesas, 
Empleados empleados, Clientes clientes
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Id_empleado = empleados.Id_empleado and
pedidos.Id_cliente = clientes.Id_cliente and
pedidos.Id_pedido = CONVERT(int, @Texto_busqueda1)

SELECT de.Id_detalle_pedido, de.Id_pedido, de.Id_tipo,
de.Tipo, pl.Nombre_plato AS Nombre,
de.Precio, de.Cantidad, (de.Precio * de.Cantidad) AS Total, 
de.Observaciones
FROM Detalle_pedido de
INNER JOIN Platos pl ON de.Id_tipo = pl.Id_plato
WHERE de.Id_pedido = CONVERT(int, @Texto_busqueda1)
UNION
SELECT de.Id_detalle_pedido, de.Id_pedido, de.Id_tipo,
de.Tipo, be.Nombre_bebida AS Nombre,
de.Precio, de.Cantidad, (de.Precio * de.Cantidad) AS Total, 
de.Observaciones 
FROM Detalle_pedido de
INNER JOIN Bebidas be ON de.Id_tipo = be.Id_bebida
WHERE de.Id_pedido = CONVERT(int, @Texto_busqueda1)

SELECT *
FROM Pedidos pe 
INNER JOIN Detalle_ingredientes_pedido dein ON pe.Id_pedido = dein.Id_pedido
INNER JOIN Ingredientes ing ON dein.Id_ingrediente = ing.Id_ingrediente
WHERE pe.Id_pedido = CONVERT(int, @Texto_busqueda1)

END
ELSE IF @Tipo_busqueda = 'FECHA'
BEGIN
SELECT TOP 50 pedidos.Id_pedido, pedidos.Fecha_pedido, pedidos.Hora_pedido, 
pedidos.Id_mesa, 
(CASE WHEN mesas.Num_mesa = 0
THEN 'DOMICILIO' ELSE 'MESA: '+ CONVERT(varchar(50),mesas.Num_mesa) END) as Mesa, 
pedidos.Id_empleado, empleados.Nombre_empleado, pedidos.Id_cliente,
(clientes.Nombre_cliente + ' - Teléfono: ' + clientes.Telefono_cliente)Cliente, mesas.Num_mesa,
pedidos.*
FROM Pedidos pedidos, Mesas mesas, 
Empleados empleados, Clientes clientes
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Id_empleado = empleados.Id_empleado and
pedidos.Id_cliente = clientes.Id_cliente and
pedidos.Fecha_pedido = @Texto_busqueda1
END
ELSE IF @Tipo_busqueda = 'RANGO FECHAS'
BEGIN
SELECT TOP 50 pedidos.Id_pedido, pedidos.Fecha_pedido, pedidos.Hora_pedido, 
pedidos.Id_mesa, 
(CASE WHEN mesas.Num_mesa = 0
THEN 'DOMICILIO' ELSE 'MESA: '+ CONVERT(varchar(50),mesas.Num_mesa) END) as Mesa, 
pedidos.Id_empleado, empleados.Nombre_empleado, pedidos.Id_cliente,
(clientes.Nombre_cliente + ' - Teléfono: ' + clientes.Telefono_cliente)Cliente, mesas.Num_mesa,
pedidos.*
FROM Pedidos pedidos, Mesas mesas, 
Empleados empleados, Clientes clientes
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Id_empleado = empleados.Id_empleado and
pedidos.Id_cliente = clientes.Id_cliente and
pedidos.Fecha_pedido BETWEEN @Texto_busqueda1 AND @Texto_busqueda2
END
ELSE IF @Tipo_busqueda = 'DETALLE PEDIDO'
BEGIN
SELECT DISTINCT TOP 50 detalle.Id_pedido, detalle.Id_tipo,
detalle.Tipo, (CASE detalle.Tipo WHEN 'PLATO' THEN platos.Nombre_plato 
WHEN 'BEBIDA' THEN bebidas.Nombre_bebida ELSE '' END) AS Nombre, detalle.Precio, detalle.Cantidad,
(detalle.Precio * detalle.Cantidad) AS Total,
detalle.Observaciones
FROM Detalle_pedido detalle, 
Platos platos, Bebidas bebidas
WHERE (detalle.Id_tipo = platos.Id_plato OR
detalle.Id_tipo = bebidas.Id_bebida) and 
detalle.Id_pedido = @Texto_busqueda1
END
ELSE IF @Tipo_busqueda = 'MESAS CON PEDIDO'
BEGIN
SELECT TOP 50 *
FROM Pedidos pedidos, Mesas mesas
WHERE pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Fecha_pedido = CONVERT(date, @Texto_busqueda1) AND
pedidos.Estado_pedido IN ('SALIENDO', 'PENDIENTE') and
--pedidos.Estado_pedido = 'PENDIENTE' OR
--pedidos.Estado_pedido = 'SALIENDO' and
pedidos.Tipo_pedido != 'DOMICILIO' and
pedidos.Tipo_pedido != 'TERMINADO'
ORDER BY pedidos.Id_pedido DESC
END
ELSE IF @Tipo_busqueda = 'MESAS COMPLETAS'
BEGIN
SELECT TOP 50 *
FROM Mesas mesas
END
ELSE IF @Tipo_busqueda = 'DOMICILIOS PENDIENTES'
BEGIN
SELECT * 
FROM Pedidos pe
INNER JOIN Mesas me ON pe.Id_mesa = me.Id_mesa
INNER JOIN Empleados em ON pe.Id_empleado = em.Id_empleado
INNER JOIN Clientes cl ON pe.Id_cliente = cl.Id_cliente
WHERE pe.Id_mesa = 0 and pe.Estado_pedido = 'PENDIENTE' and
pe.Fecha_pedido = @Texto_busqueda1 and pe.Tipo_pedido = 'DOMICILIO'
END
ELSE IF @Tipo_busqueda = 'DOMICILIOS OTROS'
BEGIN
SELECT * 
FROM Pedidos pe
INNER JOIN Mesas me ON pe.Id_mesa = me.Id_mesa
INNER JOIN Empleados em ON pe.Id_empleado = em.Id_empleado
INNER JOIN Clientes cl ON pe.Id_cliente = cl.Id_cliente
WHERE pe.Id_mesa = 0 and pe.Estado_pedido != 'PENDIENTE' and
pe.Fecha_pedido = @Texto_busqueda1 and pe.Tipo_pedido = 'DOMICILIO'
END
ELSE IF @Tipo_busqueda = 'DOMICILIOS COMPLETO'
BEGIN
SELECT * 
FROM Pedidos pe
INNER JOIN Mesas me ON pe.Id_mesa = me.Id_mesa
INNER JOIN Empleados em ON pe.Id_empleado = em.Id_empleado
INNER JOIN Clientes cl ON pe.Id_cliente = cl.Id_cliente
WHERE pe.Id_mesa = 0 and
pe.Fecha_pedido = @Texto_busqueda1 and pe.Tipo_pedido = 'DOMICILIO'
END
END
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Buscar_platos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT *
FROM Platos platos, Tipo_platos tipo
WHERE platos.Id_tipo_plato = tipo.Id_tipo_plato
and platos.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'TIPO DE PLATO'
BEGIN
SELECT TOP 50 *
FROM Platos platos, Tipo_platos tipo
WHERE platos.Id_tipo_plato = tipo.Id_tipo_plato and
tipo.Tipo_plato = @Texto_busqueda
and platos.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'ID PLATO'
BEGIN
SELECT TOP 50 *
FROM Platos platos, Tipo_platos tipo
WHERE platos.Id_tipo_plato = tipo.Id_tipo_plato and
platos.Id_plato = @Texto_busqueda
and platos.Estado = 'ACTIVO'
END
ELSE IF @Tipo_busqueda = 'NOMBRE'
BEGIN
SELECT TOP 50 *
FROM Platos platos, Tipo_platos tipo
WHERE platos.Id_tipo_plato = tipo.Id_tipo_plato and
platos.Nombre_plato like '%' + @Texto_busqueda + '%'
and platos.Estado = 'ACTIVO'
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_reservas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Buscar_reservas]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50),
@Estado varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'FECHA'
	BEGIN
	SELECT re.Id_reserva, re.Id_mesa, me.Num_mesa, 
	re.Id_cliente, cl.Nombre_cliente, re.Fecha, CONVERT(varchar(5), re.Hora) as Hora,
	re.Observaciones, re.Estado
	FROM Reservas re
	INNER JOIN Mesas me
	ON re.Id_mesa = me.Id_mesa
	INNER JOIN Clientes cl
	ON re.Id_cliente = cl.Id_cliente
	WHERE re.Fecha = CONVERT(date, @Texto_busqueda) and
	re.Estado = @Estado
	END
ELSE IF @Tipo_busqueda = 'CLIENTE'
	BEGIN
	SELECT re.Id_reserva, re.Id_mesa, me.Num_mesa, 
	re.Id_cliente, cl.Nombre_cliente, re.Fecha, CONVERT(varchar(5), re.Hora) as Hora,
	re.Observaciones, re.Estado
	FROM Reservas re
	INNER JOIN Mesas me
	ON re.Id_mesa = me.Id_mesa
	INNER JOIN Clientes cl
	ON re.Id_cliente = cl.Id_cliente
	WHERE re.Id_cliente = @Texto_busqueda and
	re.Estado = @Estado
	END
ELSE IF @Tipo_busqueda = 'MESA'
	BEGIN
	SELECT re.Id_reserva, re.Id_mesa, me.Num_mesa, 
	re.Id_cliente, cl.Nombre_cliente, re.Fecha, CONVERT(varchar(5), re.Hora) as Hora,
	re.Observaciones, re.Estado
	FROM Reservas re
	INNER JOIN Mesas me
	ON re.Id_mesa = me.Id_mesa
	INNER JOIN Clientes cl
	ON re.Id_cliente = cl.Id_cliente
	WHERE re.Id_mesa = @Texto_busqueda and
	re.Estado = @Estado
	END
END
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_tipo_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_tipo_bebidas]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 * 
FROM Tipo_bebidas
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_tipo_insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_tipo_insumos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 * 
FROM Tipo_insumos
END
ELSE IF @Tipo_busqueda = 'NOMBRE'
BEGIN
SELECT TOP 50 * 
FROM Tipo_insumos
WHERE Tipo_insumo like '%' + @Texto_busqueda + '%'
END
ELSE IF @Tipo_busqueda = 'ID'
BEGIN
SELECT TOP 50 * 
FROM Tipo_insumos
WHERE Id_tipo_insumo = @Texto_busqueda
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_tipo_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_tipo_platos]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT TOP 50 Tipo_platos.*
FROM Tipo_platos
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_turnos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_turnos]
@Tipo_busqueda varchar(50),
@Texto_busqueda1 varchar(50),
@Texto_busqueda2 varchar(50)
AS
BEGIN TRY
IF (@Tipo_busqueda = 'FECHA')
	BEGIN
		SELECT *
		FROM Turnos tu 
		WHERE tu.Fecha_turno = @Texto_busqueda1
	END
ELSE IF (@Tipo_busqueda = 'RANGO FECHAS')
	BEGIN
		SELECT *
		FROM Turnos tu 
		WHERE tu.Fecha_turno BETWEEN @Texto_busqueda1 and @Texto_busqueda2
	END
ELSE IF (@Tipo_busqueda = 'ULTIMO TURNO')
	BEGIN
		SELECT TOP 1 *
		FROM Turnos tu
		ORDER BY tu.Id_turno DESC
	END
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Buscar_ventas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Buscar_ventas]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50),
@Fecha1 date,
@Fecha2 date,
@Hora1 time(0),
@Hora2 time(0)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'COMPLETO'
BEGIN
SELECT *
FROM Ventas ve
WHERE Fecha_venta = CONVERT(date, GETDATE())
END
ELSE IF @Tipo_busqueda = 'VENTA COMPLETA'
BEGIN
SELECT TOP 50 Id_venta, ve.Id_pedido, Fecha_venta, Convert(varchar(5), Hora_venta) as Hora_venta, 
Total_parcial, Propina, Subtotal, Descuento, Bono_cupon, Total_final, 
Observaciones, pe.Id_cliente, cl.Nombre_cliente,
pe.Id_mesa, me.Num_mesa, pe.Id_empleado, em.Nombre_empleado
FROM Ventas ve, Pedidos pe, Clientes cl, Mesas me, Empleados em
WHERE ve.Id_pedido = pe.Id_pedido and
pe.Id_cliente = cl.Id_cliente and
pe.Id_mesa = me.Id_mesa and
pe.Id_empleado = em.Id_empleado
END
ELSE IF @Tipo_busqueda = 'VENTA FECHAS'
BEGIN
SELECT TOP 50 Id_venta, ve.Id_pedido, Fecha_venta, Convert(varchar(5), Hora_venta) as Hora_venta, 
Total_parcial, Propina, Subtotal, Descuento, Bono_cupon, Total_final, 
Observaciones, pe.Id_cliente, cl.Nombre_cliente,
pe.Id_mesa, me.Num_mesa, pe.Id_empleado, em.Nombre_empleado
FROM Ventas ve, Pedidos pe, Clientes cl, Mesas me, Empleados em
WHERE ve.Id_pedido = pe.Id_pedido and
pe.Id_cliente = cl.Id_cliente and
pe.Id_mesa = me.Id_mesa and
pe.Id_empleado = em.Id_empleado and
CONVERT(date,pe.Fecha_pedido) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(0),ve.Hora_venta) between CONVERT(time(0), @Hora1) and CONVERT(time(0), @Hora2)
END
ELSE IF @Tipo_busqueda = 'EMPLEADO'
BEGIN
SELECT TOP 50 Id_venta, ve.Id_pedido, Fecha_venta, Convert(varchar(5), Hora_venta) as Hora_venta, 
Total_parcial, Propina, Subtotal, Descuento, Bono_cupon, Total_final, 
Observaciones, pe.Id_cliente, cl.Nombre_cliente,
pe.Id_mesa, me.Num_mesa, pe.Id_empleado, em.Nombre_empleado
FROM Ventas ve, Pedidos pe, Clientes cl, Mesas me, Empleados em
WHERE ve.Id_pedido = pe.Id_pedido and
pe.Id_cliente = cl.Id_cliente and
pe.Id_mesa = me.Id_mesa and
pe.Id_empleado = em.Id_empleado and
CONVERT(date,pe.Fecha_pedido) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(0),ve.Hora_venta) between CONVERT(time(0), @Hora1) and CONVERT(time(0), @Hora2) 
and em.Id_empleado = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'CLIENTE'
BEGIN
SELECT TOP 50 Id_venta, ve.Id_pedido, Fecha_venta, Convert(varchar(5), Hora_venta) as Hora_venta, 
Total_parcial, Propina, Subtotal, Descuento, Bono_cupon, Total_final, 
Observaciones, pe.Id_cliente, cl.Nombre_cliente,
pe.Id_mesa, me.Num_mesa, pe.Id_empleado, em.Nombre_empleado
FROM Ventas ve, Pedidos pe, Clientes cl, Mesas me, Empleados em
WHERE ve.Id_pedido = pe.Id_pedido and
pe.Id_cliente = cl.Id_cliente and
pe.Id_mesa = me.Id_mesa and
pe.Id_empleado = em.Id_empleado and
CONVERT(date,pe.Fecha_pedido) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(0),ve.Hora_venta) between CONVERT(time(0), @Hora1) and CONVERT(time(0), @Hora2) 
and cl.Id_cliente = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'MESA'
BEGIN
SELECT TOP 50 Id_venta, ve.Id_pedido, Fecha_venta, Convert(varchar(5), Hora_venta) as Hora_venta, 
Total_parcial, Propina, Subtotal, Descuento, Bono_cupon, Total_final, 
Observaciones, pe.Id_cliente, cl.Nombre_cliente,
pe.Id_mesa, me.Num_mesa, pe.Id_empleado, em.Nombre_empleado
FROM Ventas ve, Pedidos pe, Clientes cl, Mesas me, Empleados em
WHERE ve.Id_pedido = pe.Id_pedido and
pe.Id_cliente = cl.Id_cliente and
pe.Id_mesa = me.Id_mesa and
pe.Id_empleado = em.Id_empleado and
CONVERT(date,pe.Fecha_pedido) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) and
--CONVERT(time(0),ve.Hora_venta) between CONVERT(time(0), @Hora1) and CONVERT(time(0), @Hora2) 
me.Id_mesa = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'RESUMEN VENTA'
BEGIN
DECLARE @Cantidad_ventas int
--CANTIDAD DE VENTAS
SELECT @Cantidad_ventas = COUNT(ve.Id_venta)
FROM Ventas ve
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(2),ve.Hora_venta) between CONVERT(time(2), @Hora1) and  CONVERT(time(2), @Hora2)
--VALOR TOTAL DE VENTAS
DECLARE @Valor_total_ventas int
SELECT @Valor_total_ventas = SUM(ve.Total_final)
FROM Ventas ve
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(2),ve.Hora_venta) between CONVERT(time(2), @Hora1) and  CONVERT(time(2), @Hora2)
--VALOR TOTAL DE PROPINAS
DECLARE @Valor_total_propinas int
SELECT @Valor_total_propinas = SUM(ve.Propina)
FROM Ventas ve
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(2),ve.Hora_venta) between CONVERT(time(2), @Hora1) and  CONVERT(time(2), @Hora2)

DECLARE @Valor_total_ventas_sin_propina int
SELECT @Valor_total_ventas_sin_propina = SUM(ve.Total_parcial)
FROM Ventas ve
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) --and
--CONVERT(time(2),ve.Hora_venta) between CONVERT(time(2), @Hora1) and  CONVERT(time(2), @Hora2)

DECLARE @Total_pagos_efectivo int
SELECT @Total_pagos_efectivo = COUNT(*)
FROM Ventas ve INNER JOIN Detalle_venta dv ON ve.Id_venta = dv.Id_venta
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) and
dv.Metodo_pago = 'EFECTIVO'

DECLARE @Total_pagos_tarjeta int
SELECT @Total_pagos_tarjeta = COUNT(*)
FROM Ventas ve INNER JOIN Detalle_venta dv ON ve.Id_venta = dv.Id_venta
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) and
dv.Metodo_pago = 'DEBITO' and dv.Metodo_pago = 'CREDITO'

DECLARE @Valor_total_ventas_efectivo int
SELECT @Valor_total_ventas_efectivo = SUM(ve.Total_final)
FROM Ventas ve INNER JOIN Detalle_venta dv ON ve.Id_venta = dv.Id_venta
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) and
dv.Metodo_pago = 'EFECTIVO'

DECLARE @Valor_total_ventas_tarjeta int
SELECT @Valor_total_ventas_tarjeta = SUM(ve.Total_final)
FROM Ventas ve INNER JOIN Detalle_venta dv ON ve.Id_venta = dv.Id_venta
WHERE CONVERT(date,ve.Fecha_venta) between CONVERT(date, @Fecha1) and CONVERT(date, @Fecha2) and
dv.Metodo_pago = 'DEBITO' and dv.Metodo_pago = 'CREDITO'

SELECT @Cantidad_ventas as Cantidad_ventas, @Valor_total_ventas as Total_ventas,
@Valor_total_propinas as Total_propinas, @Valor_total_ventas_sin_propina as Total_ventas_sin_propina,
@Total_pagos_efectivo as Pagos_efectivo, @Total_pagos_tarjeta as Pagos_tarjeta, 
@Valor_total_ventas_efectivo as Total_ventas_efectivo, @Valor_total_ventas_tarjeta as Total_ventas_tarjeta

END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Cambiar_estado_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Cambiar_estado_pedido]
@Id_pedido int,
@Estado varchar(50),
@Id_mesa int
AS
BEGIN TRY
BEGIN TRANSACTION CambiarEstadoPedido
BEGIN
UPDATE Pedidos
SET
Estado_pedido = @Estado,
Id_mesa = @Id_mesa
WHERE Id_pedido = @Id_pedido 
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Cancelar_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Cancelar_pedido]
@Id_pedido int,
@Observaciones varchar(200)
AS
BEGIN TRY
BEGIN TRANSACTION CambiarEstadoPedido
BEGIN
UPDATE Pedidos
SET
Estado_pedido = 'CANCELADO',
Observaciones_pedido = @Observaciones
WHERE Id_pedido = @Id_pedido 
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Cerrar_turno]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Cerrar_turno]
@Id_turno int
AS
BEGIN TRY
--OBTENER LA FECHA DEL TURNO
DECLARE @Fecha_turno date
SET @Fecha_turno = 
(SELECT Fecha_turno FROM Turnos WHERE Id_turno = @Id_turno)

--OBTENER EL VALOR INICIAL
DECLARE @Valor_inicial decimal(19,2)
SET @Valor_inicial = 
(SELECT Valor_inicial FROM Turnos WHERE Id_turno = @Id_turno)

IF (@Valor_inicial IS NULL)
BEGIN
	SET @Valor_inicial = 0;
END

--OBTENER TODOS LOS EGRESOS
DECLARE @Total_egresos decimal(19,2)
SET @Total_egresos = 
(SELECT SUM(Valor_egreso) FROM Egresos WHERE Fecha_egreso = @Fecha_turno)

IF (@Total_egresos IS NULL)
BEGIN
	SET @Total_egresos = 0;
END

--OBTENER TODOS LOS INGRESOS
DECLARE @Total_ingresos decimal(19,2)
SET @Total_ingresos = 
(SELECT SUM(Valor_ingreso) FROM Ingresos WHERE Fecha_ingreso = @Fecha_turno)

IF (@Total_ingresos IS NULL)
BEGIN
	SET @Total_ingresos = 0;
END

--OBTENER EL TOTAL DE VENTAS
DECLARE @Total_ventas decimal(19,2) = 
(SELECT SUM(ve.Total_final) FROM Ventas ve WHERE ve.Fecha_venta = @Fecha_turno)

IF (@Total_ventas IS NULL)
BEGIN
	SET @Total_ventas = 0;
END

--OBTENER EL TOTAL DE NOMINA
DECLARE @Total_nomina decimal(19,2) = 
(SELECT SUM(nom.Total_nomina) FROM Nomina_empleado nom WHERE nom.Fecha_nomina = @Fecha_turno)

IF (@Total_nomina IS NULL)
BEGIN
	SET @Total_nomina = 0;
END

DECLARE @Total_turno decimal(19,2) = @Valor_inicial + @Total_ingresos + @Total_ventas - @Total_egresos - @Total_nomina

UPDATE Turnos SET
Total_ingresos = @Total_ingresos,
Total_egresos = @Total_egresos,
Total_turno = @Total_turno,
Total_ventas = @Total_ventas,
Total_nomina = @Total_nomina,
Estado_turno = 'CERRADO'
WHERE Id_turno = @Id_turno

END TRY
BEGIN CATCH
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Clave_maestra]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Clave_maestra]
@Id_empleado int,
@Codigo_maestro int
AS
BEGIN TRY
BEGIN TRANSACTION ClaveMaestra
BEGIN
UPDATE Empleados
SET
Codigo_maestro = @Codigo_maestro
WHERE Id_empleado = @Id_empleado
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_bebidas]
@Id_bebida int,
@Nombre_bebida varchar(200),
@Descripcion_bebida varchar(500),
@Precio_bebida decimal(19, 2),
@Precio_trago decimal(19, 2),
@Precio_trago_doble decimal(19, 2),
@Precio_proveedor decimal(19, 2),
@Id_proveedor int,
@Imagen varchar(MAX),
@Id_tipo_bebida int,
@Cantidad_unidades int,
@Cantidad_x_unidades int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarBebidas
BEGIN
DECLARE @Cantidad_total int
SET @Cantidad_total = @Cantidad_unidades * @Cantidad_x_unidades
UPDATE Bebidas
SET
Nombre_bebida = @Nombre_bebida,
Descripcion_bebida = @Descripcion_bebida,
Precio_bebida = @Precio_bebida,
Imagen = @Imagen,
Id_tipo_bebida = @Id_tipo_bebida
WHERE Id_bebida = @Id_bebida
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_cliente]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_cliente]
@Id_cliente int,
@Nombre_cliente varchar(500),
@Telefono_cliente varchar(50),
@Correo_electronico varchar(500),
@Direccion_cliente varchar(100),
@Referencia_ubicacion varchar(100),
@Otras_observaciones varchar(100),
@Estado_cliente varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarCliente
BEGIN
UPDATE Clientes
SET
Nombre_cliente = @Nombre_cliente,
Telefono_cliente = @Telefono_cliente,
Correo_electronico = @Correo_electronico,
Direccion_cliente = @Direccion_cliente,
Referencia_ubicacion = @Referencia_ubicacion,
Otras_observaciones = @Otras_observaciones,
Estado_cliente = @Estado_cliente
WHERE Id_cliente = @Id_cliente
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_detalle_plato]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_detalle_plato]
@Id_detalle int,
@Id_plato int,
@Id_insumo int,
@Cantidad_insumo int,
@Tipo_update varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarDetallePlato
BEGIN
IF @Tipo_update = 'ELIMINAR'
BEGIN
DELETE FROM Detalle_platos
WHERE Id_detalle = @Id_detalle
END
ELSE
BEGIN
UPDATE Detalle_platos
SET
Id_plato = @Id_plato,
Id_insumo = @Id_insumo,
Cantidad_insumo = @Cantidad_insumo
WHERE Id_detalle = @Id_detalle
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_empleado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_empleado]
@Id_empleado int,
@Nombre_empleado varchar(500),
@Telefono_empleado varchar(50),
@Identificacion_empleado varchar(50),
@Correo_electronico varchar(500),
@Cargo_empleado varchar(50),
@Password varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarEmpleado
BEGIN
UPDATE Empleados
SET
Nombre_empleado = @Nombre_empleado,
Telefono_empleado = @Telefono_empleado,
Identificacion_empleado = @Identificacion_empleado,
Correo_electronico = @Correo_electronico,
Cargo_empleado = @Cargo_empleado
WHERE Id_empleado = @Id_empleado
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_ingrediente]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Editar_ingrediente]
@Id_ingrediente int,
@Tipo_ingrediente varchar(50),
@Nombre_ingrediente varchar(50),
@Descripcion_ingrediente varchar(200),
@Estado_ingrediente varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarIngrediente
BEGIN
	IF ((SELECT COUNT(*) FROM Ingredientes 
		WHERE Id_ingrediente = @Id_ingrediente) < 1)
	BEGIN
		RAISERROR('No se encuentra el ingrediente a editar', 11, 1)
	END
	ELSE
	BEGIN
		UPDATE Ingredientes SET
		Tipo_ingrediente = @Tipo_ingrediente, 
		Nombre_ingrediente = @Nombre_ingrediente, 
		Descripcion_ingrediente = @Descripcion_ingrediente,
		Estado_ingrediente = @Estado_ingrediente
		WHERE Id_ingrediente = @Id_ingrediente;
	END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_insumos]
@Id_insumo int,
@Nombre_insumo varchar(50),
@Id_tipo_insumo int,
@Cantidad_existente int,
@Tipo_medida varchar(50),
@Observaciones varchar(500)
AS
BEGIN TRY
BEGIN TRANSACTION EditarInsumo
BEGIN
UPDATE Insumos
SET
Nombre_insumo = @Nombre_insumo,
Id_tipo_insumo = @Id_tipo_insumo,
Cantidad_existente = @Cantidad_existente,
Tipo_medida = @Tipo_medida,
Observaciones = @Observaciones
WHERE Id_insumo = @Id_insumo
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_platos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_platos]
@Id_plato int,
@Nombre_plato varchar(50),
@Id_tipo_plato int,
@Precio_plato int,
@Imagen_plato varchar(MAX),
@Descripcion_plato varchar(500),
@Estado_plato varchar(50),
@Plato_detallado varchar(50),
@Plato_carta varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarPlatos
BEGIN
UPDATE Platos
SET
Nombre_plato = @Nombre_plato,
Id_tipo_plato = @Id_tipo_plato,
Precio_plato = @Precio_plato,
Imagen_plato = @Imagen_plato,
Descripcion_plato = @Descripcion_plato,
Estado = @Estado_plato,
Plato_detallado = @Plato_detallado,
Plato_carta = @Plato_carta
WHERE Id_plato = @Id_plato
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Editar_reserva]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Editar_reserva]
@Id_reserva int,
@Id_mesa int,
@Id_cliente int,
@Fecha date,
@Hora time(2),
@Observaciones varchar(50),
@Estado varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION EditarReserva
BEGIN
UPDATE Reservas
SET
Id_mesa = @Id_mesa,
Id_cliente = @Id_cliente,
Fecha = @Fecha,
Hora = @Hora,
Observaciones = @Observaciones,
Estado = @Estado
WHERE Id_reserva = @Id_reserva
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Estadisticas_diarias]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Estadisticas_diarias]
@Id_turno int,
@Fecha date
AS
BEGIN TRY
--OBTENER LA FECHA DEL TURNO
DECLARE @Fecha_turno date
SET @Fecha_turno = 
(SELECT Fecha_turno FROM Turnos WHERE Id_turno = @Id_turno)

--OBTENER EL VALOR INICIAL
DECLARE @Valor_inicial decimal(19,2)
SET @Valor_inicial = 
(SELECT Valor_inicial FROM Turnos WHERE Id_turno = @Id_turno)

IF (@Valor_inicial IS NULL)
BEGIN
	SET @Valor_inicial = 0;
END

--OBTENER TODOS LOS EGRESOS
DECLARE @Total_egresos decimal(19,2)
SET @Total_egresos = 
(SELECT SUM(Valor_egreso) FROM Egresos WHERE Fecha_egreso = @Fecha)

IF (@Total_egresos IS NULL)
BEGIN
	SET @Total_egresos = 0;
END

--OBTENER TODOS LOS INGRESOS
DECLARE @Total_ingresos decimal(19,2)
SET @Total_ingresos = 
(SELECT SUM(Valor_ingreso) FROM Ingresos WHERE Fecha_ingreso = @Fecha)

IF (@Total_ingresos IS NULL)
BEGIN
	SET @Total_ingresos = 0;
END

--OBTENER EL TOTAL DE VENTAS
DECLARE @Total_ventas decimal(19,2) = 
(SELECT SUM(ve.Total_final) FROM Ventas ve WHERE ve.Fecha_venta = @Fecha)

IF (@Total_ventas IS NULL)
BEGIN
	SET @Total_ventas = 0;
END

--OBTENER EL TOTAL DE NOMINA
DECLARE @Total_nomina decimal(19,2) = 
(SELECT SUM(nom.Total_nomina) FROM Nomina_empleado nom WHERE nom.Fecha_nomina = @Fecha)

IF (@Total_nomina IS NULL)
BEGIN
	SET @Total_nomina = 0;
END

DECLARE @Total_turno decimal(19,2) = @Valor_inicial + @Total_ingresos + @Total_ventas - @Total_egresos - @Total_nomina

UPDATE Turnos SET
Total_ingresos = @Total_ingresos,
Total_egresos = @Total_egresos,
Total_turno = @Total_turno,
Total_ventas = @Total_ventas,
Total_nomina = @Total_nomina
WHERE Id_turno = @Id_turno

SELECT * FROM Turnos WHERE Id_turno = @Id_turno

DECLARE @Pedidos table (Id_tipo int, Cantidad int, Tipo varchar(50), 
Precio decimal(19,2), Nombre varchar(100))

INSERT INTO @Pedidos
SELECT detalle.Id_tipo, detalle.Cantidad, 
detalle.Tipo, detalle.Precio, pl.Nombre_plato as Nombre
FROM Detalle_pedido detalle
INNER JOIN Pedidos pe ON detalle.Id_pedido = pe.Id_pedido
INNER JOIN Platos pl ON detalle.Id_tipo = pl.Id_plato
INNER JOIN Ventas ve ON pe.Id_pedido = ve.Id_pedido
WHERE pe.Fecha_pedido = @Fecha

INSERT INTO @Pedidos
SELECT detalle.Id_tipo, detalle.Cantidad, 
detalle.Tipo, detalle.Precio, be.Nombre_bebida as Nombre
FROM Detalle_pedido detalle
INNER JOIN Pedidos pe ON detalle.Id_pedido = pe.Id_pedido
INNER JOIN Bebidas be ON detalle.Id_tipo = be.Id_bebida
INNER JOIN Ventas ve ON pe.Id_pedido = ve.Id_pedido
WHERE pe.Fecha_pedido = @Fecha

SELECT * FROM @Pedidos

SELECT COUNT(ve.Id_venta) as Cantidad, 
deve.Metodo_pago, 
SUM(deve.Valor_pago) as Total
FROM Ventas ve
INNER JOIN Detalle_venta deve ON ve.Id_venta = deve.Id_venta
WHERE ve.Fecha_venta = @Fecha
GROUP BY deve.Metodo_pago

END TRY
BEGIN CATCH
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Estadisticas_fechas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Estadisticas_fechas]
@Fecha1 date,
@Fecha2 date
AS
BEGIN TRY

--OBTENER TODOS LOS EGRESOS
DECLARE @Total_egresos decimal(19,2)
SET @Total_egresos = 
(SELECT SUM(Valor_egreso) FROM Egresos WHERE Fecha_egreso BETWEEN @Fecha1 AND @Fecha2)

IF (@Total_egresos IS NULL)
BEGIN
	SET @Total_egresos = 0;
END

--OBTENER TODOS LOS INGRESOS
DECLARE @Total_ingresos decimal(19,2)
SET @Total_ingresos = 
(SELECT SUM(Valor_ingreso) FROM Ingresos WHERE Fecha_ingreso BETWEEN @Fecha1 AND @Fecha2)

IF (@Total_ingresos IS NULL)
BEGIN
	SET @Total_ingresos = 0;
END

--OBTENER EL TOTAL DE VENTAS
DECLARE @Total_ventas decimal(19,2) = 
(SELECT SUM(ve.Total_final) FROM Ventas ve WHERE ve.Fecha_venta BETWEEN @Fecha1 AND @Fecha2)

IF (@Total_ventas IS NULL)
BEGIN
	SET @Total_ventas = 0;
END

--OBTENER EL TOTAL DE NOMINA
DECLARE @Total_nomina decimal(19,2) = 
(SELECT SUM(nom.Total_nomina) FROM Nomina_empleado nom WHERE nom.Fecha_nomina BETWEEN @Fecha1 AND @Fecha2)

IF (@Total_nomina IS NULL)
BEGIN
	SET @Total_nomina = 0;
END

DECLARE @Total_turno decimal(19,2) = @Total_ingresos + @Total_ventas - @Total_egresos - @Total_nomina

SELECT 0 AS Id_turno, 
@Fecha1 as Fecha_turno, 
0 as Valor_inicial, 
@Total_ingresos as Total_ingresos, 
@Total_egresos as Total_egresos, 
@Total_ventas as Total_ventas, 
@Total_nomina as Total_nomina, 
@Total_turno as Total_turno, 
'CERRADO' as Estado_turno

DECLARE @Pedidos table (Id_tipo int, Cantidad int, Tipo varchar(50), 
Precio decimal(19,2), Nombre varchar(100))

INSERT INTO @Pedidos
SELECT detalle.Id_tipo, detalle.Cantidad, 
detalle.Tipo, detalle.Precio, pl.Nombre_plato as Nombre
FROM Detalle_pedido detalle
INNER JOIN Pedidos pe ON detalle.Id_pedido = pe.Id_pedido
INNER JOIN Platos pl ON detalle.Id_tipo = pl.Id_plato
INNER JOIN Ventas ve ON pe.Id_pedido = ve.Id_pedido
WHERE pe.Fecha_pedido BETWEEN @Fecha1 and @Fecha2

INSERT INTO @Pedidos
SELECT detalle.Id_tipo, detalle.Cantidad, 
detalle.Tipo, detalle.Precio, be.Nombre_bebida as Nombre
FROM Detalle_pedido detalle
INNER JOIN Pedidos pe ON detalle.Id_pedido = pe.Id_pedido
INNER JOIN Bebidas be ON detalle.Id_tipo = be.Id_bebida
INNER JOIN Ventas ve ON pe.Id_pedido = ve.Id_pedido
WHERE pe.Fecha_pedido BETWEEN @Fecha1 and @Fecha2

SELECT * FROM @Pedidos

SELECT COUNT(ve.Id_venta) as Cantidad, 
deve.Metodo_pago, 
SUM(deve.Valor_pago) as Total
FROM Ventas ve
INNER JOIN Detalle_venta deve ON ve.Id_venta = deve.Id_venta
WHERE ve.Fecha_venta BETWEEN @Fecha1 and @Fecha2
GROUP BY deve.Metodo_pago

END TRY
BEGIN CATCH
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Factura_final]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Factura_final]
@Tipo_busqueda varchar(50),
@Texto_busqueda varchar(50)
AS
BEGIN TRY
BEGIN
IF @Tipo_busqueda = 'DATOS VENTA'
BEGIN
SELECT ventas.Id_venta, ventas.Id_pedido, 
(CASE WHEN CONVERT(varchar,mesas.Num_mesa) = '0'
THEN 'DOMICILIO' ELSE CONVERT(varchar,mesas.Num_mesa) END) as Num_mesa,
em.Nombre_empleado, 
(cl.Nombre_cliente + ' - Teléfono: ' + cl.Telefono_cliente +
CASE WHEN cl.Direccion_cliente IS NOT NULL 
THEN ' - Dirección: ' + cl.Direccion_cliente ELSE '' END +
CASE WHEN cl.Referencia_ubicacion IS NOT NULL 
THEN ' - Referencia: ' + cl.Referencia_ubicacion ELSE '' END) as Nombre_cliente, ventas.Fecha_venta, ventas.Hora_venta,
ventas.Total_parcial, ventas.Propina, ventas.Subtotal, ventas.Descuento,
ventas.Bono_cupon, ventas.Total_final, ventas.Observaciones, mesas.Id_mesa, 
ventas.Desechables, ventas.Domicilio
FROM Ventas ventas, Pedidos pedidos, 
Mesas mesas, Empleados em, Clientes cl
WHERE ventas.Id_pedido = pedidos.Id_pedido and
pedidos.Id_mesa = mesas.Id_mesa and
pedidos.Id_empleado = em.Id_empleado and
pedidos.Id_cliente = cl.Id_cliente and
pedidos.Id_pedido = @Texto_busqueda
END
ELSE IF @Tipo_busqueda = 'DETALLE PEDIDO'
BEGIN
SELECT de.Id_detalle_pedido, de.Id_pedido, de.Id_tipo,
de.Tipo, pl.Nombre_plato AS Nombre,
FLOOR(de.Precio) as Precio, de.Cantidad, FLOOR(de.Precio * de.Cantidad) AS Total, 
de.Observaciones
FROM Detalle_pedido de
INNER JOIN Platos pl ON de.Id_tipo = pl.Id_plato
WHERE de.Id_pedido = CONVERT(int, @Texto_busqueda)
UNION
SELECT de.Id_detalle_pedido, de.Id_pedido, de.Id_tipo,
de.Tipo, be.Nombre_bebida AS Nombre,
FLOOR(de.Precio) as Precio, de.Cantidad, FLOOR(de.Precio * de.Cantidad) AS Total, 
de.Observaciones 
FROM Detalle_pedido de
INNER JOIN Bebidas be ON de.Id_tipo = be.Id_bebida
WHERE de.Id_pedido = CONVERT(int, @Texto_busqueda)

END
ELSE IF @Tipo_busqueda = 'DETALLE VENTA'
BEGIN
SELECT DISTINCT dv.* 
FROM Detalle_venta dv, Ventas
WHERE dv.Id_venta = ventas.Id_venta and
ventas.Id_pedido = @Texto_busqueda
END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Inactivar_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[sp_Inactivar_bebidas]
@Id_bebida int
AS
BEGIN TRY
BEGIN TRANSACTION InactivarBebidas
BEGIN
UPDATE Bebidas
SET
Estado = 'INACTIVO'
WHERE Id_bebida = @Id_bebida
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Inactivar_plato]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create proc [dbo].[sp_Inactivar_plato]
@Id_plato int
AS
BEGIN TRY
BEGIN TRANSACTION InactivarPlatos
BEGIN
UPDATE Platos
SET
Estado = 'INACTIVO'
WHERE Id_plato = @Id_plato
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_bebidas]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_bebidas]
@Id_bebida int output,
@Nombre_bebida varchar(200),
@Descripcion_bebida varchar(500),
@Precio_bebida decimal(19, 2),
@Precio_trago decimal(19, 2),
@Precio_trago_doble decimal(19, 2),
@Precio_proveedor decimal(19, 2),
@Id_proveedor int,
@Imagen varchar(MAX),
@Id_tipo_bebida int,
@Cantidad_unidades int,
@Cantidad_x_unidades int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarBebidas
BEGIN
DECLARE @Estado varchar(50) = 'ACTIVO'
DECLARE @Cantidad_total int
SET @Cantidad_total = @Cantidad_unidades * @Cantidad_x_unidades
INSERT INTO Bebidas
VALUES (@Nombre_bebida, @Descripcion_bebida, @Precio_bebida, @Precio_trago, @Precio_trago_doble, 
@Precio_proveedor, @Id_proveedor, @Imagen, @Id_tipo_bebida, @Cantidad_unidades,
@Cantidad_x_unidades, @Cantidad_total, @Estado)
SET @Id_bebida = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_cliente]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_cliente]
@Id_cliente int output,
@Nombre_cliente varchar(500),
@Telefono_cliente varchar(50),
@Correo_electronico varchar(500),
@Direccion_cliente varchar(100),
@Referencia_ubicacion varchar(100),
@Otras_observaciones varchar(100),
@Estado_cliente varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION InsertarCliente
BEGIN
INSERT INTO Clientes
VALUES (@Nombre_cliente, @Telefono_cliente, @Correo_electronico, @Direccion_cliente, @Referencia_ubicacion, @Otras_observaciones, @Estado_cliente)
SET @Id_cliente = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_detalle_bebida]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_detalle_bebida]
@Id_detalle int output,
@Id_bebida int,
@Tipo varchar(50),
@Id_tipo int,
@Tipo_cantidad varchar(50),
@Cantidad_usada int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarDetalleBebida
BEGIN
INSERT INTO Detalle_bebida
VALUES (@Id_bebida, @Tipo, @Id_tipo, @Tipo_cantidad, @Cantidad_usada)
SET @Id_detalle = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_detalle_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[sp_Insertar_detalle_pedido]
@Id_detalle_pedido int output,
@Id_pedido int,
@Id_tipo int,
@Tipo varchar(50),
@Precio decimal(19, 2),
@Cantidad int,
@Observaciones varchar(100)
AS
BEGIN
	INSERT INTO Detalle_pedido 
	VALUES (@Id_pedido, @Id_tipo, @Tipo, @Precio, @Cantidad, @Observaciones);

	SET @Id_detalle_pedido = SCOPE_IDENTITY();
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_detalle_plato]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_detalle_plato]
@Id_detalle int output,
@Id_plato int,
@Id_insumo int,
@Cantidad_insumo int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarDetallePlato
BEGIN
INSERT INTO Detalle_platos
VALUES (@Id_plato, @Id_insumo, @Cantidad_insumo)
SET @Id_detalle = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_empleado]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_empleado]
@Id_empleado int output,
@Nombre_empleado varchar(500),
@Telefono_empleado varchar(50),
@Identificacion_empleado varchar(50),
@Correo_electronico varchar(500),
@Cargo_empleado varchar(50),
@Password varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION InsertaEmpleado
BEGIN
INSERT INTO Empleados
VALUES (@Nombre_empleado, @Telefono_empleado, @Identificacion_empleado, @Correo_electronico, 
@Cargo_empleado, @Password, 0, 'ACTIVO')
SET @Id_empleado = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_ingrediente]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_ingrediente]
@Id_ingrediente int output,
@Tipo_ingrediente varchar(50),
@Nombre_ingrediente varchar(50),
@Descripcion_ingrediente varchar(200),
@Estado_ingrediente varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION InsertarIngrediente
BEGIN
	IF ((SELECT COUNT(*) FROM Ingredientes 
		WHERE Nombre_ingrediente = @Nombre_ingrediente) > 0)
	BEGIN
		RAISERROR('Ya existe un ingrediente con el mismo nombre', 11, 1)
	END
	ELSE
	BEGIN
		INSERT INTO Ingredientes
		VALUES (@Tipo_ingrediente, @Nombre_ingrediente, @Descripcion_ingrediente,
		@Estado_ingrediente)
		SET @Id_ingrediente = SCOPE_IDENTITY();
	END
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_insumos]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_insumos]
@Id_insumo int output,
@Nombre_insumo varchar(50),
@Id_tipo_insumo int,
@Cantidad_existente int,
@Tipo_medida varchar(50),
@Observaciones varchar(500)
AS
BEGIN TRY
BEGIN TRANSACTION InsertarInsumo
BEGIN
INSERT INTO Insumos
VALUES (@Nombre_insumo, @Id_tipo_insumo, @Cantidad_existente, @Tipo_medida, @Observaciones)
SET @Id_insumo = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_pedido]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_pedido]
@Id_pedido int output,
@Id_mesa int,
@Id_empleado int,
@Id_cliente int,
@Fecha_pedido date,
@Hora_pedido time(2),
@Tipo_pedido varchar(50),
@Observaciones_pedido varchar(200),
@CantidadClientes int
AS
BEGIN TRY
BEGIN TRANSACTION InsertarPedido
BEGIN
DECLARE @Estado_pedido varchar(50)= 'PENDIENTE'
INSERT INTO Pedidos
VALUES (@Id_mesa, @Id_empleado, @Id_cliente, @Estado_pedido ,@Fecha_pedido, 
@Hora_pedido, @Tipo_pedido, @Observaciones_pedido, @CantidadClientes)
SET @Id_pedido = SCOPE_IDENTITY();

SELECT 'OK' as Respuesta, @Id_pedido as Id_pedido

END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_plato]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[sp_Insertar_plato]
@Id_plato int output,
@Nombre_plato varchar(50),
@Id_tipo_plato int,
@Precio_plato decimal(19,2),
@Imagen_plato varchar(MAX),
@Descripcion_plato varchar(500),
@Estado_plato varchar(50),
@Plato_detallado varchar(50),
@Plato_carta varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION InsertarPlatos
BEGIN
INSERT INTO Platos
VALUES (@Nombre_plato, @Id_tipo_plato, @Precio_plato, @Imagen_plato, 
@Descripcion_plato, @Estado_plato, @Plato_detallado, @Plato_carta)
SET @Id_plato = SCOPE_IDENTITY();
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Insertar_reserva]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Insertar_reserva]
@Id_reserva int output,
@Id_mesa int,
@Id_cliente int,
@Fecha date,
@Hora time(2),
@Observaciones varchar(50)
AS
BEGIN TRY
BEGIN TRANSACTION InsertarReserva
BEGIN
DECLARE @Estado varchar(50) = 'PENDIENTE'
INSERT INTO Reservas
VALUES (@Id_mesa, @Id_cliente, @Fecha, @Hora, @Observaciones, @Estado)
SET @Id_reserva = SCOPE_IDENTITY()
END
COMMIT
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
ROLLBACK
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Login]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[sp_Login]
@Usuario varchar(50),
@Pass varchar(50),
@Fecha date
AS
BEGIN TRY
	BEGIN
	--DECLARAR LA RESPUESTA
	DECLARE @Respuesta varchar(200) = 'OK'
	--OBTENER EL ID DEL EMPLEADO
	DECLARE @Id_empleado int
	SET @Id_empleado =
	(SELECT Id_empleado
	FROM Empleados
	WHERE Id_empleado = @Usuario and
	Password = @Pass)
	--SI NO LO ENCUENTRA, RETORNA LA RESPUESTA CON EL ERROR
	IF (@Id_empleado IS NULL OR @Id_empleado = 0)
	BEGIN
		SET @Respuesta = 'Verifique la clave o el usuario'
		SELECT @Respuesta as Respuesta
		RETURN 0;
	END

	--OBTENER EL TIPO DE USUARIO (CARGO_EMPLEADO)
	DECLARE @Tipo_usuario varchar(50)
	SELECT @Tipo_usuario = Cargo_empleado
	FROM Empleados WHERE Id_empleado = @Id_empleado

	IF (@Tipo_usuario IS NULL OR @Tipo_usuario = '')
	BEGIN
		SET @Respuesta = 'Verifique el cargo del empleado'
		SELECT @Respuesta as Respuesta
		RETURN 0;
	END

	--OBTENER EL TURNO ANTERIOR
	DECLARE @Id_turno_anterior int,
	@Id_turno_actual int

	SELECT @Id_turno_anterior = tu.Id_turno
	FROM Turnos tu
	ORDER BY tu.Id_turno ASC

	--SI NO HAY TURNOS ANTERIORES INSERTAMOS UNO NUEVO
	IF (@Id_turno_anterior IS NULL OR @Id_turno_anterior = 0)
	BEGIN
		INSERT INTO Turnos VALUES (@Fecha, 0, 0, 0, 0, 0, 0, 'ABIERTO')
		SET @Id_turno_actual = SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		--SI EXISTEN TURNOS ANTERIORES VERIFICAMOS SI ESTÁ ABIERTO
		DECLARE @Fecha_turno_anterior date
		SET @Fecha_turno_anterior = 
		(SELECT Fecha_turno FROM Turnos WHERE Id_turno = @Id_turno_anterior)

		--SI EXISTEN TURNOS ANTERIORES VERIFICAMOS SI ESTÁ ABIERTO
		DECLARE @Estado_turno_anterior varchar(50)
		SET @Estado_turno_anterior = 
		(SELECT Estado_turno FROM Turnos WHERE Id_turno = @Id_turno_anterior)
		--SI ESTÁ ABIERTO LO CERRAMOS
		IF (@Estado_turno_anterior = 'ABIERTO' and @Fecha > @Fecha_turno_anterior)
		BEGIN
			--EJECUTAMOS EL CERRAR TURNO DONDE CALCULARÁ LOS TOTALES
			EXEC sp_Cerrar_turno @Id_turno_anterior
			--INSERTAMOS EL TURNO ACTUAL
			INSERT INTO Turnos VALUES (@Fecha, 0, 0, 0, 0, 0, 0,'ABIERTO')
			SET @Id_turno_actual = SCOPE_IDENTITY();
		END
		ELSE IF (@Estado_turno_anterior = 'CERRADO' and @Fecha > @Fecha_turno_anterior)
		BEGIN
			--INSERTAMOS EL TURNO ACTUAL
			INSERT INTO Turnos VALUES (@Fecha, 0, 0, 0, 0, 0, 0,'ABIERTO')
			SET @Id_turno_actual = SCOPE_IDENTITY();
		END
		ELSE --IF (@Fecha = @Fecha_turno_anterior)
		BEGIN
			SET @Id_turno_actual = @Id_turno_anterior
		END
	END

	--ENVIAMOS LA RESPUESTA
	SELECT @Respuesta as Respuesta,
	@Tipo_usuario as Tipo_usuario

	--ENVIAMOS EL EMPLEADO QUE INGRESÓ
	SELECT * 
	FROM Empleados WHERE Id_empleado = @Id_empleado

	--SELECCIONAMOS EL TURNO ACTUAL
	SELECT *
	FROM Turnos WHERE Id_turno = @Id_turno_actual

--IF @Tipo_busqueda = 'CODIGO MAESTRO'
--BEGIN
--SELECT TOP 1 * 
--FROM Empleados
--WHERE Codigo_maestro = @Texto_busqueda1
--ORDER BY Id_empleado DESC
--END
END
RETURN 1
END TRY
BEGIN CATCH
IF @@TRANCOUNT > 0
DECLARE @Mensaje_error NVARCHAR(4000) = ERROR_MESSAGE();
DECLARE @Mensaje_severidad INT = ERROR_SEVERITY();
DECLARE @Estado_error INT = ERROR_STATE();
DECLARE @Numero_error INT = ERROR_NUMBER();
DECLARE @Error_procedure varchar(500) = ERROR_PROCEDURE();
DECLARE @Error_line INT = Error_line();
RAISERROR (@Mensaje_error,
           @Mensaje_severidad,
           @Estado_error,
		   @Numero_error,
		   @Error_procedure,
		   @Error_line
           ); 
RETURN -1
END CATCH
GO
/****** Object:  StoredProcedure [dbo].[sp_Turnos_i]    Script Date: 24/10/2026 7:36:27 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE      PROC [dbo].[sp_Turnos_i]
@Id_turno int output,
@Fecha_turno date,
@Hora_turno time(2),
@Valor_inicial decimal(19,2),
@Total_ingresos decimal(19,2),
@Total_egresos decimal(19,2),
@Total_ventas decimal(19,2),
@Total_nomina decimal(19,2),
@Total_turno decimal(19,2),
@Estado_turno varchar(50)
AS
BEGIN
	INSERT INTO Turnos
	VALUES (@Fecha_turno, @Valor_inicial, @Total_ingresos, @Total_egresos,
	@Total_ventas, @Total_nomina, @Total_turno, @Estado_turno);

	SET @Id_turno = SCOPE_IDENTITY();
END
GO
