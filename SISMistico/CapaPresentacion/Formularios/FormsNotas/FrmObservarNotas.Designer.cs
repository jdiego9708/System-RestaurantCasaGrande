namespace CapaPresentacion.Formularios.FormsNotas
{
    partial class FrmObservarNotas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarNotas));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtEmpleado = new System.Windows.Forms.TextBox();
            this.lblResultados = new System.Windows.Forms.Label();
            this.panelNotas = new CapaPresentacion.Controles.CustomGridPanel();
            this.btnNuevaNota = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtEmpleado);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(311, 55);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Empleado";
            // 
            // txtEmpleado
            // 
            this.txtEmpleado.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtEmpleado.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtEmpleado.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtEmpleado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtEmpleado.Location = new System.Drawing.Point(6, 25);
            this.txtEmpleado.Name = "txtEmpleado";
            this.txtEmpleado.ReadOnly = true;
            this.txtEmpleado.Size = new System.Drawing.Size(299, 20);
            this.txtEmpleado.TabIndex = 0;
            this.txtEmpleado.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblResultados
            // 
            this.lblResultados.AutoSize = true;
            this.lblResultados.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblResultados.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultados.Location = new System.Drawing.Point(12, 78);
            this.lblResultados.Name = "lblResultados";
            this.lblResultados.Size = new System.Drawing.Size(128, 15);
            this.lblResultados.TabIndex = 3;
            this.lblResultados.Text = "Última nota (Ver todas)";
            // 
            // panelNotas
            // 
            this.panelNotas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNotas.AutoScroll = true;
            this.panelNotas.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelNotas.BackgroundImage")));
            this.panelNotas.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelNotas.Location = new System.Drawing.Point(12, 107);
            this.panelNotas.Name = "panelNotas";
            this.panelNotas.PageSize = 10;
            this.panelNotas.Size = new System.Drawing.Size(312, 211);
            this.panelNotas.TabIndex = 2;
            // 
            // btnNuevaNota
            // 
            this.btnNuevaNota.BackColor = System.Drawing.Color.White;
            this.btnNuevaNota.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnNuevaNota.BackgroundImage")));
            this.btnNuevaNota.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNuevaNota.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNuevaNota.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnNuevaNota.FlatAppearance.BorderSize = 0;
            this.btnNuevaNota.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnNuevaNota.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnNuevaNota.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevaNota.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNuevaNota.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnNuevaNota.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnNuevaNota.Location = new System.Drawing.Point(283, 71);
            this.btnNuevaNota.Name = "btnNuevaNota";
            this.btnNuevaNota.Size = new System.Drawing.Size(30, 30);
            this.btnNuevaNota.TabIndex = 12;
            this.btnNuevaNota.UseVisualStyleBackColor = false;
            // 
            // FrmObservarNotas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(336, 321);
            this.Controls.Add(this.btnNuevaNota);
            this.Controls.Add(this.lblResultados);
            this.Controls.Add(this.panelNotas);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmObservarNotas";
            this.Text = "Observar notas";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtEmpleado;
        private CapaPresentacion.Controles.CustomGridPanel panelNotas;
        private System.Windows.Forms.Label lblResultados;
        public System.Windows.Forms.Button btnNuevaNota;
    }
}