namespace CapaPresentacion.Formularios.FormsVentas
{
    partial class Filtrosventa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Filtrosventa));
            this.rdCliente = new System.Windows.Forms.RadioButton();
            this.rdNinguno = new System.Windows.Forms.RadioButton();
            this.rdEmpleado = new System.Windows.Forms.RadioButton();
            this.rdMesa = new System.Windows.Forms.RadioButton();
            this.btnSeleccione = new System.Windows.Forms.Button();
            this.txtTipo = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rdCliente
            // 
            this.rdCliente.AutoSize = true;
            this.rdCliente.Location = new System.Drawing.Point(3, 27);
            this.rdCliente.Name = "rdCliente";
            this.rdCliente.Size = new System.Drawing.Size(65, 21);
            this.rdCliente.TabIndex = 0;
            this.rdCliente.TabStop = true;
            this.rdCliente.Text = "Cliente";
            this.rdCliente.UseVisualStyleBackColor = true;
            // 
            // rdNinguno
            // 
            this.rdNinguno.AutoSize = true;
            this.rdNinguno.Location = new System.Drawing.Point(3, 3);
            this.rdNinguno.Name = "rdNinguno";
            this.rdNinguno.Size = new System.Drawing.Size(76, 21);
            this.rdNinguno.TabIndex = 1;
            this.rdNinguno.TabStop = true;
            this.rdNinguno.Text = "Ninguno";
            this.rdNinguno.UseVisualStyleBackColor = true;
            // 
            // rdEmpleado
            // 
            this.rdEmpleado.AutoSize = true;
            this.rdEmpleado.Location = new System.Drawing.Point(3, 50);
            this.rdEmpleado.Name = "rdEmpleado";
            this.rdEmpleado.Size = new System.Drawing.Size(85, 21);
            this.rdEmpleado.TabIndex = 2;
            this.rdEmpleado.TabStop = true;
            this.rdEmpleado.Text = "Empleado";
            this.rdEmpleado.UseVisualStyleBackColor = true;
            // 
            // rdMesa
            // 
            this.rdMesa.AutoSize = true;
            this.rdMesa.Location = new System.Drawing.Point(3, 73);
            this.rdMesa.Name = "rdMesa";
            this.rdMesa.Size = new System.Drawing.Size(58, 21);
            this.rdMesa.TabIndex = 3;
            this.rdMesa.TabStop = true;
            this.rdMesa.Text = "Mesa";
            this.rdMesa.UseVisualStyleBackColor = true;
            // 
            // btnSeleccione
            // 
            this.btnSeleccione.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccione.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnSeleccione.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccione.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnSeleccione.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccione.Image = ((System.Drawing.Image)(resources.GetObject("btnSeleccione.Image")));
            this.btnSeleccione.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSeleccione.Location = new System.Drawing.Point(104, 19);
            this.btnSeleccione.Name = "btnSeleccione";
            this.btnSeleccione.Size = new System.Drawing.Size(138, 59);
            this.btnSeleccione.TabIndex = 17;
            this.btnSeleccione.Text = "Seleccione un";
            this.btnSeleccione.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSeleccione.UseVisualStyleBackColor = true;
            // 
            // txtTipo
            // 
            this.txtTipo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtTipo.Location = new System.Drawing.Point(3, 104);
            this.txtTipo.Name = "txtTipo";
            this.txtTipo.ReadOnly = true;
            this.txtTipo.Size = new System.Drawing.Size(239, 25);
            this.txtTipo.TabIndex = 18;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdNinguno);
            this.panel1.Controls.Add(this.rdCliente);
            this.panel1.Controls.Add(this.rdEmpleado);
            this.panel1.Controls.Add(this.rdMesa);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(95, 95);
            this.panel1.TabIndex = 19;
            // 
            // Filtrosventa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.txtTipo);
            this.Controls.Add(this.btnSeleccione);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Filtrosventa";
            this.Size = new System.Drawing.Size(254, 137);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.Button btnSeleccione;
        public System.Windows.Forms.RadioButton rdCliente;
        public System.Windows.Forms.RadioButton rdNinguno;
        public System.Windows.Forms.RadioButton rdEmpleado;
        public System.Windows.Forms.RadioButton rdMesa;
        public System.Windows.Forms.TextBox txtTipo;
        public System.Windows.Forms.Panel panel1;
    }
}
