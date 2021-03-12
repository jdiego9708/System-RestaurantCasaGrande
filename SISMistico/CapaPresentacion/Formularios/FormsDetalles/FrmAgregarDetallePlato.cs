using CapaNegocio;
using CapaPresentacion.Formularios.FormsInsumos;
using CapaPresentacion.Formularios.FormsPlatos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsDetalles
{
    public partial class FrmAgregarDetallePlato : Form
    {
        PoperContainer poperContainer;
        ConvertidorPorciones porciones;
        public FrmAgregarDetallePlato()
        {
            InitializeComponent();
            this.porciones = new ConvertidorPorciones();
            this.poperContainer = new PoperContainer(porciones);
            this.Load += FrmAgregarDetallePlato_Load;
            this.txtPlato.Click += TxtPlato_Click;
            this.txtCantidad.LostFocus += TxtCantidad_LostFocus;
            this.btnAgregar.Click += BtnAgregar_Click;
            this.btnQuitarInsumos.Click += BtnQuitar_Click;
            this.btnEditar.Click += BtnEditar_Click;
            this.btnGuardar.Click += BtnGuardar_Click;
            this.btnAyuda.Click += BtnAyuda_Click;
            this.btnQuitarInsumosUsados.Click += BtnQuitarInsumosUsados_Click;
            this.btnCancelar.Click += BtnCancelar_Click;
            this.txtCantidad.GotFocus += TxtCantidad_GotFocus;
        }

        private void TxtCantidad_GotFocus(object sender, EventArgs e)
        {
            this.txtCantidad.Select();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnQuitarInsumosUsados_Click(object sender, EventArgs e)
        {
            if (this.dgvInsumosUsados.Rows.Count > 0)
            {
                DialogResult dialog;
                Mensajes.MensajePregunta("Se eliminará este insumo, ¿desea continuar?",
                    "Continuar", "Cancelar", out dialog);
                if (dialog == DialogResult.Yes)
                {
                    DataRowView currentDataRowView = (DataRowView)dgvInsumosUsados.CurrentRow.DataBoundItem;
                    DataRow row = currentDataRowView.Row;
                    List<string> vs = new List<string>
                    {
                        Convert.ToString(row["Id_detalle"]), Convert.ToString(0),
                        Convert.ToString(0), Convert.ToString(0), "ELIMINAR"
                    };
                    string rpta =
                        NPlatos.EditarDetallePlatos(vs);
                    if (rpta.Equals("OK"))
                    {
                        this.BuscarDetallePlatos("ID PLATO", Convert.ToString(this.txtPlato.Tag));
                        Mensajes.MensajeOkForm("Se eliminó correctamente");
                    }
                }
            }
        }

        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            this.poperContainer.Show(this.btnAyuda);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvInsumos.Rows.Count > 0)
                {
                    string rpta = "";
                    foreach (DataGridViewRow row in this.dgvInsumos.Rows)
                    {
                        List<string> vs = new List<string>
                        {
                            Convert.ToString(this.txtPlato.Tag), Convert.ToString(row.Cells["Id_insumo"].Value),
                            Convert.ToString(row.Cells["Cantidad_usada"].Value)
                        };
                        rpta = NPlatos.InsertarDetallePlatos(vs);
                        if (!rpta.Equals("OK"))
                            break;
                    }
                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm("Se insertaron todos los ingredientes correctamente");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al guardar los detalles del plato", ex.Message);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.dgvInsumosUsados.CurrentRow != null)
                {
                    DataGridViewRow row =
                        this.dgvInsumosUsados.CurrentRow;
                    int id_detalle = Convert.ToInt32(row.Cells["Id_detalle"].Value);
                    List<string> vs = new List<string>
                    {
                        Convert.ToString(id_detalle), Convert.ToString(0),
                        Convert.ToString(0), Convert.ToString(0), "ELIMINAR"
                    };
                    string rpta =
                        NPlatos.EditarDetallePlatos(vs);
                    if (rpta.Equals("OK"))
                    {
                        List<string> datos = new List<string>
                        {
                            Convert.ToString(row.Cells["Id_insumo"].Value), Convert.ToString(row.Cells["Nombre_insumo"].Value),
                            Convert.ToString(row.Cells["Id_tipo_insumo"].Value), Convert.ToString(row.Cells["Tipo_insumo"].Value)
                        };
                        this.txtInsumo.Text = Convert.ToString(row.Cells["Nombre_insumo"].Value);
                        this.txtInsumo.Tag = datos;
                    }
                    else
                    {
                        Mensajes.MensajeErrorForm("Hubo un problema al eliminar el insumo del plato");
                    }
                    this.BuscarDetallePlatos("ID PLATO", Convert.ToString(this.txtPlato.Tag));
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorForm("Hubo un error al editar un plato " + ex.Message);
            }
        }

        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            try
            {
                DataRowView currentDataRowView = (DataRowView)dgvInsumos.CurrentRow.DataBoundItem;
                DataRow row = currentDataRowView.Row;
                this.DtInsumos.Rows.Remove(row);
                this.BuscarDetallePlatos("ID PLATO", Convert.ToString(this.txtPlato.Tag));
            }
            catch (Exception)
            {
                //Mensajes.MensajeErrorForm(ex.Message);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                bool result = true;
                if (this.txtInsumo.Text.Equals(""))
                {
                    result = false;
                    errorProvider1.SetError(this.txtInsumo, "Por favor seleccione un insumo de la lista derecha");
                }

                if (this.txtCantidad.Text.Equals(""))
                {
                    result = false;
                    errorProvider1.SetError(this.txtInsumo, "Obligatorio");
                }

                if (result)
                {
                    //Casteamos el object del tag para obtener los datos
                    List<string> datos = (List<string>)this.txtInsumo.Tag;

                    DataRow row = this.DtInsumos.NewRow();
                    row["Id_insumo"] = datos[0];
                    row["Nombre_insumo"] = datos[1];
                    row["Id_tipo_insumo"] = datos[2];
                    row["Tipo_insumo"] = datos[3];
                    row["Cantidad_usada"] = this.txtCantidad.Text;
                    this.DtInsumos.Rows.Add(row);

                    this.txtCantidad.Text = "0";
                    this.txtInsumo.Text = string.Empty;
                    this.txtInsumo.Tag = null;
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnAgregar_Click",
                    "Hubo un error al agregar un insumo", ex.Message);
            }
        }

        private void TxtCantidad_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            int num;
            bool result = int.TryParse(txt.Text, out num);
            if (!result)
            {
                txt.Text = "0";
            }
        }

        private void BuscarDetallePlatos(string tipo_busqueda, string texto_busqueda)
        {
            try
            {
                DataTable table =
                    NPlatos.BuscarDetallePlatos(tipo_busqueda, texto_busqueda);
                this.dgvInsumosUsados.DataSource = table;
                if (table != null)
                {
                    this.lblins.Text = "Insumos usados " + table.Rows.Count;

                    string[] columnsHeader =
                    {
                        "Id detalle", "Id plato", "Id insumos", "Insumo", "Cantidad", "Cantidad usada", "Id tipo insumo", "Tipo"
                    };
                    bool[] columnsVisible =
                    {
                        false, false, false, true, false, true, false, false
                    };
                    this.dgvInsumosUsados =
                        DatagridString.ChangeHeaderTextAndVisible(this.dgvInsumosUsados, columnsHeader, columnsVisible);
                }
                else
                {
                    this.lblins.Text = "No hay insumos agregados";
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BuscarDetallePlatos",
                    "Hubo un error al buscar un detalle", ex.Message);
            }
        }

        public void ObtenerDatosPlato(List<string> datos)
        {
            int id_plato;
            bool result = int.TryParse(datos[0], out id_plato);
            if (result)
            {
                this.txtPlato.Tag = id_plato;
                this.txtPlato.Text = datos[1];
                this.BuscarDetallePlatos("ID PLATO", id_plato.ToString());
            }
        }

        public void DatosInsumos(List<string> datos)
        {
            //Guardo los datos en el tag del txtInsumo
            this.txtInsumo.Tag = datos;
            this.txtInsumo.Text = datos[1];
        }

        private void CrearTabla()
        {
            this.DtInsumos = new DataTable();
            this.DtInsumos.Columns.Add("Id_insumo", typeof(string));
            this.DtInsumos.Columns.Add("Nombre_insumo", typeof(string));
            this.DtInsumos.Columns.Add("Id_tipo_insumo", typeof(string));
            this.DtInsumos.Columns.Add("Tipo_insumo", typeof(string));
            this.DtInsumos.Columns.Add("Cantidad_usada", typeof(string));
            this.dgvInsumos.DataSource = this.DtInsumos;

            string[] columns_header =
            {
                "Id insumo", "Insumo", "Id tipo insumo", "Tipo", "Cantidad"
            };
            bool[] columns_visible =
            {
                false, true, false, true, true
            };

            this.dgvInsumos =
                DatagridString.ChangeHeaderTextAndVisible(this.dgvInsumos, columns_header, columns_visible);
        }

        private void TxtPlato_Click(object sender, EventArgs e)
        {
            FrmObservarPlatos platos = new FrmObservarPlatos();
            platos.agregarDetallePlato = this;
            platos.StartPosition = FormStartPosition.CenterScreen;
            platos.ShowDialog();
        }

        private void FrmAgregarDetallePlato_Load(object sender, EventArgs e)
        {
            this.dgvInsumos =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvInsumos);
            this.dgvInsumosUsados =
                ConfiguracionDatagridview.ConfigurationGrid(this.dgvInsumosUsados);

            if (this.panel1.Controls.Count > 0)
                this.panel1.Controls.Clear();

            FrmObservarInsumos insumos = new FrmObservarInsumos();
            insumos.detallePlato = this;
            insumos.FormBorderStyle = FormBorderStyle.None;
            insumos.Dock = DockStyle.Fill;
            insumos.TopLevel = false;
            this.panel1.Controls.Add(insumos);
            insumos.Show();

            this.CrearTabla();
            this.txtCantidad.Text = "0";
        }

        private DataTable dtInsumos;

        public DataTable DtInsumos { get => dtInsumos; set => dtInsumos = value; }
    }
}
