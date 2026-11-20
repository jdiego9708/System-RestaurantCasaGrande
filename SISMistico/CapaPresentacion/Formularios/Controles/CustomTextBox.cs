using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion
{
    public partial class CustomTextBox : UserControl
    {
        public CustomTextBox()
        {
            InitializeComponent();
            this.txtBusqueda.LostFocus += TxtBusqueda_LostFocus;
            this.txtBusqueda.GotFocus += TxtBusqueda_GotFocus;
            this.Load += CustomTextBox_Load;
            this.txtBusqueda.TextChanged += TxtBusqueda_TextChanged;
            this.txtBusqueda.TextChanged += new EventHandler(OnTextoChanged);
            this.txtBusqueda.SizeChanged += TxtBusqueda_SizeChanged;
            this.txtBusqueda.KeyPress += new KeyPressEventHandler(OnKeyPress);
        }

        private void TxtBusqueda_SizeChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (this.px != null)
            {
                this.panel1.Controls.Remove(px);
            }

            if (this.Visible_px)
            {
                int size = txt.Height;
                this.px = new PictureBox()
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = Properties.Resources.lupa,
                    Size = new Size(size, size),
                    Location =
                    new Point(txt.Width - size, txt.Location.Y),
                    Cursor = Cursors.Hand,
                };
                this.px.Click += new EventHandler(OnPxClick);
                this.panel1.Controls.Add(px);
                px.BringToFront();
            }
        }

        #region EVENTOS 
        public event EventHandler onPxClick;

        private void OnPxClick(object sender, EventArgs e)
        {
            bool cancelar = false;
            if (this.txtBusqueda.Text.Equals(""))
            {
                cancelar = true;
            }
            else if (this.txtBusqueda.Text.Equals(this.Texto_inicial))
            {
                cancelar = true;
            }

            if (cancelar == false)
            {
                if (this.onPxClick != null)
                    this.onPxClick(this, e);
            }
        }

        public event KeyPressEventHandler onKeyPress;

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            bool cancelar = false;
            if (txt.Text.Equals(""))
            {
                cancelar = true;
            }
            else if (txt.Text.Equals(this.Texto_inicial))
            {
                cancelar = true;
            }

            if (cancelar == false)
            {
                if (this.onKeyPress != null)
                    this.onKeyPress(this, e);
            }
        }

        private EventHandler onTextoChanged;

        public event EventHandler ChangedText
        {
            add
            {
                onTextoChanged += value;
            }
            remove
            {
                onTextoChanged -= value;
            }
        }

        protected virtual void OnTextoChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            bool cancelar = false;
            if (txt.Text.Equals(""))
            {
                cancelar = true;
            }
            else if (txt.Text.Equals(this.Texto_inicial))
            {
                cancelar = true;
            }

            if (cancelar == false)
            {
                if (this.onTextoChanged != null)
                    this.onTextoChanged(this, e);
            }
        }
        #endregion

        PictureBox px;

        private string texto;
        private string texto_inicial;
        private bool _visible_px;

        public string Texto_inicial { get => texto_inicial;
            set
            {
                texto_inicial = value;
                this.EstablecerTextoInicial();
            }
        }
        public bool Visible_px { get => _visible_px; set => _visible_px = value; }
        public string Texto { get => texto; set => texto = value; }

        private const float DefaultTextSize = 9.75F;
        private float textSize = DefaultTextSize;

        /// <summary>
        /// Tamano de la letra del cuadro de texto, en puntos. Con el valor por defecto el control queda como
        /// siempre; con otro valor la altura del control sigue a la del texto.
        /// </summary>
        [System.ComponentModel.DefaultValue(DefaultTextSize)]
        public float TextSize
        {
            get => this.textSize;
            set
            {
                this.textSize = value;
                this.UpdateTextFont();
                if (this.textSize != DefaultTextSize)
                {
                    // El TextBox recalcula su alto al cambiar de fuente; el panel y el control lo acompanan.
                    this.panel1.Height = this.txtBusqueda.Height;
                    this.Height = this.panel1.Height;
                }
            }
        }

        // Cursiva mientras muestra el texto inicial; normal cuando el usuario escribe.
        private void UpdateTextFont()
        {
            TextBox txt = this.txtBusqueda;
            if (txt.Text.Equals("") || txt.Text.Equals(this.Texto_inicial))
            {
                txt.Font =
                        new Font("Segoe UI", this.textSize, FontStyle.Italic, GraphicsUnit.Point, ((byte)(0)));
                txt.ForeColor = Color.FromArgb(64, 64, 64);
            }
            else
            {
                txt.Font =
                        new Font("Segoe UI", this.textSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
                txt.ForeColor = Color.Black;
            }
        }

        private void TxtBusqueda_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)sender;
            this.Texto = txt.Text;
            this.UpdateTextFont();
        }

        private void CustomTextBox_Load(object sender, EventArgs e)
        {
            this.txtBusqueda.Text = this.Texto_inicial;
            if (this.Texto != null)
            {
                this.txtBusqueda.Text = this.Texto;
            }
        }

        private void TxtBusqueda_GotFocus(object sender, EventArgs e)
        {
            this.panel1.BackColor = Color.Teal;
            if (this.txtBusqueda.Text.Equals(this.Texto_inicial))
            {
                this.txtBusqueda.Clear();
            }
        }

        private void TxtBusqueda_LostFocus(object sender, EventArgs e)
        {
            this.panel1.BackColor = Color.Silver;
            if (this.txtBusqueda.Text.Equals(""))
            {
                this.txtBusqueda.Text = this.Texto_inicial;
            }
        }

        public void EstablecerTextoInicial()
        {
            this.txtBusqueda.Text = this.Texto_inicial;
        }

        public void AsignarTexto()
        {
            this.txtBusqueda.Text = this.Texto;
        }

    }
}
