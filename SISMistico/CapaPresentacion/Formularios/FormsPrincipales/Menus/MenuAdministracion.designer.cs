namespace CapaPresentacion.Formularios.FormsPrincipales.Menus
{
    partial class MenuAdministracion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuAdministracion));
            this.btnEstadisticasDiarias = new System.Windows.Forms.Button();
            this.btnObservarMovimientos = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnEstadisticasDiarias
            // 
            this.btnEstadisticasDiarias.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEstadisticasDiarias.FlatAppearance.BorderSize = 0;
            this.btnEstadisticasDiarias.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEstadisticasDiarias.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEstadisticasDiarias.ForeColor = System.Drawing.Color.White;
            this.btnEstadisticasDiarias.Image = ((System.Drawing.Image)(resources.GetObject("btnEstadisticasDiarias.Image")));
            this.btnEstadisticasDiarias.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEstadisticasDiarias.Location = new System.Drawing.Point(4, 58);
            this.btnEstadisticasDiarias.Name = "btnEstadisticasDiarias";
            this.btnEstadisticasDiarias.Size = new System.Drawing.Size(191, 49);
            this.btnEstadisticasDiarias.TabIndex = 8;
            this.btnEstadisticasDiarias.Text = "Estadísticas";
            this.btnEstadisticasDiarias.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnEstadisticasDiarias.UseVisualStyleBackColor = true;
            // 
            // btnObservarMovimientos
            // 
            this.btnObservarMovimientos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnObservarMovimientos.FlatAppearance.BorderSize = 0;
            this.btnObservarMovimientos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnObservarMovimientos.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnObservarMovimientos.ForeColor = System.Drawing.Color.White;
            this.btnObservarMovimientos.Image = ((System.Drawing.Image)(resources.GetObject("btnObservarMovimientos.Image")));
            this.btnObservarMovimientos.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnObservarMovimientos.Location = new System.Drawing.Point(4, 3);
            this.btnObservarMovimientos.Name = "btnObservarMovimientos";
            this.btnObservarMovimientos.Size = new System.Drawing.Size(191, 49);
            this.btnObservarMovimientos.TabIndex = 3;
            this.btnObservarMovimientos.Text = "Observar movimientos";
            this.btnObservarMovimientos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnObservarMovimientos.UseVisualStyleBackColor = true;
            // 
            // MenuAdministracion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(152)))), ((int)(((byte)(202)))));
            this.Controls.Add(this.btnEstadisticasDiarias);
            this.Controls.Add(this.btnObservarMovimientos);
            this.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MenuAdministracion";
            this.Size = new System.Drawing.Size(195, 112);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Button btnObservarMovimientos;
        public System.Windows.Forms.Button btnEstadisticasDiarias;
    }
}
