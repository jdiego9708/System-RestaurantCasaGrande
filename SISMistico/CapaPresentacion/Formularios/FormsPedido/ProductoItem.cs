using CapaPresentacion.Formularios.Controles;
using CapaPresentacion.Formularios.FormsPlatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPedido
{
    public partial class ProductoItem : UserControl
    {
        Observacion observacion;
        PoperContainer container;

        public ProductoItem()
        {
            InitializeComponent();
            this.btnAdd.Click += BtnAdd_Click;
            this.btnRemove.Click += BtnRemove_Click;
            this.btnComment.Click += BtnComment_Click;
            this.pxImage.Click += PxImage_Click;

            this.btnEditar.Click += BtnEditar_Click;
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (this.Product == null) return;

            if (this.Product.Tipo_producto.Equals("PLATO"))
            {
                CapaEntidades.Models.Platos plato = (CapaEntidades.Models.Platos)this.Product.Product;

                FrmAgregarPlato frmplato = new FrmAgregarPlato()
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    Plato = plato,
                };
                frmplato.OnPlatoSuccess += Frmplato_OnPlatoSuccess;
                frmplato.ShowDialog();
            }
        }

        private void Frmplato_OnPlatoSuccess(object sender, EventArgs e)
        {
            CapaEntidades.Models.Platos plato = (CapaEntidades.Models.Platos)sender;
            this.Product.Product = plato;
            this.AsignarDatos(this.Product);
        }

        private void PxImage_Click(object sender, EventArgs e)
        {
            this.OnBtnAddClick?.Invoke(this.Product, e);
        }

        public event EventHandler OnBtnAddClick;
        public event EventHandler OnBtnRemoveClick;

        private void BtnComment_Click(object sender, EventArgs e)
        {
            if (this.observacion == null)
                this.observacion = new Observacion();

            this.observacion.txtObservacion.Text = 
                this.Product.Observaciones == null ? "" : this.Product.Observaciones;
            this.container = new PoperContainer(this.observacion);
            this.container.Closed += Container_Closed;
            this.container.Show(this.btnComment);
        }

        private void Container_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (this.observacion != null)
                this.Product.Observaciones = this.observacion.txtObservacion.Text;
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            this.OnBtnRemoveClick?.Invoke(this.Product, e);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            this.OnBtnAddClick?.Invoke(this.Product, e);
        }

        private void AsignarDatos(ProductBinding product)
        {
            this.txtInfo.Text = product.Informacion;
            if (product.IsAddBD)
            {
                this.btnRemove.Visible = true;
                this.btnAdd.Visible = false;
                this.pxImage.Enabled = false;
            }
            else
            {
                if (product.IsEditar)
                {
                    this.btnRemove.Visible = true;
                    this.btnAdd.Visible = false;
                    this.pxImage.Enabled = false;
                }
                else
                {
                    this.btnRemove.Visible = false;
                    this.btnAdd.Visible = true;
                    this.pxImage.Enabled = true;
                }

            }
        }

        private Image _imageProduct;
        private ProductBinding _product;

        public ProductBinding Product
        {
            get => _product;
            set
            {
                _product = value;
                this.AsignarDatos(value);
                if (value.Cantidad < 1)
                {
                    this.txtInfo.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                else
                {
                    this.txtInfo.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
            }
        }

        public Image ImageProduct
        {
            get => _imageProduct;
            set
            {
                _imageProduct = value;
                this.pxImage.Image = value;
            }
        }
    }
}

