namespace CapaPresentacion.Formularios.FormsReservas
{
    partial class MenuNotification
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuNotification));
            this.btnOcultarNotificacion = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOcultarNotificacion
            // 
            this.btnOcultarNotificacion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOcultarNotificacion.FlatAppearance.BorderSize = 0;
            this.btnOcultarNotificacion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOcultarNotificacion.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOcultarNotificacion.ForeColor = System.Drawing.Color.White;
            this.btnOcultarNotificacion.Image = ((System.Drawing.Image)(resources.GetObject("btnOcultarNotificacion.Image")));
            this.btnOcultarNotificacion.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOcultarNotificacion.Location = new System.Drawing.Point(0, 1);
            this.btnOcultarNotificacion.Name = "btnOcultarNotificacion";
            this.btnOcultarNotificacion.Size = new System.Drawing.Size(151, 49);
            this.btnOcultarNotificacion.TabIndex = 5;
            this.btnOcultarNotificacion.Text = "Ocultar";
            this.btnOcultarNotificacion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOcultarNotificacion.UseVisualStyleBackColor = true;
            // 
            // MenuNotification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(152)))), ((int)(((byte)(202)))));
            this.Controls.Add(this.btnOcultarNotificacion);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MenuNotification";
            this.Size = new System.Drawing.Size(154, 51);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Button btnOcultarNotificacion;
    }
}
