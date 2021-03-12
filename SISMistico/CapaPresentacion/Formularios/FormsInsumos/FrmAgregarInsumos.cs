using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsDetalles;

namespace CapaPresentacion.Formularios.FormsInsumos
{
    public partial class FrmAgregarInsumos : Form
    {
        PoperContainer poperContainer;
        ConvertidorPorciones convertidorPorciones;
        public FrmAgregarInsumos()
        {
            InitializeComponent();
            this.convertidorPorciones = new ConvertidorPorciones();
            this.poperContainer = new PoperContainer(this.convertidorPorciones);

            this.Load += FrmAgregarInsumos_Load;
            this.btnAyuda.Click += BtnAyuda_Click;

            this.txtCantidad.KeyPress += TxtCantidad_KeyPress;
            this.txtCantidad.LostFocus += TxtPrecio_LostFocus;
            this.txtCantidad.GotFocus += TxtPrecio_GotFocus;

            this.txtCantidadMinima.KeyPress += TxtCantidad_KeyPress;
            this.txtCantidadMinima.LostFocus += TxtPrecio_LostFocus;
            this.txtCantidadMinima.GotFocus += TxtPrecio_GotFocus;

            this.btnCancelar.Click += BtnCancelar_Click;
            this.btnGuardar.Click += BtnGuardar_Click;
        }

        private void TxtPrecio_LostFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Text.Equals(""))
            {
                txt.Text = "0";
            }
        }

        private void TxtPrecio_GotFocus(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            txt.SelectAll();
        }

        public void ObtenerDatos(List<string> vs)
        {
            this.Text = "Editar un insumo";
            this.btnGuardar.Text = "Guardar cambios";

            this.ListaTipo =
                    LlenarListas.LlenarListaTipoInsumos(this.ListaTipo);
            this.ListaTipoMedida =
                LlenarListas.LlenarListaTipoMedidaInsumos(this.ListaTipoMedida);
            this.ListaTipoMedida.SelectedIndex = 0;
            this.ListaTipoMedida.Enabled = false;

            this.Tag = vs[0];
            this.txtNombre.Text = vs[1];
            this.ListaTipo.SelectedValue = vs[2];
            this.txtCantidad.Text = vs[4];
            this.ListaTipoMedida.Text = vs[5];
            this.txtObservaciones.Text = vs[6];

            this.txtCantidadMinima.Text = vs[7];
        }

        private List<string> Variables()
        {
            if (this.IsEditar)
            {
                List<string> vs = new List<string>
                {
                Convert.ToString(this.Tag), this.txtNombre.Text, Convert.ToString(this.ListaTipo.SelectedValue),
                this.txtCantidad.Text, this.ListaTipoMedida.Text, this.txtObservaciones.Text, this.txtCantidadMinima.Text
                };
                return vs;
            }
            else
            {
                List<string> vs = new List<string>
                {
                this.txtNombre.Text, Convert.ToString(this.ListaTipo.SelectedValue),
                this.txtCantidad.Text, this.ListaTipoMedida.Text, this.txtObservaciones.Text, this.txtCantidadMinima.Text
                };
                return vs;
            }
        }

        private bool Comprobaciones()
        {
            bool result = true;
            if (this.txtNombre.Text.Equals(""))
            {
                result = false;
                this.errorProvider1.SetError(this.txtNombre, "Campo obligatorio");
            }

            if (this.ListaTipo.Text.Equals(""))
            {
                result = false;
                this.errorProvider1.SetError(this.ListaTipo, "Campo obligatorio");
            }

            if (this.txtCantidad.Text.Equals(""))
            {
                result = false;
                this.errorProvider1.SetError(this.txtCantidad, "Campo obligatorio");
            }

            if (this.ListaTipoMedida.Text.Equals(""))
            {
                result = false;
                this.errorProvider1.SetError(this.ListaTipoMedida, "Campo obligatorio");
            }

            return result;
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                string rpta = "";
                if (this.Comprobaciones())
                {
                    if (this.IsEditar)
                    {
                        rpta = NInsumos.EditarInsumos(this.Variables());
                    }
                    else
                    {
                        rpta = NInsumos.InsertarInsumos(this.Variables());
                    }

                    if (rpta.Equals("OK"))
                    {
                        Mensajes.MensajeOkForm("Se guardó correctamente el insumo");
                        if (this.IsEditar)
                        {
                            this.Close();
                        }
                        else
                        {
                            if (this.chkCerrar.Checked)
                            {
                                this.Close();
                            }
                            else
                            {
                                this.Tag = null;
                                this.txtNombre.Clear();
                                this.txtCantidad.Clear();
                                this.txtObservaciones.Clear();
                                this.ListaTipo.SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Mensajes.MensajeErrorCompleto(this.Name, "BtnGuardar_Click",
                    "Hubo un error al guardar o editar un insumo", ex.Message);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TxtCantidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsLetter(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void BtnAyuda_Click(object sender, EventArgs e)
        {
            this.poperContainer.Show(this.btnAyuda);
        }

        private void FrmAgregarInsumos_Load(object sender, EventArgs e)
        {
            if (!this.IsEditar)
            {
                this.ListaTipo =
                    LlenarListas.LlenarListaTipoInsumos(this.ListaTipo);
                this.ListaTipoMedida =
                    LlenarListas.LlenarListaTipoMedidaInsumos(this.ListaTipoMedida);
                this.ListaTipoMedida.SelectedIndex = 0;
                this.ListaTipoMedida.Enabled = false;
            }
        }

        private bool isEditar;

        public bool IsEditar { get => isEditar; set => isEditar = value; }
    }
}
