namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class DescuentosOpcionesPedido
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
            this.ListaDescuentos = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPropina = new System.Windows.Forms.TextBox();
            this.gbCupon = new System.Windows.Forms.GroupBox();
            this.txtCupon = new System.Windows.Forms.TextBox();
            this.lblTotalParcial = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSubtotal = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblPropinaSugerida = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnDefault = new System.Windows.Forms.Button();
            this.txtDebito = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtEfectivo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkEfectivo = new System.Windows.Forms.CheckBox();
            this.rdCredito = new System.Windows.Forms.RadioButton();
            this.rdDebito = new System.Windows.Forms.RadioButton();
            this.chkDesechables = new System.Windows.Forms.CheckBox();
            this.txtPrecioDesechables = new System.Windows.Forms.TextBox();
            this.txtDomicilio = new System.Windows.Forms.TextBox();
            this.chkDomicilio = new System.Windows.Forms.CheckBox();
            this.gbCupon.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 137);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Descuentos disponibles:";
            // 
            // ListaDescuentos
            // 
            this.ListaDescuentos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ListaDescuentos.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ListaDescuentos.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ListaDescuentos.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListaDescuentos.FormattingEnabled = true;
            this.ListaDescuentos.Location = new System.Drawing.Point(5, 162);
            this.ListaDescuentos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ListaDescuentos.Name = "ListaDescuentos";
            this.ListaDescuentos.Size = new System.Drawing.Size(128, 29);
            this.ListaDescuentos.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(218, 24);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Propina mesero:";
            // 
            // txtPropina
            // 
            this.txtPropina.Location = new System.Drawing.Point(223, 50);
            this.txtPropina.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtPropina.Name = "txtPropina";
            this.txtPropina.Size = new System.Drawing.Size(125, 29);
            this.txtPropina.TabIndex = 3;
            this.txtPropina.Tag = "PROPINA";
            // 
            // gbCupon
            // 
            this.gbCupon.Controls.Add(this.txtCupon);
            this.gbCupon.Location = new System.Drawing.Point(204, 129);
            this.gbCupon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCupon.Name = "gbCupon";
            this.gbCupon.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.gbCupon.Size = new System.Drawing.Size(196, 71);
            this.gbCupon.TabIndex = 4;
            this.gbCupon.TabStop = false;
            this.gbCupon.Text = "Cupón o bono";
            // 
            // txtCupon
            // 
            this.txtCupon.Location = new System.Drawing.Point(7, 29);
            this.txtCupon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtCupon.Name = "txtCupon";
            this.txtCupon.Size = new System.Drawing.Size(180, 29);
            this.txtCupon.TabIndex = 5;
            // 
            // lblTotalParcial
            // 
            this.lblTotalParcial.AutoSize = true;
            this.lblTotalParcial.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalParcial.Location = new System.Drawing.Point(98, 5);
            this.lblTotalParcial.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTotalParcial.Name = "lblTotalParcial";
            this.lblTotalParcial.Size = new System.Drawing.Size(96, 20);
            this.lblTotalParcial.TabIndex = 21;
            this.lblTotalParcial.Text = "Total parcial:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(2, 3);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(95, 21);
            this.label12.TabIndex = 20;
            this.label12.Text = "Total parcial:";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Location = new System.Drawing.Point(2, 454);
            this.txtObservaciones.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.Size = new System.Drawing.Size(420, 52);
            this.txtObservaciones.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 430);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 21);
            this.label3.TabIndex = 24;
            this.label3.Text = "Observaciones:";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(88, 202);
            this.lblTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(53, 25);
            this.lblTotal.TabIndex = 27;
            this.lblTotal.Text = "total";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(-2, 202);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(73, 25);
            this.label5.TabIndex = 26;
            this.label5.Text = "TOTAL:";
            // 
            // lblSubtotal
            // 
            this.lblSubtotal.AutoSize = true;
            this.lblSubtotal.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtotal.Location = new System.Drawing.Point(291, 95);
            this.lblSubtotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSubtotal.Name = "lblSubtotal";
            this.lblSubtotal.Size = new System.Drawing.Size(83, 17);
            this.lblSubtotal.TabIndex = 29;
            this.lblSubtotal.Text = "Total parcial:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(219, 92);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 21);
            this.label6.TabIndex = 28;
            this.label6.Text = "Subtotal:";
            // 
            // lblPropinaSugerida
            // 
            this.lblPropinaSugerida.AutoSize = true;
            this.lblPropinaSugerida.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPropinaSugerida.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPropinaSugerida.Location = new System.Drawing.Point(358, 3);
            this.lblPropinaSugerida.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPropinaSugerida.Name = "lblPropinaSugerida";
            this.lblPropinaSugerida.Size = new System.Drawing.Size(53, 17);
            this.lblPropinaSugerida.TabIndex = 30;
            this.lblPropinaSugerida.Text = "Propina";
            this.toolTip1.SetToolTip(this.lblPropinaSugerida, "Oprima para editar");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(218, 3);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 21);
            this.label4.TabIndex = 31;
            this.label4.Text = "Propina sugerida:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(0, 236);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(423, 190);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Método de pago";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnDefault);
            this.panel1.Controls.Add(this.txtDebito);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.txtEfectivo);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.chkEfectivo);
            this.panel1.Controls.Add(this.rdCredito);
            this.panel1.Controls.Add(this.rdDebito);
            this.panel1.Location = new System.Drawing.Point(0, 29);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(416, 155);
            this.panel1.TabIndex = 0;
            // 
            // btnDefault
            // 
            this.btnDefault.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDefault.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefault.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnDefault.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDefault.Location = new System.Drawing.Point(92, 113);
            this.btnDefault.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDefault.Name = "btnDefault";
            this.btnDefault.Size = new System.Drawing.Size(182, 34);
            this.btnDefault.TabIndex = 15;
            this.btnDefault.Text = "Valores por defecto";
            this.btnDefault.UseVisualStyleBackColor = true;
            // 
            // txtDebito
            // 
            this.txtDebito.Location = new System.Drawing.Point(232, 76);
            this.txtDebito.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtDebito.Name = "txtDebito";
            this.txtDebito.Size = new System.Drawing.Size(128, 29);
            this.txtDebito.TabIndex = 7;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(230, 50);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(115, 21);
            this.label8.TabIndex = 6;
            this.label8.Text = "Valor de debito";
            // 
            // txtEfectivo
            // 
            this.txtEfectivo.Location = new System.Drawing.Point(11, 76);
            this.txtEfectivo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtEfectivo.Name = "txtEfectivo";
            this.txtEfectivo.Size = new System.Drawing.Size(128, 29);
            this.txtEfectivo.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 50);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 21);
            this.label7.TabIndex = 4;
            this.label7.Text = "Valor de efectivo";
            // 
            // chkEfectivo
            // 
            this.chkEfectivo.AutoSize = true;
            this.chkEfectivo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkEfectivo.Location = new System.Drawing.Point(7, 10);
            this.chkEfectivo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkEfectivo.Name = "chkEfectivo";
            this.chkEfectivo.Size = new System.Drawing.Size(83, 25);
            this.chkEfectivo.TabIndex = 3;
            this.chkEfectivo.Text = "Efectivo";
            this.chkEfectivo.UseVisualStyleBackColor = true;
            // 
            // rdCredito
            // 
            this.rdCredito.AutoSize = true;
            this.rdCredito.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdCredito.Location = new System.Drawing.Point(245, 10);
            this.rdCredito.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rdCredito.Name = "rdCredito";
            this.rdCredito.Size = new System.Drawing.Size(146, 25);
            this.rdCredito.TabIndex = 2;
            this.rdCredito.TabStop = true;
            this.rdCredito.Tag = "CREDITO";
            this.rdCredito.Text = "Tarjeta de crédito";
            this.rdCredito.UseVisualStyleBackColor = true;
            // 
            // rdDebito
            // 
            this.rdDebito.AutoSize = true;
            this.rdDebito.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdDebito.Location = new System.Drawing.Point(108, 10);
            this.rdDebito.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rdDebito.Name = "rdDebito";
            this.rdDebito.Size = new System.Drawing.Size(121, 25);
            this.rdDebito.TabIndex = 1;
            this.rdDebito.TabStop = true;
            this.rdDebito.Tag = "DEBITO";
            this.rdDebito.Text = "Tarjeta débito";
            this.rdDebito.UseVisualStyleBackColor = true;
            // 
            // chkDesechables
            // 
            this.chkDesechables.AutoSize = true;
            this.chkDesechables.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDesechables.Location = new System.Drawing.Point(7, 27);
            this.chkDesechables.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkDesechables.Name = "chkDesechables";
            this.chkDesechables.Size = new System.Drawing.Size(165, 25);
            this.chkDesechables.TabIndex = 34;
            this.chkDesechables.Text = "Cobrar desechables";
            this.chkDesechables.UseVisualStyleBackColor = true;
            // 
            // txtPrecioDesechables
            // 
            this.txtPrecioDesechables.Location = new System.Drawing.Point(7, 50);
            this.txtPrecioDesechables.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtPrecioDesechables.Name = "txtPrecioDesechables";
            this.txtPrecioDesechables.Size = new System.Drawing.Size(125, 29);
            this.txtPrecioDesechables.TabIndex = 35;
            this.txtPrecioDesechables.Tag = "0";
            this.txtPrecioDesechables.Visible = false;
            // 
            // txtDomicilio
            // 
            this.txtDomicilio.Location = new System.Drawing.Point(7, 104);
            this.txtDomicilio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtDomicilio.Name = "txtDomicilio";
            this.txtDomicilio.Size = new System.Drawing.Size(125, 29);
            this.txtDomicilio.TabIndex = 37;
            this.txtDomicilio.Tag = "0";
            this.txtDomicilio.Visible = false;
            // 
            // chkDomicilio
            // 
            this.chkDomicilio.AutoSize = true;
            this.chkDomicilio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDomicilio.Location = new System.Drawing.Point(7, 81);
            this.chkDomicilio.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkDomicilio.Name = "chkDomicilio";
            this.chkDomicilio.Size = new System.Drawing.Size(145, 25);
            this.chkDomicilio.TabIndex = 36;
            this.chkDomicilio.Text = "Cobrar domicilio";
            this.chkDomicilio.UseVisualStyleBackColor = true;
            // 
            // DescuentosOpcionesPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.txtDomicilio);
            this.Controls.Add(this.chkDomicilio);
            this.Controls.Add(this.txtPrecioDesechables);
            this.Controls.Add(this.chkDesechables);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblPropinaSugerida);
            this.Controls.Add(this.lblSubtotal);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtObservaciones);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTotalParcial);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.gbCupon);
            this.Controls.Add(this.txtPropina);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ListaDescuentos);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DescuentosOpcionesPedido";
            this.Size = new System.Drawing.Size(428, 517);
            this.gbCupon.ResumeLayout(false);
            this.gbCupon.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gbCupon;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton rdDebito;
        private System.Windows.Forms.RadioButton rdCredito;
        private System.Windows.Forms.CheckBox chkEfectivo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtEfectivo;
        private System.Windows.Forms.TextBox txtDebito;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.ComboBox ListaDescuentos;
        public System.Windows.Forms.TextBox txtPropina;
        public System.Windows.Forms.TextBox txtCupon;
        public System.Windows.Forms.Label lblTotalParcial;
        public System.Windows.Forms.Label lblSubtotal;
        public System.Windows.Forms.Label lblPropinaSugerida;
        public System.Windows.Forms.TextBox txtObservaciones;
        public System.Windows.Forms.Button btnDefault;
        private System.Windows.Forms.CheckBox chkDesechables;
        public System.Windows.Forms.TextBox txtPrecioDesechables;
        public System.Windows.Forms.TextBox txtDomicilio;
        private System.Windows.Forms.CheckBox chkDomicilio;
    }
}
