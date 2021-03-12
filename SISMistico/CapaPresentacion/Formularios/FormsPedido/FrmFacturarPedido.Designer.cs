namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmFacturarPedido
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFacturarPedido));
            this.label1 = new System.Windows.Forms.Label();
            this.lblIdPedido = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.dgvPedido = new System.Windows.Forms.DataGridView();
            this.label6 = new System.Windows.Forms.Label();
            this.lblCliente = new System.Windows.Forms.Label();
            this.lblMesero = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnTerminar = new System.Windows.Forms.Button();
            this.btnDescuentos = new System.Windows.Forms.Button();
            this.panelTipoPedido = new System.Windows.Forms.Panel();
            this.rdNinguna = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.metroTile1 = new MetroFramework.Controls.MetroTile();
            this.chkRecordarOpcion = new System.Windows.Forms.CheckBox();
            this.rdAmbas = new System.Windows.Forms.RadioButton();
            this.rdCorreo = new System.Windows.Forms.RadioButton();
            this.rdImprimir = new System.Windows.Forms.RadioButton();
            this.lblTotal = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.lblTotalParcial = new System.Windows.Forms.Label();
            this.lblMesa = new System.Windows.Forms.Label();
            this.lblSubTotal = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panelSubTotal = new System.Windows.Forms.Panel();
            this.txtCantidadPedidosCliente = new System.Windows.Forms.TextBox();
            this.panelDescuentos = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedido)).BeginInit();
            this.panelTipoPedido.SuspendLayout();
            this.panelSubTotal.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pedido número:";
            // 
            // lblIdPedido
            // 
            this.lblIdPedido.AutoSize = true;
            this.lblIdPedido.Location = new System.Drawing.Point(102, 2);
            this.lblIdPedido.Name = "lblIdPedido";
            this.lblIdPedido.Size = new System.Drawing.Size(66, 17);
            this.lblIdPedido.TabIndex = 1;
            this.lblIdPedido.Text = "Id_pedido";
            this.lblIdPedido.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(166, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Fecha:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 17);
            this.label4.TabIndex = 2;
            this.label4.Text = "Detalle del pedido:";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Location = new System.Drawing.Point(208, 2);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(44, 17);
            this.lblFecha.TabIndex = 4;
            this.lblFecha.Text = "Fecha:";
            this.lblFecha.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvPedido
            // 
            this.dgvPedido.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPedido.Location = new System.Drawing.Point(6, 83);
            this.dgvPedido.Name = "dgvPedido";
            this.dgvPedido.Size = new System.Drawing.Size(553, 141);
            this.dgvPedido.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 17);
            this.label6.TabIndex = 6;
            this.label6.Text = "Cliente:";
            // 
            // lblCliente
            // 
            this.lblCliente.AutoSize = true;
            this.lblCliente.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblCliente.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCliente.Location = new System.Drawing.Point(52, 22);
            this.lblCliente.Name = "lblCliente";
            this.lblCliente.Size = new System.Drawing.Size(50, 17);
            this.lblCliente.TabIndex = 7;
            this.lblCliente.Text = "Cliente:";
            this.toolTip1.SetToolTip(this.lblCliente, "Ver datos del cliente");
            // 
            // lblMesero
            // 
            this.lblMesero.AutoSize = true;
            this.lblMesero.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblMesero.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMesero.Location = new System.Drawing.Point(59, 43);
            this.lblMesero.Name = "lblMesero";
            this.lblMesero.Size = new System.Drawing.Size(56, 17);
            this.lblMesero.TabIndex = 9;
            this.lblMesero.Text = "Mesero:";
            this.toolTip1.SetToolTip(this.lblMesero, "Seleccionar otro mesero");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 43);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 17);
            this.label9.TabIndex = 8;
            this.label9.Text = "Mesero:";
            // 
            // btnTerminar
            // 
            this.btnTerminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTerminar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnTerminar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnTerminar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnTerminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTerminar.Location = new System.Drawing.Point(7, 392);
            this.btnTerminar.Name = "btnTerminar";
            this.btnTerminar.Size = new System.Drawing.Size(114, 49);
            this.btnTerminar.TabIndex = 11;
            this.btnTerminar.Text = "Terminar";
            this.toolTip1.SetToolTip(this.btnTerminar, "Terminar");
            this.btnTerminar.UseVisualStyleBackColor = true;
            // 
            // btnDescuentos
            // 
            this.btnDescuentos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDescuentos.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDescuentos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnDescuentos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnDescuentos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDescuentos.Location = new System.Drawing.Point(408, 229);
            this.btnDescuentos.Name = "btnDescuentos";
            this.btnDescuentos.Size = new System.Drawing.Size(151, 49);
            this.btnDescuentos.TabIndex = 18;
            this.btnDescuentos.Text = "Descuentos y opciones";
            this.toolTip1.SetToolTip(this.btnDescuentos, "Terminar");
            this.btnDescuentos.UseVisualStyleBackColor = true;
            this.btnDescuentos.Visible = false;
            // 
            // panelTipoPedido
            // 
            this.panelTipoPedido.Controls.Add(this.rdNinguna);
            this.panelTipoPedido.Controls.Add(this.label2);
            this.panelTipoPedido.Controls.Add(this.metroTile1);
            this.panelTipoPedido.Controls.Add(this.chkRecordarOpcion);
            this.panelTipoPedido.Controls.Add(this.rdAmbas);
            this.panelTipoPedido.Controls.Add(this.rdCorreo);
            this.panelTipoPedido.Controls.Add(this.rdImprimir);
            this.panelTipoPedido.Location = new System.Drawing.Point(7, 332);
            this.panelTipoPedido.Name = "panelTipoPedido";
            this.panelTipoPedido.Size = new System.Drawing.Size(544, 54);
            this.panelTipoPedido.TabIndex = 13;
            // 
            // rdNinguna
            // 
            this.rdNinguna.AutoSize = true;
            this.rdNinguna.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdNinguna.Location = new System.Drawing.Point(323, 28);
            this.rdNinguna.Name = "rdNinguna";
            this.rdNinguna.Size = new System.Drawing.Size(75, 21);
            this.rdNinguna.TabIndex = 25;
            this.rdNinguna.TabStop = true;
            this.rdNinguna.Tag = "Ninguna";
            this.rdNinguna.Text = "Ninguna";
            this.rdNinguna.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(126, 17);
            this.label2.TabIndex = 24;
            this.label2.Text = "Opciones de factura";
            // 
            // metroTile1
            // 
            this.metroTile1.ActiveControl = null;
            this.metroTile1.BackColor = System.Drawing.Color.Silver;
            this.metroTile1.Location = new System.Drawing.Point(402, 28);
            this.metroTile1.Name = "metroTile1";
            this.metroTile1.Size = new System.Drawing.Size(10, 23);
            this.metroTile1.TabIndex = 4;
            this.metroTile1.Text = "metroTile1";
            this.metroTile1.UseCustomBackColor = true;
            this.metroTile1.UseSelectable = true;
            // 
            // chkRecordarOpcion
            // 
            this.chkRecordarOpcion.AutoSize = true;
            this.chkRecordarOpcion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRecordarOpcion.Location = new System.Drawing.Point(416, 29);
            this.chkRecordarOpcion.Name = "chkRecordarOpcion";
            this.chkRecordarOpcion.Size = new System.Drawing.Size(125, 21);
            this.chkRecordarOpcion.TabIndex = 3;
            this.chkRecordarOpcion.Text = "Recordar opción";
            this.chkRecordarOpcion.UseVisualStyleBackColor = true;
            this.chkRecordarOpcion.Visible = false;
            // 
            // rdAmbas
            // 
            this.rdAmbas.AutoSize = true;
            this.rdAmbas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdAmbas.Location = new System.Drawing.Point(253, 28);
            this.rdAmbas.Name = "rdAmbas";
            this.rdAmbas.Size = new System.Drawing.Size(66, 21);
            this.rdAmbas.TabIndex = 2;
            this.rdAmbas.TabStop = true;
            this.rdAmbas.Tag = "Ambas";
            this.rdAmbas.Text = "Ambas";
            this.rdAmbas.UseVisualStyleBackColor = true;
            // 
            // rdCorreo
            // 
            this.rdCorreo.AutoSize = true;
            this.rdCorreo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdCorreo.Location = new System.Drawing.Point(82, 28);
            this.rdCorreo.Name = "rdCorreo";
            this.rdCorreo.Size = new System.Drawing.Size(172, 21);
            this.rdCorreo.TabIndex = 1;
            this.rdCorreo.TabStop = true;
            this.rdCorreo.Tag = "Correo";
            this.rdCorreo.Text = "Enviar correo electrónico";
            this.rdCorreo.UseVisualStyleBackColor = true;
            // 
            // rdImprimir
            // 
            this.rdImprimir.AutoSize = true;
            this.rdImprimir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdImprimir.Location = new System.Drawing.Point(4, 28);
            this.rdImprimir.Name = "rdImprimir";
            this.rdImprimir.Size = new System.Drawing.Size(75, 21);
            this.rdImprimir.TabIndex = 0;
            this.rdImprimir.TabStop = true;
            this.rdImprimir.Tag = "Imprimir";
            this.rdImprimir.Text = "Imprimir";
            this.rdImprimir.UseVisualStyleBackColor = true;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(57, 312);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(47, 17);
            this.lblTotal.TabIndex = 15;
            this.lblTotal.Text = "TOTAL:";
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(9, 312);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(47, 17);
            this.label11.TabIndex = 14;
            this.label11.Text = "TOTAL:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 229);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(82, 17);
            this.label12.TabIndex = 16;
            this.label12.Text = "Total parcial:";
            // 
            // lblTotalParcial
            // 
            this.lblTotalParcial.AutoSize = true;
            this.lblTotalParcial.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalParcial.Location = new System.Drawing.Point(83, 229);
            this.lblTotalParcial.Name = "lblTotalParcial";
            this.lblTotalParcial.Size = new System.Drawing.Size(83, 17);
            this.lblTotalParcial.TabIndex = 17;
            this.lblTotalParcial.Text = "Total parcial:";
            // 
            // lblMesa
            // 
            this.lblMesa.AutoSize = true;
            this.lblMesa.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMesa.Location = new System.Drawing.Point(457, 63);
            this.lblMesa.Name = "lblMesa";
            this.lblMesa.Size = new System.Drawing.Size(40, 17);
            this.lblMesa.TabIndex = 20;
            this.lblMesa.Text = "mesa";
            this.lblMesa.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSubTotal
            // 
            this.lblSubTotal.AutoSize = true;
            this.lblSubTotal.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubTotal.Location = new System.Drawing.Point(63, 10);
            this.lblSubTotal.Name = "lblSubTotal";
            this.lblSubTotal.Size = new System.Drawing.Size(47, 17);
            this.lblSubTotal.TabIndex = 22;
            this.lblSubTotal.Text = "TOTAL:";
            this.lblSubTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 17);
            this.label5.TabIndex = 21;
            this.label5.Text = "Subtotal:";
            // 
            // panelSubTotal
            // 
            this.panelSubTotal.Controls.Add(this.label5);
            this.panelSubTotal.Controls.Add(this.lblSubTotal);
            this.panelSubTotal.Location = new System.Drawing.Point(6, 260);
            this.panelSubTotal.Name = "panelSubTotal";
            this.panelSubTotal.Size = new System.Drawing.Size(138, 35);
            this.panelSubTotal.TabIndex = 23;
            // 
            // txtCantidadPedidosCliente
            // 
            this.txtCantidadPedidosCliente.BackColor = System.Drawing.Color.White;
            this.txtCantidadPedidosCliente.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCantidadPedidosCliente.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCantidadPedidosCliente.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtCantidadPedidosCliente.Location = new System.Drawing.Point(127, 392);
            this.txtCantidadPedidosCliente.Multiline = true;
            this.txtCantidadPedidosCliente.Name = "txtCantidadPedidosCliente";
            this.txtCantidadPedidosCliente.Size = new System.Drawing.Size(358, 49);
            this.txtCantidadPedidosCliente.TabIndex = 24;
            // 
            // panelDescuentos
            // 
            this.panelDescuentos.Location = new System.Drawing.Point(565, 12);
            this.panelDescuentos.Name = "panelDescuentos";
            this.panelDescuentos.Size = new System.Drawing.Size(428, 517);
            this.panelDescuentos.TabIndex = 25;
            // 
            // FrmFacturarPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(995, 533);
            this.Controls.Add(this.panelDescuentos);
            this.Controls.Add(this.txtCantidadPedidosCliente);
            this.Controls.Add(this.panelSubTotal);
            this.Controls.Add(this.lblMesa);
            this.Controls.Add(this.btnDescuentos);
            this.Controls.Add(this.lblTotalParcial);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.panelTipoPedido);
            this.Controls.Add(this.btnTerminar);
            this.Controls.Add(this.lblMesero);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lblCliente);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.dgvPedido);
            this.Controls.Add(this.lblFecha);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblIdPedido);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FrmFacturarPedido";
            this.Text = "Facturar pedido";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedido)).EndInit();
            this.panelTipoPedido.ResumeLayout(false);
            this.panelTipoPedido.PerformLayout();
            this.panelSubTotal.ResumeLayout(false);
            this.panelSubTotal.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblIdPedido;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.DataGridView dgvPedido;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.Label lblMesero;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnTerminar;
        private System.Windows.Forms.Panel panelTipoPedido;
        private System.Windows.Forms.RadioButton rdCorreo;
        private System.Windows.Forms.RadioButton rdImprimir;
        private System.Windows.Forms.RadioButton rdAmbas;
        private System.Windows.Forms.CheckBox chkRecordarOpcion;
        private MetroFramework.Controls.MetroTile metroTile1;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblTotalParcial;
        private System.Windows.Forms.Button btnDescuentos;
        private System.Windows.Forms.Label lblMesa;
        private System.Windows.Forms.Label lblSubTotal;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelSubTotal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCantidadPedidosCliente;
        private System.Windows.Forms.RadioButton rdNinguna;
        private System.Windows.Forms.Panel panelDescuentos;
    }
}