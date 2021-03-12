namespace CapaPresentacion.Formularios.FormsBebidas
{
    partial class FrmAgregarBebidas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAgregarBebidas));
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPrecio = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPrecioTrago = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPrecioTragoDoble = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPrecioProveedor = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtProveedor = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.adjuntarImagen = new CapaPresentacion.Formularios.Controles.AdjuntarImagen();
            this.label7 = new System.Windows.Forms.Label();
            this.ListaTipoBebidas = new System.Windows.Forms.ComboBox();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.txtCantidadxUnidad = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCantidadUnidad = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.lblCantidadTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtNombre
            // 
            this.txtNombre.BackColor = System.Drawing.Color.White;
            this.txtNombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtNombre.Location = new System.Drawing.Point(12, 25);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(243, 25);
            this.txtNombre.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Nombre";
            // 
            // txtPrecio
            // 
            this.txtPrecio.BackColor = System.Drawing.Color.White;
            this.txtPrecio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPrecio.Location = new System.Drawing.Point(12, 72);
            this.txtPrecio.Name = "txtPrecio";
            this.txtPrecio.Size = new System.Drawing.Size(67, 25);
            this.txtPrecio.TabIndex = 5;
            this.txtPrecio.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Precio";
            // 
            // txtPrecioTrago
            // 
            this.txtPrecioTrago.BackColor = System.Drawing.Color.White;
            this.txtPrecioTrago.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPrecioTrago.Location = new System.Drawing.Point(85, 72);
            this.txtPrecioTrago.Name = "txtPrecioTrago";
            this.txtPrecioTrago.Size = new System.Drawing.Size(71, 25);
            this.txtPrecioTrago.TabIndex = 7;
            this.txtPrecioTrago.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(82, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Precio trago";
            // 
            // txtPrecioTragoDoble
            // 
            this.txtPrecioTragoDoble.BackColor = System.Drawing.Color.White;
            this.txtPrecioTragoDoble.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPrecioTragoDoble.Location = new System.Drawing.Point(12, 125);
            this.txtPrecioTragoDoble.Name = "txtPrecioTragoDoble";
            this.txtPrecioTragoDoble.Size = new System.Drawing.Size(93, 25);
            this.txtPrecioTragoDoble.TabIndex = 9;
            this.txtPrecioTragoDoble.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 17);
            this.label4.TabIndex = 8;
            this.label4.Text = "Precio trago doble";
            // 
            // txtPrecioProveedor
            // 
            this.txtPrecioProveedor.BackColor = System.Drawing.Color.White;
            this.txtPrecioProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPrecioProveedor.Location = new System.Drawing.Point(133, 125);
            this.txtPrecioProveedor.Name = "txtPrecioProveedor";
            this.txtPrecioProveedor.Size = new System.Drawing.Size(93, 25);
            this.txtPrecioProveedor.TabIndex = 11;
            this.txtPrecioProveedor.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(130, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 17);
            this.label5.TabIndex = 10;
            this.label5.Text = "Precio proveedor";
            // 
            // txtProveedor
            // 
            this.txtProveedor.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProveedor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtProveedor.Location = new System.Drawing.Point(12, 230);
            this.txtProveedor.Name = "txtProveedor";
            this.txtProveedor.ReadOnly = true;
            this.txtProveedor.Size = new System.Drawing.Size(243, 25);
            this.txtProveedor.TabIndex = 13;
            this.txtProveedor.Tag = "0";
            this.txtProveedor.Text = "Seleccione proveedor (INHABILITADA)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 210);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 17);
            this.label6.TabIndex = 12;
            this.label6.Text = "Proveedor";
            // 
            // adjuntarImagen
            // 
            this.adjuntarImagen.BackColor = System.Drawing.Color.White;
            this.adjuntarImagen.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.adjuntarImagen.Location = new System.Drawing.Point(261, 3);
            this.adjuntarImagen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.adjuntarImagen.Name = "adjuntarImagen";
            this.adjuntarImagen.Nombre_imagen = "SIN IMAGEN";
            this.adjuntarImagen.RutaOrigen = "";
            this.adjuntarImagen.Size = new System.Drawing.Size(232, 304);
            this.adjuntarImagen.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 262);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(98, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "Tipo de bebida";
            // 
            // ListaTipoBebidas
            // 
            this.ListaTipoBebidas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ListaTipoBebidas.FormattingEnabled = true;
            this.ListaTipoBebidas.Location = new System.Drawing.Point(12, 282);
            this.ListaTipoBebidas.Name = "ListaTipoBebidas";
            this.ListaTipoBebidas.Size = new System.Drawing.Size(167, 25);
            this.ListaTipoBebidas.TabIndex = 16;
            // 
            // btnGuardar
            // 
            this.btnGuardar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnGuardar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnGuardar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.ForeColor = System.Drawing.Color.Gray;
            this.btnGuardar.Image = ((System.Drawing.Image)(resources.GetObject("btnGuardar.Image")));
            this.btnGuardar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnGuardar.Location = new System.Drawing.Point(12, 313);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(134, 44);
            this.btnGuardar.TabIndex = 25;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGuardar.UseVisualStyleBackColor = true;
            // 
            // btnCancelar
            // 
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnCancelar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.Gray;
            this.btnCancelar.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelar.Image")));
            this.btnCancelar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancelar.Location = new System.Drawing.Point(362, 314);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(119, 44);
            this.btnCancelar.TabIndex = 26;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancelar.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.errorProvider1.ContainerControl = this;
            this.errorProvider1.Icon = ((System.Drawing.Icon)(resources.GetObject("errorProvider1.Icon")));
            // 
            // txtCantidadxUnidad
            // 
            this.txtCantidadxUnidad.BackColor = System.Drawing.Color.White;
            this.txtCantidadxUnidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtCantidadxUnidad.Location = new System.Drawing.Point(133, 177);
            this.txtCantidadxUnidad.Name = "txtCantidadxUnidad";
            this.txtCantidadxUnidad.Size = new System.Drawing.Size(93, 25);
            this.txtCantidadxUnidad.TabIndex = 30;
            this.txtCantidadxUnidad.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(130, 157);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(129, 17);
            this.label8.TabIndex = 29;
            this.label8.Text = "Cantidad por unidad";
            // 
            // txtCantidadUnidad
            // 
            this.txtCantidadUnidad.BackColor = System.Drawing.Color.White;
            this.txtCantidadUnidad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtCantidadUnidad.Location = new System.Drawing.Point(12, 177);
            this.txtCantidadUnidad.Name = "txtCantidadUnidad";
            this.txtCantidadUnidad.Size = new System.Drawing.Size(93, 25);
            this.txtCantidadUnidad.TabIndex = 28;
            this.txtCantidadUnidad.Text = "0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 157);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 17);
            this.label9.TabIndex = 27;
            this.label9.Text = "Cantidad unidades";
            // 
            // lblCantidadTotal
            // 
            this.lblCantidadTotal.AutoSize = true;
            this.lblCantidadTotal.Location = new System.Drawing.Point(130, 205);
            this.lblCantidadTotal.Name = "lblCantidadTotal";
            this.lblCantidadTotal.Size = new System.Drawing.Size(90, 17);
            this.lblCantidadTotal.TabIndex = 31;
            this.lblCantidadTotal.Text = "Cantidad total";
            // 
            // FrmAgregarBebidas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(493, 364);
            this.Controls.Add(this.lblCantidadTotal);
            this.Controls.Add(this.txtCantidadxUnidad);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtCantidadUnidad);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.ListaTipoBebidas);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.adjuntarImagen);
            this.Controls.Add(this.txtProveedor);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtPrecioProveedor);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtPrecioTragoDoble);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPrecioTrago);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtPrecio);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmAgregarBebidas";
            this.Text = "Agregar una bebida";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPrecio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPrecioTrago;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPrecioTragoDoble;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPrecioProveedor;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtProveedor;
        private System.Windows.Forms.Label label6;
        private Controles.AdjuntarImagen adjuntarImagen;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox ListaTipoBebidas;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.TextBox txtCantidadxUnidad;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCantidadUnidad;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCantidadTotal;
    }
}