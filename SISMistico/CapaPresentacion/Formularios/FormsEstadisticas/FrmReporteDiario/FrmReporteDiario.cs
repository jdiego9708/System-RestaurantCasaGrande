using CapaEntidades.Models;
using CapaNegocio;
using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsEstadisticas
{
    public partial class FrmReporteDiario : Form
    {
        public FrmReporteDiario()
        {
            InitializeComponent();
            this.Load += FrmReporteDiario_Load;
            this.date1.ValueChanged += Date1_ValueChanged;
            this.date2.ValueChanged += Date2_ValueChanged;
            this.btnRefresh.Click += BtnRefresh_Click;
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await this.LoadEstadistica(this.date1.Value, this.date2.Value);
        }

        private async void Date2_ValueChanged(object sender, EventArgs e)
        {
            await this.LoadEstadistica(this.date1.Value, this.date2.Value);
        }

        private void Date1_ValueChanged(object sender, EventArgs e)
        {
            DateTimePicker date = (DateTimePicker)sender;
            this.date2.MinDate = date.Value;
        }

        private void FrmReporteDiario_Load(object sender, EventArgs e)
        {
            this.reportViewer1 = new ReportViewer();
            this.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
                "CapaPresentacion.ReportesFacturas.NominaEmpleado.rptNominaEmpleado.rdlc";
        }

        public string InformacionEmpleado { get; set; }
        public string CantidadPedidos { get; set; }
        public string ResumenResultados { get; set; }
        public string InformacionEgresos { get; set; }
        public string InformacionIngresos { get; set; }
        public string IdentificacionTurno { get; set; }
        public string FechaHora { get; set; }

        private ReportViewer reportViewer1;
        ControladorImpresion objImpresion = new ControladorImpresion();

        public async Task LoadEstadistica(DateTime date1, DateTime date2)
        {
            MensajeEspera.ShowWait("Cargando...");

            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();

            string informacionEmpleado = "Reporte generado por " +
                datos.Nombre_empleado + " - Teléfono: " + datos.EmpleadoLogin.Telefono_empleado;
            string cantidadPedidos = "";
            var (rpta, dtPedidos) =
               await NPedido.BuscarPedidos("RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
            if (dtPedidos == null)
                cantidadPedidos = "No hay ventas el día de hoy";
            else
                cantidadPedidos = "Cantidad de ventas: " + dtPedidos.Rows.Count + "";

            string id_turno = "0";
            StringBuilder resumenResultados = new StringBuilder();

            DataTable dtEstadistica;
            DataTable dtDetalle;
            DataTable dtPagos;
            bool isRango = false;

            if (date1.ToString("yyyy-MM-dd") == date2.ToString("yyyy-MM-dd"))
            {
                isRango = false;
                var result =
                await NNomina.EstadisticasDiarias(datos.Turno.Id_turno, date1.ToString("yyyy-MM-dd"));
                dtEstadistica = result.dtEstadistica;
                dtDetalle = result.dtDetalle;
                dtPagos = result.dtPagos;
            }
            else
            {
                isRango = true;
                var result =
                await NNomina.EstadisticasDiarias(date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
                dtEstadistica = result.dtEstadistica;
                dtDetalle = result.dtDetalle;
                dtPagos = result.dtPagos;
            }

            if (dtEstadistica != null)
            {
                Turno turno = new Turno(dtEstadistica.Rows[0]);
                id_turno = "Identificación del turno: " + turno.Id_turno.ToString();

                //if (!isRango)
                //    resumenResultados.Append("Fecha: ").Append(turno.Fecha_turno.ToLongDateString()).Append(Environment.NewLine);
                //else
                //    resumenResultados.Append("Entre ").Append(date1.ToLongDateString() + " y ").Append(date2.ToLongDateString()).Append(" se registra la siguiente información:").Append(Environment.NewLine);

                resumenResultados.Append("Caja inicial: ").Append(turno.Valor_inicial.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                resumenResultados.Append("Total ingresos: ").Append(turno.Total_ingresos.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                if (this.chkInfoGastos.Checked)
                    resumenResultados.Append("Total egresos: ").Append(turno.Total_egresos.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                resumenResultados.Append("Total ventas: ").Append(turno.Total_ventas.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                if (this.chkInfoNomina.Checked)
                    resumenResultados.Append("Total nomina: ").Append(turno.Total_nomina.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                resumenResultados.Append("Total: ").Append(turno.Total_turno.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);

                if (this.chkInfoDetalleVentas.Checked)
                {
                    List<TipoResumen> resumen = new List<TipoResumen>();
                    //PLATOS Y BEBIDAS
                    foreach (DataRow row in dtDetalle.Rows)
                    {
                        int id_tipo = Convert.ToInt32(row["Id_tipo"]);
                        int cantidad = Convert.ToInt32(row["Cantidad"]);
                        string nombre = Convert.ToString(row["Nombre"]);
                        decimal precio = Convert.ToDecimal(row["Precio"]) * cantidad;

                        List<TipoResumen> results = resumen.Where(x => x.Id_tipo == id_tipo).ToList();
                        if (results.Count > 0)
                        {
                            results[0].Cantidad += cantidad;
                            results[0].Valor_total += precio;
                        }
                        else
                        {
                            TipoResumen tipo = new TipoResumen
                            {
                                Id_tipo = id_tipo,
                                Nombre = nombre,
                                Cantidad = cantidad,
                                Valor_total = precio,
                            };
                            resumen.Add(tipo);
                        }
                    }

                    if (resumen.Count > 0)
                    {
                        IOrderedEnumerable<TipoResumen> detallesOrdenados = from s in resumen
                                                                            orderby s.Cantidad descending
                                                                            select s;

                        resumenResultados.Append("Detalles del día: ").Append(Environment.NewLine);

                        foreach (TipoResumen re in detallesOrdenados)
                        {
                            resumenResultados.Append("* " + re.Nombre + " - Cantidad ").Append(re.Cantidad);
                            resumenResultados.Append(" - Valor total: ").Append(re.Valor_total.ToString("C").Replace(",00", "").Replace(",00", ""));
                            resumenResultados.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        resumenResultados.Append("No se encontraron detalles");
                    }
                }

            }
            else
            {
                resumenResultados.Append("Hubo un error al encontrar los datos de estadísticas diarias");
            }

            StringBuilder infoEgresos = new StringBuilder();

            if (this.chkInfoGastos.Checked)
            {
                DataTable dtEgresos = null;

                if (isRango)
                {
                    var result = await NEgresos.BuscarEgresos("RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
                    dtEgresos = result.dtEgresos;
                }
                else
                {
                    var result = await NEgresos.BuscarEgresos("FECHA", date1.ToString("yyyy-MM-dd"));
                    dtEgresos = result.dtEgresos;
                }

                if (dtEgresos != null)
                {
                    if (dtEgresos.Rows.Count == 0)
                    {
                        infoEgresos.Append("No hay conceptos de egresos.").Append(Environment.NewLine);
                    }
                    else
                    {
                        infoEgresos.Append("Descripción de los egresos: ").Append(Environment.NewLine);
                        int contador = 0;
                        foreach (DataRow row in dtEgresos.Rows)
                        {
                            contador += 1;
                            Egresos egreso = new Egresos(row);
                            infoEgresos.Append(contador + ") Fecha: ").Append(egreso.Fecha_egreso).Append(" | ");
                            infoEgresos.Append("Valor: ").Append(egreso.Valor_egreso.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);
                            infoEgresos.Append("Observaciones: ").Append(egreso.Descripcion_egreso).Append(Environment.NewLine);
                        }
                    }
                }
                else
                    infoEgresos.Append("No hay conceptos de egresos.").Append(Environment.NewLine);
            }

            StringBuilder infoIngresos = new StringBuilder();

            if (this.chkIngresos.Checked)
            {
                DataTable dtIngresos = null;

                if (isRango)
                {
                    var result = await NIngresos.BuscarIngresos("RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"));
                    dtIngresos = result.dtIngresos;
                }
                else
                {
                    var result = await NIngresos.BuscarIngresos("FECHA", date1.ToString("yyyy-MM-dd"));
                    dtIngresos = result.dtIngresos;
                }

                if (dtIngresos != null)
                {
                    if (dtIngresos.Rows.Count == 0)
                    {
                        infoIngresos.Append("No hay conceptos de ingresos extras.").Append(Environment.NewLine);
                    }
                    else
                    {
                        infoIngresos.Append("Descripción de los ingresos: ").Append(Environment.NewLine);
                        int contador = 0;
                        foreach (DataRow row in dtIngresos.Rows)
                        {
                            contador += 1;
                            Ingresos ingreso = new Ingresos(row);
                            infoIngresos.Append(contador + ") Fecha: ").Append(ingreso.Fecha_ingreso.ToString("yyyy-MM-dd")).Append(" | ");
                            infoIngresos.Append("Valor: ").Append(ingreso.Valor_ingreso.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);
                            infoIngresos.Append("Observaciones: ").Append(ingreso.Descripcion_ingreso).Append(Environment.NewLine);
                        }
                    }
                }
                else
                    infoIngresos.Append("No hay conceptos de ingresos extras.").Append(Environment.NewLine);
            }

            if (this.chkInfoNomina.Checked)
            {
                infoEgresos.Append(Environment.NewLine).Append(Environment.NewLine);
                DataTable dtNomina;

                if (isRango)
                {
                    dtNomina = NNomina.BuscarNomina("GENERAL RANGO FECHAS", date1.ToString("yyyy-MM-dd"), date2.ToString("yyyy-MM-dd"), out rpta);
                }
                else
                {
                    dtNomina = NNomina.BuscarNomina("GENERAL FECHA", date1.ToString("yyyy-MM-dd"), out rpta);
                }

                if (dtNomina != null)
                {
                    if (dtNomina.Rows.Count == 0)
                    {
                        infoEgresos.Append("No hay nómina paga.").Append(Environment.NewLine);
                    }
                    else
                    {
                        infoEgresos.Append("Descripción de la nómina: ").Append(Environment.NewLine);
                        foreach (DataRow row in dtNomina.Rows)
                        {
                            EmpleadoNominaBinding nomina = new EmpleadoNominaBinding(row);
                            if (nomina.Estado_nomina.Equals("TERMINADO"))
                            {
                                infoEgresos.Append("*Fecha: ").Append(nomina.Fecha_nomina.ToString("dd-MM-yyyy")).Append(" | ");
                                infoEgresos.Append(nomina.Empleado.Nombre_empleado).Append(" | ");
                                infoEgresos.Append("Valor: ").Append(nomina.Total_nomina.ToString("C").Replace(",00", "").Replace(",00", "")).Append(Environment.NewLine);
                                if (!string.IsNullOrEmpty(nomina.Observaciones))
                                    infoEgresos.Append("Observaciones: " + nomina.Observaciones).Append(Environment.NewLine);
                                else
                                    infoEgresos.Append(Environment.NewLine);
                            }
                        }
                    }
                }
                else
                    infoEgresos.Append("No hay nómina paga.").Append(Environment.NewLine);
            }

            if (this.chkInfoPagos.Checked)
            {
                StringBuilder infoPagos = new StringBuilder();
                infoPagos.Append(Environment.NewLine + "Métodos de pago: ").Append(Environment.NewLine);
                foreach (DataRow rowPago in dtPagos.Rows)
                {
                    int cantidad = Convert.ToInt32(rowPago["Cantidad"]);
                    string metodo = Convert.ToString(rowPago["Metodo_pago"]);
                    decimal total = Convert.ToInt32(rowPago["Total"]);
                    infoPagos.Append("*").Append(metodo).Append(" - Cantidad " + cantidad + " por valor de " + total.ToString("C").Replace(",00", "")).Append(Environment.NewLine);
                }
                resumenResultados.Append(infoPagos);
            }

            if (this.chkDeletePedidos.Checked)
            {
                if (dtPedidos != null)
                {
                    StringBuilder infoDeletePedidos = new StringBuilder();
                    infoDeletePedidos.Append("Información de pedidos cancelados").Append(Environment.NewLine);

                    bool pedidos_cancelados = false;
                    foreach (DataRow row in dtPedidos.Rows)
                    {
                        string estado_pedido = Convert.ToString(row["Estado_pedido"]);
                        if (estado_pedido.Equals("CANCELADO"))
                        {
                            pedidos_cancelados = true;
                            Pedidos pedido = new Pedidos(row);
                            infoDeletePedidos.Append("* Fecha: " + pedido.Fecha_pedido.ToString("dd-MM-yy") + " - Hora: " + pedido.Hora_pedido + " - Motivo: " + pedido.Observaciones_pedido).Append(Environment.NewLine);
                        }
                    }

                    if (pedidos_cancelados == false)
                        infoDeletePedidos = new StringBuilder();

                    resumenResultados.Append(infoDeletePedidos);
                }
            }

            this.InformacionEmpleado = informacionEmpleado;
            this.CantidadPedidos = cantidadPedidos;
            this.ResumenResultados = resumenResultados.ToString();
            this.InformacionEgresos = infoEgresos.ToString();
            this.InformacionIngresos = infoIngresos.ToString();
            this.IdentificacionTurno = id_turno;
            this.FechaHora = DateTime.Now.ToLongDateString();
            this.ObtenerReporte();
            MensajeEspera.CloseForm();
        }

        public class TipoResumen
        {
            public int Id_tipo { get; set; }
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
            public decimal Valor_total { get; set; }
        }

        public void ObtenerReporte()
        {
            if (this.gbReporte.Controls.Count > 0)
                this.gbReporte.Controls.Clear();

            this.reportViewer1 = new ReportViewer
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
            };
            this.gbReporte.Controls.Add(this.reportViewer1);
            this.reportViewer1.LocalReport.ReportEmbeddedResource =
            "CapaPresentacion.Formularios.FormsEstadisticas.FrmReporteDiario.rptReporteDiario.rdlc";

            ReportParameter[] reportParameters = new ReportParameter[7];
            reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
            reportParameters[1] = new ReportParameter("CantidadPedidos", CantidadPedidos);
            reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
            reportParameters[3] = new ReportParameter("InformacionEgresos", InformacionEgresos);
            reportParameters[4] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
            reportParameters[5] = new ReportParameter("FechaHora", FechaHora);
            reportParameters[6] = new ReportParameter("InformacionIngresos", InformacionIngresos);
            this.reportViewer1.LocalReport.SetParameters(reportParameters);
            this.reportViewer1.RefreshReport();
        }

        public void ImprimirFactura(int Repetir)
        {
            try
            {
                ReportParameter[] reportParameters = new ReportParameter[7];
                reportParameters[0] = new ReportParameter("InformacionEmpleado", InformacionEmpleado);
                reportParameters[1] = new ReportParameter("CantidadPedidos", CantidadPedidos);
                reportParameters[2] = new ReportParameter("ResumenResultados", ResumenResultados);
                reportParameters[3] = new ReportParameter("InformacionEgresos", InformacionEgresos);
                reportParameters[4] = new ReportParameter("IdentificacionTurno", IdentificacionTurno);
                reportParameters[5] = new ReportParameter("FechaHora", FechaHora);
                reportParameters[6] = new ReportParameter("InformacionIngresos", InformacionIngresos);
                this.reportViewer1.LocalReport.SetParameters(reportParameters);
                this.reportViewer1.RefreshReport();

                int contador = 0;
                while (contador != Repetir)
                {
                    objImpresion.Imprimir(reportViewer1.LocalReport);
                    objImpresion.Dispose();

                    contador += 1;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm(ex.Message);
            }
        }

    }
}
