using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class FrmObservarMesas : Form
    {
        #region PATRON SINGLETON
        private static FrmObservarMesas _Instancia;
        public static FrmObservarMesas GetInstancia()
        {
            if (_Instancia == null || _Instancia.IsDisposed)
            {
                _Instancia = new FrmObservarMesas();
            }
            return _Instancia;
        }
        #endregion

        private FrmObservarMesas()
        {
            InitializeComponent();
            BuildTablesWorkspace();
            this.Load += FrmNuevoPedido_Load;
            this.btnCargar.Click += BtnCargar_Click;
            this.btnDomicilio.Click += BtnDomicilio_Click;

            this.timerDispositivos = new System.Windows.Forms.Timer { Interval = 5000 };
            this.timerDispositivos.Tick += (s, e) => this.ActualizarDispositivos();
            this.toolTipDispositivos = new ToolTip();
            this.Load += (s, e) =>
            {
                this.ActualizarDispositivos();
                this.timerDispositivos.Start();
            };
            this.FormClosed += (s, e) =>
            {
                this.timerDispositivos.Stop();
                this.timerDispositivos.Dispose();
                this.toolTipDispositivos.Dispose();
            };
        }

        private readonly System.Windows.Forms.Timer timerDispositivos;
        private readonly ToolTip toolTipDispositivos;

        /// <summary>
        /// Dispositivos móviles con actividad reciente en la API (una petición con sesión en los últimos 90 s).
        /// Se oculta si la API no está activa; el tooltip lista empleado, IP y hace cuánto hablaron.
        /// </summary>
        private void ActualizarDispositivos()
        {
            bool activa = CapaPresentacion.Api.ApiServer.Activo;
            this.panelDispositivos.Visible = activa;
            this.lblDispositivos.Visible = activa;
            if (!activa)
                return;

            List<CapaPresentacion.Api.ApiDispositivo> activos = CapaPresentacion.Api.ApiAuth.DispositivosActivos();
            this.lblDispositivos.Text = activos.Count + (activos.Count == 1 ? " DISPOSITIVO" : " DISPOSITIVOS");
            this.panelDispositivos.BackColor = activos.Count > 0 ? TableWorkspaceColors.Available : Color.Gray;

            string detalle = activos.Count == 0
                ? "Ningún dispositivo ha consultado la API en el último minuto y medio."
                : string.Join(Environment.NewLine, activos.ConvertAll(d =>
                    (string.IsNullOrEmpty(d.Empleado) ? "(sin nombre)" : d.Empleado) + " · " + d.Ip +
                    " · hace " + Math.Max(0, (int)(DateTime.Now - d.Ultima).TotalSeconds) + " s"));
            this.toolTipDispositivos.SetToolTip(this.panelDispositivos, detalle);
            this.toolTipDispositivos.SetToolTip(this.lblDispositivos, detalle);
        }


        private void BtnDomicilio_Click(object sender, EventArgs e)
        {
            if (this.PedidosDomicilios.Count > 0)
            {
                FrmDomicilios frmDomicilios = new FrmDomicilios
                {
                    WindowState = FormWindowState.Maximized,
                };
                frmDomicilios.Show();
            }
            else
            {
                FrmRealizarPedido FrmPedido = new FrmRealizarPedido
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_mesa = 0,
                    Numero_mesa = 0,
                    WindowState = FormWindowState.Maximized,
                    IsDomicilio = true,
                };
                FrmPedido.OnPedidoDomicilioSuccess += FrmPedido_OnPedidoDomicilioSuccess;
                FrmPedido.ShowDialog();
            }
        }

        private void FrmPedido_OnPedidoDomicilioSuccess(object sender, EventArgs e)
        {
            int id_pedido = (int)sender;

            if (this.PedidosDomicilios == null)
                this.PedidosDomicilios = new List<Pedidos>();

            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public void LiberarMesa(int id_mesa)
        {
            foreach (Control control in this.panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (btn.Id_mesa == id_mesa)
                    {
                        btn.ObtenerEstado("DISPONIBLE", btn.Id_pedido, false);
                        btn.RefreshDetails();
                        break;
                    }
                }
            }
        }

        public void MesaSaliendo(int id_mesa, int id_pedido)
        {
            foreach (Control control in this.panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (btn.Id_mesa == id_mesa)
                    {
                        btn.ObtenerEstado("SALIENDO", id_pedido, true);
                        btn.RefreshDetails();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Vuelve a leer el estado de todas las mesas desde la base de datos, sin escribir nada.
        /// La usa la API tras cambiar un pedido (agregar/quitar líneas, pasar a SALIENDO o cancelar):
        /// ObtenerPedido escribe el estado, así que no sirve para refrescar.
        /// </summary>
        public void RecargarMesas()
        {
            this.CargarMesas();
        }

        public void ObtenerPedido(int id_pedido, int numero_mesa, string estado)
        {
            foreach (Control control in panelMesas.Controls)
            {
                if (control is CustomButtonMesa btn)
                {
                    if (Convert.ToInt32(btn.Tag) == numero_mesa)
                    {
                        btn.ObtenerEstado(estado, id_pedido, true);
                        btn.RefreshDetails();
                        break;
                    }
                }
            }
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            this.CargarMesas();
        }

        private void FrmNuevoPedido_Load(object sender, EventArgs e)
        {
            this.CargarMesas();
            this.CargarDomicilios(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        public void CargarDomicilios(string fecha)
        {
            this.PedidosDomicilios = new List<Pedidos>();
            DataTable dtPedidos = NPedido.BuscarPedidos("DOMICILIOS COMPLETO", fecha);
            if (dtPedidos != null)
            {
                int contadorPendientes = 0;
                int contadorOtros = 0;
                foreach (DataRow row in dtPedidos.Rows)
                {
                    Pedidos pedido = new Pedidos(row);
                    if (pedido.Estado_pedido.Equals("PENDIENTE"))
                    {
                        this.PedidosDomicilios.Add(pedido);
                        contadorPendientes += 1;
                    }
                    else
                    {
                        this.PedidosDomicilios.Add(pedido);
                        contadorOtros += 1;
                    }
                }

                this.btnDomicilio.Text = "Domicilios P:(" + contadorPendientes + ") T:(" + this.PedidosDomicilios.Count + ")";
            }
        }

        private bool loadingTables, reloadRequested;

        // Las consultas corren en segundo plano y las tarjetas se montan de una sola vez con el panel oculto;
        // antes cada mesa hacía una escritura en la base de datos en el hilo de la interfaz y se pintaba una a una.
        private async void CargarMesas()
        {
            // Si llega otra recarga mientras se carga (p. ej. un cambio desde el móvil), se repite al terminar.
            if (loadingTables) { reloadRequested = true; return; }
            loadingTables = true;
            MensajeEspera.ShowWait("Cargando mesas");
            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                DataTable tablaMesasConPedido = null, tablaMesas = null, tablesDetails = null;
                await Task.Run(() =>
                {
                    tablaMesasConPedido = NPedido.BuscarPedidos("MESAS CON PEDIDO", today);
                    tablaMesas = NPedido.BuscarPedidos("MESAS COMPLETAS", "");
                    // Total, hora y mesero salen de la misma consulta que alimenta al móvil, para que ambos muestren
                    // lo mismo; si falla, las tarjetas quedan solo con el estado.
                    try { tablesDetails = NApiMovil.Mesas(DateTime.Today); }
                    catch (Exception) { }
                });
                if (IsDisposed) return;

                this.Numero_mesas = tablaMesas.Rows.Count - 2;
                // Índices por número de mesa: evita un DataTable.Select por cada mesa.
                var mesaRows = IndexByTable(tablaMesas);
                var pedidoRows = IndexByTable(tablaMesasConPedido);
                var detailRows = IndexByTable(tablesDetails);

                string estado = "DISPONIBLE";
                int id_pedido = 0;
                int id_mesa = 0;
                var controles = new List<Control>();
                var pendingSync = new List<List<string>>();
                for (int contador = 1; contador <= this.Numero_mesas; contador++)
                {
                    DataRow row;
                    if (mesaRows.TryGetValue(contador, out row))
                        id_mesa = Convert.ToInt32(row["Id_mesa"]);

                    if (tablaMesasConPedido != null)
                    {
                        if (pedidoRows.TryGetValue(contador, out row))
                        {
                            id_pedido = Convert.ToInt32(row["Id_pedido"]);
                            estado = Convert.ToString(row["Estado_pedido"]);
                        }
                        else
                        {
                            id_pedido = 0;
                            estado = "DISPONIBLE";
                        }
                    }

                    // Solo se usa la fila del móvil si habla del mismo pedido que el mapa.
                    DataRow detailRow = null;
                    if (id_pedido != 0 && detailRows.TryGetValue(contador, out row)
                        && Convert.ToInt32(row["Id_pedido"]) == id_pedido)
                        detailRow = row;

                    CustomButtonMesa ButtonMesa = new CustomButtonMesa
                    {
                        Details = TableCardText.Detail(estado, detailRow),
                        Estado_mesa = estado,
                        Texto = "Mesa " + (contador),
                        Tag = contador,
                        Numero_mesa = contador,
                        Id_mesa = id_mesa,
                        Id_pedido = id_pedido
                    };
                    ButtonMesa.ApplyState(estado, id_pedido);
                    ButtonMesa.OnBtnCambiarMesaClick += ButtonMesa_OnBtnCambiarMesaClick;
                    ButtonMesa.OnBtnCancelarPedidoClick += ButtonMesa_OnBtnCancelarPedidoClick;
                    ButtonMesa.OnBtnEditarPedidoClick += ButtonMesa_OnBtnEditarPedidoClick;
                    ButtonMesa.OnBtnFacturarPedidoClick += ButtonMesa_OnBtnFacturarPedidoClick;
                    ButtonMesa.OnBtnPrecuentaPedidoClick += ButtonMesa_OnBtnPrecuentaPedidoClick;
                    ButtonMesa.VisualStateChanged += (sender, args) => UpdateTableWorkspace();
                    controles.Add(ButtonMesa);
                    pendingSync.Add(new List<string> { id_pedido.ToString(), estado, id_mesa.ToString() });
                }

                // Montaje en bloque: panel oculto y sin layout parcial, un solo pintado al terminar.
                panelMesas.SuspendLayout();
                panelMesas.Visible = false;
                try
                {
                    foreach (Control old in new List<Control>(this.panelMesas.Controls.Cast<Control>())) old.Dispose();
                    this.panelMesas.Controls.Clear();
                    this.panelMesas.Controls.AddRange(controles.ToArray());
                    UpdateTableWorkspace();
                }
                finally
                {
                    panelMesas.ResumeLayout(true);
                    panelMesas.Visible = true;
                }

                // Sincroniza el estado en la base de datos como antes, pero fuera del hilo de la interfaz.
                Task.Run(() =>
                {
                    foreach (var variables in pendingSync)
                    {
                        try { NPedido.CambiarEstadoPedido(variables); }
                        catch (Exception) { }
                    }
                });
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "CargarMesas",
                    "Hubo un error al cargar las mesas", ex.Message);
            }
            finally
            {
                loadingTables = false;
                MensajeEspera.CloseForm();
                if (reloadRequested && !IsDisposed) { reloadRequested = false; CargarMesas(); }
            }
        }

        // Primera fila de cada número de mesa (misma semántica que el DataTable.Select anterior).
        private static Dictionary<int, DataRow> IndexByTable(DataTable table)
        {
            var index = new Dictionary<int, DataRow>();
            if (table == null || !table.Columns.Contains("Num_mesa")) return index;
            foreach (DataRow row in table.Rows)
            {
                int number;
                if (int.TryParse(Convert.ToString(row["Num_mesa"]), out number) && !index.ContainsKey(number))
                    index[number] = row;
            }
            return index;
        }

        private void ButtonMesa_OnBtnPrecuentaPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                IsPrecuenta = true
            };
            facturarPedido.ObtenerPedido(pedido.Id_pedido);
            facturarPedido.Show();
        }

        private void ButtonMesa_OnBtnFacturarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            FrmFacturarPedido facturarPedido = new FrmFacturarPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                IsPrecuenta = false
            };
            facturarPedido.ObtenerPedido(pedido.Id_pedido);
            facturarPedido.Show();
        }

        private void ButtonMesa_OnBtnEditarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            DatosInicioSesion datos = DatosInicioSesion.GetInstancia();
            FrmPedido FrmPedido = new FrmPedido
            {
                StartPosition = FormStartPosition.CenterScreen,
                Numero_mesa = pedido.Mesa.Num_mesa,
                Tipo_servicio = "MESA",
                EmpleadoSelected = datos.EmpleadoClaveMaestra,
                ClienteSelected = pedido.Cliente,
                MesaSelected = pedido.Mesa,
                WindowState = FormWindowState.Maximized,
                Pedido = pedido,
                IsEditar = true,
            };
            FrmPedido.ShowDialog();
        }

        private void ButtonMesa_OnBtnCancelarPedidoClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;
            string rpta = NPedido.CancelarPedido(pedido.Id_pedido, pedido.Observaciones_pedido);
            if (rpta.Equals("OK"))
            {
                this.CargarMesas();
            }
            else
            {
                Mensajes.MensajeInformacion("Hubo un error al cancelar el pedido");
            }
        }

        private void ButtonMesa_OnBtnCambiarMesaClick(object sender, EventArgs e)
        {
            Pedidos pedido = (Pedidos)sender;

            if (pedido != null)
            {
                FrmCambiarMesa frmCambiarMesa = new FrmCambiarMesa
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Id_pedido_origen = pedido.Id_pedido,
                    Numero_mesa_origen = pedido.Mesa.Num_mesa,
                    Id_mesa_origen = pedido.Id_mesa,
                    Estado_mesa_origen = pedido.Estado_pedido
                };
                frmCambiarMesa.FormClosed += FrmCambiarMesa_FormClosed;
                frmCambiarMesa.ShowDialog();
            }
        }

        private void FrmCambiarMesa_FormClosed(object sender, FormClosedEventArgs e)
        {
            FrmCambiarMesa frm = (FrmCambiarMesa)sender;
            if (frm.DialogResult == DialogResult.OK)
            {
                bool result = true;
                if (frm.Numero_mesa_destino <= this.Numero_mesas)
                {
                    foreach (Control control in this.panelMesas.Controls)
                    {
                        if (control is CustomButtonMesa btn)
                        {
                            if (btn.Numero_mesa == frm.Numero_mesa_destino)
                            {
                                if (btn.Estado_mesa.Equals("DISPONIBLE"))
                                {
                                    btn.ObtenerEstado(frm.Estado_mesa_origen, frm.Id_pedido_origen, true);
                                    btn.RefreshDetails();
                                }
                                else
                                {
                                    Mensajes.MensajeInformacion("La mesa de destino debe estar disponible", "Entendido");
                                    result = false;
                                }
                                break;
                            }
                        }
                    }
                    if (result)
                    {
                        this.LiberarMesa(frm.Id_mesa_origen);
                    }
                }
                else
                {
                    Mensajes.MensajeErrorForm("La mesa no existe o no está habilitada");
                }
            }
        }

        int _numero_mesas;

        private List<Pedidos> _pedidosDomicilios;
        public int Numero_mesas { get => _numero_mesas; set => _numero_mesas = value; }
        public List<Pedidos> PedidosDomicilios
        {
            get => _pedidosDomicilios;
            set
            {
                _pedidosDomicilios = value;
                if (value.Count > 0)
                    this.btnDomicilio.Text = "Domicilios (" + value.Count + ")";
                else
                    this.btnDomicilio.Text = "Domicilio";
            }
        }
    }
}
