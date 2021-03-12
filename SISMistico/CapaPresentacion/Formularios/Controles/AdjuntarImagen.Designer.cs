namespace CapaPresentacion.Formularios.Controles
{
    partial class AdjuntarImagen
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtImagen = new System.Windows.Forms.TextBox();
            this.pxImagen = new System.Windows.Forms.PictureBox();
            this.btnImagen = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pxImagen)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Imagen";
            // 
            // txtImagen
            // 
            this.txtImagen.Location = new System.Drawing.Point(0, 22);
            this.txtImagen.Name = "txtImagen";
            this.txtImagen.ReadOnly = true;
            this.txtImagen.Size = new System.Drawing.Size(224, 29);
            this.txtImagen.TabIndex = 1;
            this.txtImagen.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pxImagen
            // 
            this.pxImagen.Location = new System.Drawing.Point(0, 95);
            this.pxImagen.Name = "pxImagen";
            this.pxImagen.Size = new System.Drawing.Size(224, 165);
            this.pxImagen.TabIndex = 2;
            this.pxImagen.TabStop = false;
            // 
            // btnImagen
            // 
            this.btnImagen.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnImagen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnImagen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnImagen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImagen.Location = new System.Drawing.Point(0, 53);
            this.btnImagen.Name = "btnImagen";
            this.btnImagen.Size = new System.Drawing.Size(224, 36);
            this.btnImagen.TabIndex = 3;
            this.btnImagen.Text = "Seleccionar";
            this.btnImagen.UseVisualStyleBackColor = true;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnLimpiar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnLimpiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.Location = new System.Drawing.Point(0, 261);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(96, 31);
            this.btnLimpiar.TabIndex = 4;
            this.btnLimpiar.Text = "Limpiar";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // AdjuntarImagen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.btnImagen);
            this.Controls.Add(this.pxImagen);
            this.Controls.Add(this.txtImagen);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AdjuntarImagen";
            this.Size = new System.Drawing.Size(227, 295);
            ((System.ComponentModel.ISupportInitialize)(this.pxImagen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtImagen;
        private System.Windows.Forms.PictureBox pxImagen;
        private System.Windows.Forms.Button btnImagen;
        private System.Windows.Forms.Button btnLimpiar;
    }
}
