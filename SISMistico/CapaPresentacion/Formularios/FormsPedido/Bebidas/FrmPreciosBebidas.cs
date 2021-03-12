using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido.Bebidas
{
    public partial class FrmPreciosBebidas : Form
    {
        public FrmPreciosBebidas()
        {
            InitializeComponent();
            this.btnTerminar.Click += BtnTerminar_Click;
        }

        private int precio_cobrar;

        public int Precio_cobrar { get => precio_cobrar; set => precio_cobrar = value; }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Insert)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (keyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
            return true;
        }

        private void BtnTerminar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public void ObtenerBebida(DataGridViewRow row)
        {

            List<int> precios = new List<int>();
            precios.Add(Convert.ToInt32(row.Cells["Precio_bebida"].Value));
            precios.Add(Convert.ToInt32(row.Cells["Precio_trago"].Value));
            precios.Add(Convert.ToInt32(row.Cells["Precio_trago_doble"].Value));

            List<string> nombresRd = new List<string>();
            nombresRd.Add("Precio normal");
            nombresRd.Add("Precio trago");
            nombresRd.Add("Precio trago doble");

            int locationY = 0;
            bool primero = true;
            for (int i = 0; i <= precios.Count - 1; i++)
            {
                RadioButton rd = new RadioButton
                {
                    Text = nombresRd[i] + " - " + precios[i],
                    Location = new Point(0, locationY),
                    Tag = precios[i],
                    Cursor = Cursors.Hand,
                    AutoSize = true
                };
                rd.CheckedChanged += Rd_CheckedChanged;
                this.panelPreciosBebidas.Controls.Add(rd);
                if (primero)
                    rd.Checked = true;

                primero = false;
                locationY = rd.Location.Y + rd.Height;
            }

        }

        private void Rd_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rd = (RadioButton)sender;
            if (rd.Checked)
            {
                this.Precio_cobrar = Convert.ToInt32(rd.Tag);
            }
        }
    }
}
