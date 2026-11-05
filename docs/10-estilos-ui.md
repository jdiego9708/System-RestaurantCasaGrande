# 10 · Estilos de interfaz (tema moderno)

Todo formulario nuevo del escritorio debe usar el tema de `SISMistico/CapaPresentacion/UI`. La pantalla de
referencia es **`FrmDeliveries`** (Domicilios): copiar su estructura para pantallas nuevas.

## Qué hay

| Pieza | Para qué |
|---|---|
| `Theme` | Paleta (`Primary`, `Success`, `Warning`, `Danger`, `Info`, `Neutral`, `Rappi`, fondos y textos), fuentes (`Title`, `Heading`, `Body`, `BodyBold`, `Small`, `Kpi`, `Button`), `Radius`, `Gap`, utilidades `Darken`/`Lighten` y dibujo (`RoundedPath`, `DrawShadow`). **Nada de colores o fuentes escritos a mano.** |
| `ModernCard` | Panel tipo tarjeta: esquinas redondeadas, sombra suave y franja de color opcional (`AccentColor`). Los hijos van dentro de su `Padding`. |
| `ModernButton` | Botón redondeado con hover/pulsado e icono. `Kind`: `Primary`, `Success`, `Danger`, `Warning`, `Neutral`, `Outline`. `Icon`: cualquier `IconChar` de FontAwesome. |
| `ModernTextBox` | Caja de texto con borde redondeado que se resalta al enfocar, icono (`Icon`), texto de ayuda (`PlaceholderText`) y estado de error (`HasError`, se quita al escribir). Se usa como un `TextBox` (`Text`, `TextChanged`, `KeyDown`, `Focus()`). |
| `Badge` | Etiqueta tipo píldora (hereda de `Label`, así que sustituye a cualquier etiqueta del Designer cambiando solo el tipo). `Kind`: `Primary`, `Success`, `Warning`, `Danger`, `Info`, `Neutral`, `Rappi`. `Badge.Draw(...)` la pinta también en celdas de tabla. |
| `KpiCard` | Indicador grande con degradado, icono, valor y título (`Caption`, `Value`, `Icon`, `Accent`). |
| `ThemeExtensions` | `form.ApplyTheme()`, `button.StyleButton(kind)`, `label.StyleLabel(LabelKind)`, `grid.StyleGrid()`, `grid.StyleModernGrid()` (lista alta, cabecera clara, sin cuadrícula), `grid.UseBadgeColumn(encabezado, texto => BadgeKind)` (pinta una columna como badges), `control.Rounded(radio)`. Referencia: `FrmFacturarPedido.Modern.cs`. |

## Cómo usarlo

**Formulario nuevo** (recomendado): construir con `ModernCard`, `ModernButton` y `KpiCard`, y al final del constructor
llamar `this.ApplyTheme()` para fondo, fuente base, cajas de texto, etiquetas y tablas.

```csharp
this.BackColor = Theme.Background;
ModernCard card = new ModernCard { Dock = DockStyle.Fill, AccentColor = Theme.Primary };
ModernButton save = new ModernButton { Text = "Guardar", Kind = ButtonKind.Success, Icon = IconChar.Check };
Label title = new Label { Text = "Clientes" };
title.StyleLabel(LabelKind.Title);
```

**Formulario ya existente (Designer)**: `ApplyTheme()` recorre los controles y les da el estilo del tema; para botones
con otro color, `button.StyleButton(ButtonKind.Danger)`. Si el Designer declara un `Button`, se puede cambiar su
tipo a `CapaPresentacion.UI.ModernButton` (como se hizo en `DomicilioSmall`).

## Reglas

- Los iconos son de **FontAwesome.Sharp** (paquete NuGet, MIT; los iconos Free son CC BY 4.0). Se usa con
  `IconChar`, en `ModernButton.Icon`, `KpiCard.Icon` o `IconPictureBox`.
- Estados con color: éxito = `Success`, peligro/cancelar = `Danger`, acción principal = `Primary`, secundaria = `Outline`.
- Un control con esquinas redondeadas pinta su fondo con el `BackColor` del contenedor: asigna el fondo del
  contenedor **antes** (o llama `ApplyTheme()` al final).
- La sombra de `ModernCard` ocupa 4 px dentro del propio control; su `Margin` ya lo descuenta para alinear con otros controles.
- En listas con barra vertical, reservar `SystemInformation.VerticalScrollBarWidth` al calcular el ancho de las
  tarjetas (si no, sale también la barra horizontal).

## Dependencia

`FontAwesome.Sharp 6.6.0` está en `SISMistico/CapaPresentacion/packages.config`. La carpeta `packages` no se sube a git:
en una máquina nueva se restaura con `nuget restore SISMistico\SISMistico.sln`.
