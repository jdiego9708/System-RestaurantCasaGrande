namespace CapaPresentacion.Formularios
{
    partial class CustomButtonMesa
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
            this.btnMesa = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnMesa
            // 
            this.btnMesa.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMesa.BackColor = System.Drawing.Color.DarkOrange;
            this.btnMesa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMesa.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMesa.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnMesa.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGray;
            this.btnMesa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMesa.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMesa.Location = new System.Drawing.Point(0, 0);
            this.btnMesa.Name = "btnMesa";
            this.btnMesa.Size = new System.Drawing.Size(197, 148);
            this.btnMesa.TabIndex = 0;
            this.btnMesa.Tag = "0";
            this.btnMesa.Text = "Texto";
            this.btnMesa.UseVisualStyleBackColor = false;
            // 
            // CustomButtonMesa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnMesa);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "CustomButtonMesa";
            this.Size = new System.Drawing.Size(197, 148);
            this.Tag = "0";
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnMesa;
    }
}
