namespace CapaPresentacion.Formularios.FormsReservas
{
    partial class ReservaMesaSmall
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReservaMesaSmall));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtMesa = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtInformacion = new System.Windows.Forms.TextBox();
            this.btnEliminar = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnEnviarConfirmacion = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtMesa);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(118, 97);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mesa reservada";
            // 
            // txtMesa
            // 
            this.txtMesa.BackColor = System.Drawing.Color.Teal;
            this.txtMesa.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMesa.ForeColor = System.Drawing.Color.White;
            this.txtMesa.Location = new System.Drawing.Point(7, 20);
            this.txtMesa.Name = "txtMesa";
            this.txtMesa.ReadOnly = true;
            this.txtMesa.Size = new System.Drawing.Size(100, 71);
            this.txtMesa.TabIndex = 1;
            this.txtMesa.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtInformacion);
            this.groupBox2.Location = new System.Drawing.Point(127, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(217, 97);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Información";
            // 
            // txtInformacion
            // 
            this.txtInformacion.BackColor = System.Drawing.Color.White;
            this.txtInformacion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInformacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtInformacion.Location = new System.Drawing.Point(6, 20);
            this.txtInformacion.Multiline = true;
            this.txtInformacion.Name = "txtInformacion";
            this.txtInformacion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInformacion.Size = new System.Drawing.Size(205, 71);
            this.txtInformacion.TabIndex = 0;
            // 
            // btnEliminar
            // 
            this.btnEliminar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEliminar.BackgroundImage")));
            this.btnEliminar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEliminar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminar.FlatAppearance.BorderSize = 0;
            this.btnEliminar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnEliminar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminar.Location = new System.Drawing.Point(350, 42);
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(29, 26);
            this.btnEliminar.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnEliminar, "Cancelar esta reserva");
            this.btnEliminar.UseVisualStyleBackColor = true;
            // 
            // btnEditar
            // 
            this.btnEditar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEditar.BackgroundImage")));
            this.btnEditar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEditar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditar.FlatAppearance.BorderSize = 0;
            this.btnEditar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.Location = new System.Drawing.Point(350, 13);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(29, 26);
            this.btnEditar.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnEditar, "Editar los datos de la reserva");
            this.btnEditar.UseVisualStyleBackColor = true;
            // 
            // btnEnviarConfirmacion
            // 
            this.btnEnviarConfirmacion.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEnviarConfirmacion.BackgroundImage")));
            this.btnEnviarConfirmacion.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEnviarConfirmacion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnviarConfirmacion.FlatAppearance.BorderSize = 0;
            this.btnEnviarConfirmacion.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnEnviarConfirmacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnviarConfirmacion.Location = new System.Drawing.Point(350, 74);
            this.btnEnviarConfirmacion.Name = "btnEnviarConfirmacion";
            this.btnEnviarConfirmacion.Size = new System.Drawing.Size(29, 26);
            this.btnEnviarConfirmacion.TabIndex = 4;
            this.toolTip1.SetToolTip(this.btnEnviarConfirmacion, "Enviar recordatorio");
            this.btnEnviarConfirmacion.UseVisualStyleBackColor = true;
            this.btnEnviarConfirmacion.Visible = false;
            // 
            // ReservaMesaSmall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnEnviarConfirmacion);
            this.Controls.Add(this.btnEliminar);
            this.Controls.Add(this.btnEditar);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ReservaMesaSmall";
            this.Size = new System.Drawing.Size(390, 103);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtMesa;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtInformacion;
        private System.Windows.Forms.ToolTip toolTip1;
        public System.Windows.Forms.Button btnEditar;
        public System.Windows.Forms.Button btnEliminar;
        public System.Windows.Forms.Button btnEnviarConfirmacion;
    }
}
