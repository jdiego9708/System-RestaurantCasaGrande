namespace CapaPresentacion.Formularios.Controles
{
    partial class UploadImage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UploadImage));
            this.pxImagen = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnAgregarComentario = new System.Windows.Forms.Button();
            this.btnQuitar = new System.Windows.Forms.Button();
            this.btnScreenShot = new System.Windows.Forms.Button();
            this.btnConfig = new System.Windows.Forms.Button();
            this.txtImagen = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pxImagen)).BeginInit();
            this.SuspendLayout();
            // 
            // pxImagen
            // 
            this.pxImagen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pxImagen.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pxImagen.Image = ((System.Drawing.Image)(resources.GetObject("pxImagen.Image")));
            this.pxImagen.Location = new System.Drawing.Point(3, 3);
            this.pxImagen.Name = "pxImagen";
            this.pxImagen.Size = new System.Drawing.Size(165, 150);
            this.pxImagen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxImagen.TabIndex = 0;
            this.pxImagen.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(174, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Imagen";
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccionar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnSeleccionar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionar.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.ForeColor = System.Drawing.Color.Gray;
            this.btnSeleccionar.Image = ((System.Drawing.Image)(resources.GetObject("btnSeleccionar.Image")));
            this.btnSeleccionar.Location = new System.Drawing.Point(174, 23);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(76, 37);
            this.btnSeleccionar.TabIndex = 7;
            this.toolTip1.SetToolTip(this.btnSeleccionar, "Seleccione una imagen de su equipo");
            this.btnSeleccionar.UseVisualStyleBackColor = false;
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnLimpiar.BackColor = System.Drawing.Color.White;
            this.btnLimpiar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLimpiar.BackgroundImage")));
            this.btnLimpiar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLimpiar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLimpiar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnLimpiar.FlatAppearance.BorderSize = 0;
            this.btnLimpiar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnLimpiar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnLimpiar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimpiar.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLimpiar.ForeColor = System.Drawing.Color.Gray;
            this.btnLimpiar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLimpiar.Location = new System.Drawing.Point(252, 118);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(35, 35);
            this.btnLimpiar.TabIndex = 9;
            this.btnLimpiar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnLimpiar, "Limpiar imagen");
            this.btnLimpiar.UseVisualStyleBackColor = false;
            // 
            // btnAgregarComentario
            // 
            this.btnAgregarComentario.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnAgregarComentario.BackColor = System.Drawing.Color.White;
            this.btnAgregarComentario.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAgregarComentario.BackgroundImage")));
            this.btnAgregarComentario.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAgregarComentario.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAgregarComentario.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnAgregarComentario.FlatAppearance.BorderSize = 0;
            this.btnAgregarComentario.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnAgregarComentario.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnAgregarComentario.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarComentario.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarComentario.ForeColor = System.Drawing.Color.Gray;
            this.btnAgregarComentario.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAgregarComentario.Location = new System.Drawing.Point(212, 118);
            this.btnAgregarComentario.Name = "btnAgregarComentario";
            this.btnAgregarComentario.Size = new System.Drawing.Size(34, 35);
            this.btnAgregarComentario.TabIndex = 10;
            this.btnAgregarComentario.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnAgregarComentario, "Agregar comentario");
            this.btnAgregarComentario.UseVisualStyleBackColor = false;
            // 
            // btnQuitar
            // 
            this.btnQuitar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnQuitar.BackColor = System.Drawing.Color.White;
            this.btnQuitar.BackgroundImage = global::CapaPresentacion.Properties.Resources.negative;
            this.btnQuitar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuitar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuitar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnQuitar.FlatAppearance.BorderSize = 0;
            this.btnQuitar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnQuitar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnQuitar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitar.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuitar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnQuitar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnQuitar.Location = new System.Drawing.Point(291, 118);
            this.btnQuitar.Name = "btnQuitar";
            this.btnQuitar.Size = new System.Drawing.Size(35, 35);
            this.btnQuitar.TabIndex = 11;
            this.toolTip1.SetToolTip(this.btnQuitar, "Quitar imagen");
            this.btnQuitar.UseVisualStyleBackColor = false;
            // 
            // btnScreenShot
            // 
            this.btnScreenShot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScreenShot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnScreenShot.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScreenShot.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnScreenShot.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnScreenShot.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnScreenShot.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScreenShot.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScreenShot.ForeColor = System.Drawing.Color.Gray;
            this.btnScreenShot.Image = ((System.Drawing.Image)(resources.GetObject("btnScreenShot.Image")));
            this.btnScreenShot.Location = new System.Drawing.Point(256, 23);
            this.btnScreenShot.Name = "btnScreenShot";
            this.btnScreenShot.Size = new System.Drawing.Size(83, 37);
            this.btnScreenShot.TabIndex = 12;
            this.btnScreenShot.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnScreenShot, "Tome una captura de pantalla");
            this.btnScreenShot.UseVisualStyleBackColor = false;
            // 
            // btnConfig
            // 
            this.btnConfig.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnConfig.BackColor = System.Drawing.Color.White;
            this.btnConfig.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnConfig.BackgroundImage")));
            this.btnConfig.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnConfig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfig.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnConfig.FlatAppearance.BorderSize = 0;
            this.btnConfig.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnConfig.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfig.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfig.ForeColor = System.Drawing.Color.Gray;
            this.btnConfig.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfig.Location = new System.Drawing.Point(172, 118);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(34, 35);
            this.btnConfig.TabIndex = 13;
            this.btnConfig.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnConfig, "Configuraciones de imagen");
            this.btnConfig.UseVisualStyleBackColor = false;
            // 
            // txtImagen
            // 
            this.txtImagen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImagen.BackColor = System.Drawing.Color.White;
            this.txtImagen.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtImagen.ForeColor = System.Drawing.Color.Gray;
            this.txtImagen.Location = new System.Drawing.Point(174, 66);
            this.txtImagen.Multiline = true;
            this.txtImagen.Name = "txtImagen";
            this.txtImagen.Size = new System.Drawing.Size(165, 46);
            this.txtImagen.TabIndex = 8;
            // 
            // UploadImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.btnScreenShot);
            this.Controls.Add(this.btnQuitar);
            this.Controls.Add(this.btnAgregarComentario);
            this.Controls.Add(this.btnLimpiar);
            this.Controls.Add(this.txtImagen);
            this.Controls.Add(this.btnSeleccionar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pxImagen);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "UploadImage";
            this.Size = new System.Drawing.Size(342, 156);
            ((System.ComponentModel.ISupportInitialize)(this.pxImagen)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pxImagen;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSeleccionar;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtImagen;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnAgregarComentario;
        public System.Windows.Forms.Button btnQuitar;
        private System.Windows.Forms.Button btnScreenShot;
        private System.Windows.Forms.Button btnConfig;
    }
}
