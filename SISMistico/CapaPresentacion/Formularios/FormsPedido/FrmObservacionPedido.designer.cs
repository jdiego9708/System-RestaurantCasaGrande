namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmObservacionPedido
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

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservacionPedido));
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.panelToolBox = new System.Windows.Forms.Panel();
            this.panelCenter = new System.Windows.Forms.Panel();
            this.pxMaximize = new System.Windows.Forms.PictureBox();
            this.pxMinimize = new System.Windows.Forms.PictureBox();
            this.pxClose = new System.Windows.Forms.PictureBox();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.toolTipToolBox = new System.Windows.Forms.ToolTip(this.components);
            this.panelRight = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtObservacion = new System.Windows.Forms.TextBox();
            this.btnListo = new System.Windows.Forms.Button();
            this.panelToolBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pxMaximize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxClose)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Observaciones";
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelBottom.Location = new System.Drawing.Point(-184, 203);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(659, 2);
            this.panelBottom.TabIndex = 4;
            // 
            // panelToolBox
            // 
            this.panelToolBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelToolBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelToolBox.Controls.Add(this.panelCenter);
            this.panelToolBox.Controls.Add(this.pxMaximize);
            this.panelToolBox.Controls.Add(this.pxMinimize);
            this.panelToolBox.Controls.Add(this.pxClose);
            this.panelToolBox.Controls.Add(this.label1);
            this.panelToolBox.ForeColor = System.Drawing.Color.White;
            this.panelToolBox.Location = new System.Drawing.Point(2, 2);
            this.panelToolBox.Name = "panelToolBox";
            this.panelToolBox.Size = new System.Drawing.Size(471, 44);
            this.panelToolBox.TabIndex = 6;
            // 
            // panelCenter
            // 
            this.panelCenter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCenter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelCenter.Location = new System.Drawing.Point(0, 41);
            this.panelCenter.Name = "panelCenter";
            this.panelCenter.Size = new System.Drawing.Size(466, 3);
            this.panelCenter.TabIndex = 5;
            // 
            // pxMaximize
            // 
            this.pxMaximize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxMaximize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxMaximize.Image = ((System.Drawing.Image)(resources.GetObject("pxMaximize.Image")));
            this.pxMaximize.Location = new System.Drawing.Point(422, 0);
            this.pxMaximize.Name = "pxMaximize";
            this.pxMaximize.Size = new System.Drawing.Size(23, 21);
            this.pxMaximize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxMaximize.TabIndex = 3;
            this.pxMaximize.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxMaximize, "Maximizar");
            // 
            // pxMinimize
            // 
            this.pxMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxMinimize.Image = ((System.Drawing.Image)(resources.GetObject("pxMinimize.Image")));
            this.pxMinimize.Location = new System.Drawing.Point(398, 0);
            this.pxMinimize.Name = "pxMinimize";
            this.pxMinimize.Size = new System.Drawing.Size(23, 21);
            this.pxMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxMinimize.TabIndex = 2;
            this.pxMinimize.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxMinimize, "Minimizar");
            // 
            // pxClose
            // 
            this.pxClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pxClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pxClose.Image = ((System.Drawing.Image)(resources.GetObject("pxClose.Image")));
            this.pxClose.Location = new System.Drawing.Point(445, 0);
            this.pxClose.Name = "pxClose";
            this.pxClose.Size = new System.Drawing.Size(23, 21);
            this.pxClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pxClose.TabIndex = 1;
            this.pxClose.TabStop = false;
            this.toolTipToolBox.SetToolTip(this.pxClose, "Cerrar");
            // 
            // panelLeft
            // 
            this.panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(2, 208);
            this.panelLeft.TabIndex = 5;
            // 
            // panelTop
            // 
            this.panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelTop.Location = new System.Drawing.Point(-187, -1);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(659, 2);
            this.panelTop.TabIndex = 5;
            // 
            // panelRight
            // 
            this.panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.panelRight.Location = new System.Drawing.Point(471, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(2, 207);
            this.panelRight.TabIndex = 6;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.btnListo);
            this.panel1.Controls.Add(this.txtObservacion);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(2, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(468, 157);
            this.panel1.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Observación:";
            // 
            // txtObservacion
            // 
            this.txtObservacion.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtObservacion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtObservacion.Location = new System.Drawing.Point(6, 21);
            this.txtObservacion.Multiline = true;
            this.txtObservacion.Name = "txtObservacion";
            this.txtObservacion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtObservacion.Size = new System.Drawing.Size(452, 79);
            this.txtObservacion.TabIndex = 1;
            // 
            // btnListo
            // 
            this.btnListo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnListo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnListo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnListo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListo.Location = new System.Drawing.Point(157, 102);
            this.btnListo.Name = "btnListo";
            this.btnListo.Size = new System.Drawing.Size(151, 48);
            this.btnListo.TabIndex = 15;
            this.btnListo.Text = "Listo";
            this.btnListo.UseVisualStyleBackColor = true;
            // 
            // FrmObservacionPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(474, 205);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelLeft);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelToolBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmObservacionPedido";
            this.Text = "Form1";
            this.panelToolBox.ResumeLayout(false);
            this.panelToolBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pxMaximize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pxClose)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Panel panelToolBox;
        private System.Windows.Forms.PictureBox pxClose;
        private System.Windows.Forms.PictureBox pxMinimize;
        private System.Windows.Forms.PictureBox pxMaximize;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelCenter;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ToolTip toolTipToolBox;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtObservacion;
        public System.Windows.Forms.Button btnListo;
    }
}

