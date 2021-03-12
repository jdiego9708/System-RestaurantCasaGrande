namespace CapaPresentacion.Formularios.FormsVentas
{
    partial class FrmDetalleVenta
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDetalleVenta));
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelToolBox = new System.Windows.Forms.Panel();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.pxMaximize = new System.Windows.Forms.PictureBox();
            this.pxMinimize = new System.Windows.Forms.PictureBox();
            this.pxClose = new System.Windows.Forms.PictureBox();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.toolTipToolBox = new System.Windows.Forms.ToolTip(this.components);
            this.panelRight = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblIdPedido = new System.Windows.Forms.Label();
            this.lblMesa = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.lblTotalFinal = new System.Windows.Forms.Label();
            this.lblDescuento = new System.Windows.Forms.Label();
            this.lblSubTotal = new System.Windows.Forms.Label();
            this.lblPropina = new System.Windows.Forms.Label();
            this.dgvPagos = new System.Windows.Forms.DataGridView();
            this.label8 = new System.Windows.Forms.Label();
            this.dgvDetalle_pedido = new System.Windows.Forms.DataGridView();
            this.lblTotal_parcial = new System.Windows.Forms.Label();
            this.lblHora = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.lblCliente = new System.Windows.Forms.Label();
            this.lblEmpleado = new System.Windows.Forms.Label();
            this.lblId_venta = new System.Windows.Forms.Label();
            this.btnPrint = new System.Windows.Forms.Button();
            this.panelToolBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pxMaximize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxClose)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle_pedido)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Detalle de venta";
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelBottom.Location = new System.Drawing.Point(3, 543);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(659, 2);
            this.panelBottom.TabIndex = 4;
            // 
            // panelToolBox
            // 
            this.panelToolBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelToolBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelToolBox.Controls.Add(this.panelCenter);
            this.panelToolBox.Controls.Add(this.pxMaximize);
            this.panelToolBox.Controls.Add(this.pxMinimize);
            this.panelToolBox.Controls.Add(this.pxClose);
            this.panelToolBox.Controls.Add(this.label1);
            this.panelToolBox.Location = new System.Drawing.Point(2, 2);
            this.panelToolBox.Name = "panelToolBox";
            this.panelToolBox.Size = new System.Drawing.Size(658, 39);
            this.panelToolBox.TabIndex = 6;
            // 
            // panelCenter
            // 
            this.panelCenter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelCenter.Location = new System.Drawing.Point(0, 36);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(653, 3);
            this.panelCenter.TabIndex = 5;
            // 
            // pxMaximize
            // 
            this.pxMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxMaximize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxMaximize.Image = ((System.Drawing.Image)(resources.GetObject("pxMaximize.Image")));
            this.pxMaximize.Location = new System.Drawing.Point(608, -1);
            this.pxMaximize.Name = "pxMaximize";
            this.pxMaximize.Size = new System.Drawing.Size(23, 21);
            this.pxMaximize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxMaximize.TabIndex = 3;
            this.pxMaximize.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxMaximize, "Maximizar");
            // 
            // pxMinimize
            // 
            this.pxMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxMinimize.Image = ((System.Drawing.Image)(resources.GetObject("pxMinimize.Image")));
            this.pxMinimize.Location = new System.Drawing.Point(585, -2);
            this.pxMinimize.Name = "pxMinimize";
            this.pxMinimize.Size = new System.Drawing.Size(23, 21);
            this.pxMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxMinimize.TabIndex = 2;
            this.pxMinimize.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxMinimize, "Minimizar");
            // 
            // pxClose
            // 
            this.pxClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxClose.Image = ((System.Drawing.Image)(resources.GetObject("pxClose.Image")));
            this.pxClose.Location = new System.Drawing.Point(632, -1);
            this.pxClose.Name = "pxClose";
            this.pxClose.Size = new System.Drawing.Size(23, 21);
            this.pxClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxClose.TabIndex = 1;
            this.pxClose.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxClose, "Cerrar");
            // 
            // panelLeft
            // 
            this.panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(2, 548);
            this.panelLeft.TabIndex = 5;
            // 
            // panelTop
            // 
            this.panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelTop.Location = new System.Drawing.Point(0, -1);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(659, 2);
            this.panelTop.TabIndex = 5;
            // 
            // panelRight
            // 
            this.panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelRight.Location = new System.Drawing.Point(658, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(2, 547);
            this.panelRight.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.btnPrint);
            this.panel1.Controls.Add(this.lblIdPedido);
            this.panel1.Controls.Add(this.lblMesa);
            this.panel1.Controls.Add(this.txtObservaciones);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.lblTotalFinal);
            this.panel1.Controls.Add(this.lblDescuento);
            this.panel1.Controls.Add(this.lblSubTotal);
            this.panel1.Controls.Add(this.lblPropina);
            this.panel1.Controls.Add(this.dgvPagos);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.dgvDetalle_pedido);
            this.panel1.Controls.Add(this.lblTotal_parcial);
            this.panel1.Controls.Add(this.lblHora);
            this.panel1.Controls.Add(this.lblFecha);
            this.panel1.Controls.Add(this.lblCliente);
            this.panel1.Controls.Add(this.lblEmpleado);
            this.panel1.Controls.Add(this.lblId_venta);
            this.panel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(2, 38);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(655, 506);
            this.panel1.TabIndex = 7;
            // 
            // lblIdPedido
            // 
            this.lblIdPedido.AutoSize = true;
            this.lblIdPedido.Location = new System.Drawing.Point(5, 25);
            this.lblIdPedido.Name = "lblIdPedido";
            this.lblIdPedido.Size = new System.Drawing.Size(119, 21);
            this.lblIdPedido.TabIndex = 16;
            this.lblIdPedido.Text = "Pedido número:";
            // 
            // lblMesa
            // 
            this.lblMesa.AutoSize = true;
            this.lblMesa.Location = new System.Drawing.Point(559, 4);
            this.lblMesa.Name = "lblMesa";
            this.lblMesa.Size = new System.Drawing.Size(50, 21);
            this.lblMesa.TabIndex = 15;
            this.lblMesa.Text = "Mesa:";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Location = new System.Drawing.Point(9, 430);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservaciones.Size = new System.Drawing.Size(288, 70);
            this.txtObservaciones.TabIndex = 14;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 406);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(115, 21);
            this.label13.TabIndex = 13;
            this.label13.Text = "Observaciones:";
            // 
            // lblTotalFinal
            // 
            this.lblTotalFinal.AutoSize = true;
            this.lblTotalFinal.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalFinal.Location = new System.Drawing.Point(347, 407);
            this.lblTotalFinal.Name = "lblTotalFinal";
            this.lblTotalFinal.Size = new System.Drawing.Size(58, 25);
            this.lblTotalFinal.TabIndex = 12;
            this.lblTotalFinal.Text = "Total:";
            // 
            // lblDescuento
            // 
            this.lblDescuento.AutoSize = true;
            this.lblDescuento.Location = new System.Drawing.Point(303, 348);
            this.lblDescuento.Name = "lblDescuento";
            this.lblDescuento.Size = new System.Drawing.Size(86, 21);
            this.lblDescuento.TabIndex = 11;
            this.lblDescuento.Text = "Descuento:";
            // 
            // lblSubTotal
            // 
            this.lblSubTotal.AutoSize = true;
            this.lblSubTotal.Location = new System.Drawing.Point(303, 322);
            this.lblSubTotal.Name = "lblSubTotal";
            this.lblSubTotal.Size = new System.Drawing.Size(71, 21);
            this.lblSubTotal.TabIndex = 10;
            this.lblSubTotal.Text = "Subtotal:";
            // 
            // lblPropina
            // 
            this.lblPropina.AutoSize = true;
            this.lblPropina.Location = new System.Drawing.Point(303, 301);
            this.lblPropina.Name = "lblPropina";
            this.lblPropina.Size = new System.Drawing.Size(67, 21);
            this.lblPropina.TabIndex = 9;
            this.lblPropina.Text = "Propina:";
            // 
            // dgvPagos
            // 
            this.dgvPagos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPagos.Location = new System.Drawing.Point(9, 301);
            this.dgvPagos.Name = "dgvPagos";
            this.dgvPagos.Size = new System.Drawing.Size(288, 102);
            this.dgvPagos.TabIndex = 8;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 276);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(134, 21);
            this.label8.TabIndex = 7;
            this.label8.Text = "Métodos de pago:";
            // 
            // dgvDetalle_pedido
            // 
            this.dgvDetalle_pedido.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDetalle_pedido.Location = new System.Drawing.Point(9, 86);
            this.dgvDetalle_pedido.Name = "dgvDetalle_pedido";
            this.dgvDetalle_pedido.Size = new System.Drawing.Size(636, 187);
            this.dgvDetalle_pedido.TabIndex = 6;
            // 
            // lblTotal_parcial
            // 
            this.lblTotal_parcial.AutoSize = true;
            this.lblTotal_parcial.Location = new System.Drawing.Point(456, 276);
            this.lblTotal_parcial.Name = "lblTotal_parcial";
            this.lblTotal_parcial.Size = new System.Drawing.Size(95, 21);
            this.lblTotal_parcial.TabIndex = 5;
            this.lblTotal_parcial.Text = "Total parcial:";
            // 
            // lblHora
            // 
            this.lblHora.AutoSize = true;
            this.lblHora.Location = new System.Drawing.Point(181, 21);
            this.lblHora.Name = "lblHora";
            this.lblHora.Size = new System.Drawing.Size(47, 21);
            this.lblHora.TabIndex = 4;
            this.lblHora.Text = "Hora:";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Location = new System.Drawing.Point(181, 4);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(116, 21);
            this.lblFecha.TabIndex = 3;
            this.lblFecha.Text = "Fecha de venta:";
            // 
            // lblCliente
            // 
            this.lblCliente.AutoSize = true;
            this.lblCliente.Location = new System.Drawing.Point(4, 62);
            this.lblCliente.Name = "lblCliente";
            this.lblCliente.Size = new System.Drawing.Size(61, 21);
            this.lblCliente.TabIndex = 2;
            this.lblCliente.Text = "Cliente:";
            // 
            // lblEmpleado
            // 
            this.lblEmpleado.AutoSize = true;
            this.lblEmpleado.Location = new System.Drawing.Point(4, 45);
            this.lblEmpleado.Name = "lblEmpleado";
            this.lblEmpleado.Size = new System.Drawing.Size(67, 21);
            this.lblEmpleado.TabIndex = 1;
            this.lblEmpleado.Text = "Atendió:";
            // 
            // lblId_venta
            // 
            this.lblId_venta.AutoSize = true;
            this.lblId_venta.Location = new System.Drawing.Point(4, 4);
            this.lblId_venta.Name = "lblId_venta";
            this.lblId_venta.Size = new System.Drawing.Size(111, 21);
            this.lblId_venta.TabIndex = 0;
            this.lblId_venta.Text = "Venta número:";
            // 
            // btnPrint
            // 
            this.btnPrint.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPrint.BackgroundImage")));
            this.btnPrint.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPrint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPrint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnPrint.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnPrint.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPrint.Location = new System.Drawing.Point(598, 452);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(40, 40);
            this.btnPrint.TabIndex = 17;
            this.btnPrint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPrint.UseVisualStyleBackColor = true;
            // 
            // FrmDetalleVenta
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(661, 545);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelToolBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmDetalleVenta";
            this.Text = "Form1";
            this.panelToolBox.ResumeLayout(false);
            this.panelToolBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pxMaximize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxClose)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPagos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetalle_pedido)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelToolBox;
        private System.Windows.Forms.PictureBox pxClose;
        private System.Windows.Forms.PictureBox pxMinimize;
        private System.Windows.Forms.PictureBox pxMaximize;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ToolTip toolTipToolBox;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblId_venta;
        private System.Windows.Forms.Label lblEmpleado;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Label lblHora;
        private System.Windows.Forms.Label lblTotal_parcial;
        private System.Windows.Forms.DataGridView dgvDetalle_pedido;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgvPagos;
        private System.Windows.Forms.Label lblPropina;
        private System.Windows.Forms.Label lblSubTotal;
        private System.Windows.Forms.Label lblDescuento;
        private System.Windows.Forms.Label lblTotalFinal;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.Label lblMesa;
        private System.Windows.Forms.Label lblIdPedido;
        public System.Windows.Forms.Button btnPrint;
    }
}

