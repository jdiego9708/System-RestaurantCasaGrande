using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CapaPresentacion.Formularios.FormsClientes;
using CapaPresentacion.Formularios.FormsEmpleados;

namespace CapaPresentacion.Formularios.FormsVentas
{
    public partial class Filtrosventa : UserControl
    {
        public Filtrosventa()
        {
            InitializeComponent();
            this.rdNinguno.CheckedChanged += Rd_CheckedChanged;
            this.rdCliente.CheckedChanged += Rd_CheckedChanged;
            this.rdEmpleado.CheckedChanged += Rd_CheckedChanged;
            this.rdMesa.CheckedChanged += Rd_CheckedChanged;
            this.Load += Filtrosventa_Load;
        }
        public ComboBox listaMesas = new ComboBox();

        public string ObtenerValorPanel(Panel panel)
        {
            string rpta = "";
            foreach (Control control in panel.Controls)
            {
                if (control is RadioButton rd)
                {
                    if (rd.Checked)
                    {
                        rpta = rd.Text.ToUpper();
                        break;
                    }
                }
            }
            return rpta;
        }

        private void Filtrosventa_Load(object sender, EventArgs e)
        {
            this.rdNinguno.Checked = true;
        }

        private void Rd_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rd = (RadioButton)sender;
            if (rd.Checked)
            {
                switch (rd.Text.ToUpper())
                {
                    case "NINGUNO":
                        this.btnSeleccione.Enabled = false;
                        this.txtTipo.Enabled = false;
                        break;
                    case "CLIENTE":
                        this.btnSeleccione.Enabled = true;
                        this.btnSeleccione.Text = "Seleccione un cliente";
                        this.txtTipo.Enabled = true;
                        this.txtTipo.Text = "Información cliente";
                        this.txtTipo.Visible = true;
                        this.Controls.Remove(this.listaMesas);
                        break;
                    case "EMPLEADO":
                        this.btnSeleccione.Enabled = true;
                        this.btnSeleccione.Text = "Seleccione un empleado";
                        this.txtTipo.Enabled = true;
                        this.txtTipo.Text = "Información empleado";
                        this.txtTipo.Visible = true;
                        this.Controls.Remove(this.listaMesas);
                        break;
                    case "MESA":
                        this.btnSeleccione.Enabled = false;
                        this.btnSeleccione.Text = "Seleccione una mesa de la lista";
                        this.txtTipo.Enabled = true;
                        this.txtTipo.Text = "Información empleado";
                        this.txtTipo.Visible = false;

                        listaMesas.Location = this.txtTipo.Location;
                        listaMesas.Size = this.txtTipo.Size;
                        listaMesas.DropDownStyle = ComboBoxStyle.DropDownList;
                        listaMesas = LlenarListas.LlenarListaMesas(this.listaMesas);
                        this.Controls.Add(listaMesas);
                        break;
                }
                this.btnSeleccione.Tag = rd.Text.ToUpper();
            }
        }
    }
}
