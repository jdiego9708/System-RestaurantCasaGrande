namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class ContextMenuDatosPedido
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

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCliente = new System.Windows.Forms.TextBox();
            this.btnSeleccionarCliente = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTotalParcial = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.lblFecha = new System.Windows.Forms.Label();
            this.btnTerminarPedido = new System.Windows.Forms.Button();
            this.btnGuardarPedidosBorradores = new System.Windows.Forms.Button();
            this.chkImprimirPedido = new System.Windows.Forms.CheckBox();
            this.btnNumeroComandas = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Cliente:";
            // 
            // txtCliente
            // 
            this.txtCliente.Location = new System.Drawing.Point(5, 113);
            this.txtCliente.Name = "txtCliente";
            this.txtCliente.ReadOnly = true;
            this.txtCliente.Size = new System.Drawing.Size(345, 25);
            this.txtCliente.TabIndex = 1;
            this.txtCliente.Tag = "0";
            this.txtCliente.Text = "Nombre";
            // 
            // btnSeleccionarCliente
            // 
            this.btnSeleccionarCliente.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccionarCliente.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnSeleccionarCliente.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionarCliente.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnSeleccionarCliente.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionarCliente.Location = new System.Drawing.Point(5, 147);
            this.btnSeleccionarCliente.Name = "btnSeleccionarCliente";
            this.btnSeleccionarCliente.Size = new System.Drawing.Size(345, 34);
            this.btnSeleccionarCliente.TabIndex = 2;
            this.btnSeleccionarCliente.Text = "Seleccionar cliente";
            this.btnSeleccionarCliente.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 184);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Total parcial:";
            // 
            // lblTotalParcial
            // 
            this.lblTotalParcial.AutoSize = true;
            this.lblTotalParcial.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalParcial.Location = new System.Drawing.Point(5, 207);
            this.lblTotalParcial.Name = "lblTotalParcial";
            this.lblTotalParcial.Size = new System.Drawing.Size(45, 21);
            this.lblTotalParcial.TabIndex = 4;
            this.lblTotalParcial.Text = "Total";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panel1.Controls.Add(this.label3);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(361, 24);
            this.panel1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(117, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 19);
            this.label3.TabIndex = 6;
            this.label3.Text = "Datos del pedido";
            // 
            // lblFecha
            // 
            this.lblFecha.AutoSize = true;
            this.lblFecha.Location = new System.Drawing.Point(2, 27);
            this.lblFecha.Name = "lblFecha";
            this.lblFecha.Size = new System.Drawing.Size(109, 17);
            this.lblFecha.TabIndex = 13;
            this.lblFecha.Text = "Fecha del pedido";
            // 
            // btnTerminarPedido
            // 
            this.btnTerminarPedido.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTerminarPedido.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnTerminarPedido.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnTerminarPedido.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnTerminarPedido.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTerminarPedido.Location = new System.Drawing.Point(-2, 281);
            this.btnTerminarPedido.Name = "btnTerminarPedido";
            this.btnTerminarPedido.Size = new System.Drawing.Size(151, 48);
            this.btnTerminarPedido.TabIndex = 14;
            this.btnTerminarPedido.Text = "Terminar pedido";
            this.toolTip1.SetToolTip(this.btnTerminarPedido, "Factura y termina el pedido finalmente");
            this.btnTerminarPedido.UseVisualStyleBackColor = true;
            // 
            // btnGuardarPedidosBorradores
            // 
            this.btnGuardarPedidosBorradores.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardarPedidosBorradores.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnGuardarPedidosBorradores.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnGuardarPedidosBorradores.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnGuardarPedidosBorradores.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardarPedidosBorradores.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardarPedidosBorradores.Location = new System.Drawing.Point(210, 281);
            this.btnGuardarPedidosBorradores.Name = "btnGuardarPedidosBorradores";
            this.btnGuardarPedidosBorradores.Size = new System.Drawing.Size(151, 48);
            this.btnGuardarPedidosBorradores.TabIndex = 15;
            this.btnGuardarPedidosBorradores.Text = "Precuenta";
            this.toolTip1.SetToolTip(this.btnGuardarPedidosBorradores, "Imprimir la precuenta, puede editar después esta factura");
            this.btnGuardarPedidosBorradores.UseVisualStyleBackColor = true;
            this.btnGuardarPedidosBorradores.Visible = false;
            // 
            // chkImprimirPedido
            // 
            this.chkImprimirPedido.AutoSize = true;
            this.chkImprimirPedido.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkImprimirPedido.Location = new System.Drawing.Point(5, 58);
            this.chkImprimirPedido.Name = "chkImprimirPedido";
            this.chkImprimirPedido.Size = new System.Drawing.Size(140, 21);
            this.chkImprimirPedido.TabIndex = 17;
            this.chkImprimirPedido.Text = "Imprimir comandas";
            this.chkImprimirPedido.UseVisualStyleBackColor = true;
            // 
            // btnNumeroComandas
            // 
            this.btnNumeroComandas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNumeroComandas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnNumeroComandas.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnNumeroComandas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnNumeroComandas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNumeroComandas.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNumeroComandas.Location = new System.Drawing.Point(183, 58);
            this.btnNumeroComandas.Name = "btnNumeroComandas";
            this.btnNumeroComandas.Size = new System.Drawing.Size(159, 27);
            this.btnNumeroComandas.TabIndex = 18;
            this.btnNumeroComandas.Text = "Número de comandas";
            this.btnNumeroComandas.UseVisualStyleBackColor = true;
            this.btnNumeroComandas.Visible = false;
            // 
            // ContextMenuDatosPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.btnNumeroComandas);
            this.Controls.Add(this.chkImprimirPedido);
            this.Controls.Add(this.btnGuardarPedidosBorradores);
            this.Controls.Add(this.btnTerminarPedido);
            this.Controls.Add(this.lblFecha);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblTotalParcial);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSeleccionarCliente);
            this.Controls.Add(this.txtCliente);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ContextMenuDatosPedido";
            this.Size = new System.Drawing.Size(359, 327);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSeleccionarCliente;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTotalParcial;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblFecha;
        private System.Windows.Forms.Button btnNumeroComandas;
        public System.Windows.Forms.Button btnTerminarPedido;
        public System.Windows.Forms.Button btnGuardarPedidosBorradores;
        public System.Windows.Forms.TextBox txtCliente;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.CheckBox chkImprimirPedido;
    }
}
