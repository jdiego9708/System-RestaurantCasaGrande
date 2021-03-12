namespace CapaPresentacion.Servicios
{
    partial class FrmEmailTicket
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEmailTicket));
            this.chkPasos = new System.Windows.Forms.CheckBox();
            this.txtDescripcion = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFechaHora = new System.Windows.Forms.Label();
            this.txtAsunto = new CapaPresentacion.CustomTextBox();
            this.panelPasos = new CapaPresentacion.Controles.CustomGridPanel();
            this.btnAddPaso = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnEnviar = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // chkPasos
            // 
            this.chkPasos.AutoSize = true;
            this.chkPasos.Location = new System.Drawing.Point(109, 210);
            this.chkPasos.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkPasos.Name = "chkPasos";
            this.chkPasos.Size = new System.Drawing.Size(124, 24);
            this.chkPasos.TabIndex = 11;
            this.chkPasos.Text = "Agregar pasos";
            this.chkPasos.UseVisualStyleBackColor = true;
            // 
            // txtDescripcion
            // 
            this.txtDescripcion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescripcion.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtDescripcion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescripcion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtDescripcion.Location = new System.Drawing.Point(6, 75);
            this.txtDescripcion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDescripcion.Multiline = true;
            this.txtDescripcion.Name = "txtDescripcion";
            this.txtDescripcion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescripcion.Size = new System.Drawing.Size(554, 128);
            this.txtDescripcion.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-1, 50);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 20);
            this.label2.TabIndex = 9;
            this.label2.Text = "Descripción del error:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-1, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Asunto:";
            // 
            // lblFechaHora
            // 
            this.lblFechaHora.AutoSize = true;
            this.lblFechaHora.Location = new System.Drawing.Point(-1, 4);
            this.lblFechaHora.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFechaHora.Name = "lblFechaHora";
            this.lblFechaHora.Size = new System.Drawing.Size(95, 20);
            this.lblFechaHora.TabIndex = 6;
            this.lblFechaHora.Text = "Fecha y hora:";
            // 
            // txtAsunto
            // 
            this.txtAsunto.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAsunto.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtAsunto.BackColor = System.Drawing.Color.White;
            this.txtAsunto.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAsunto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.errorProvider1.SetIconPadding(this.txtAsunto, -3);
            this.txtAsunto.Location = new System.Drawing.Point(59, 26);
            this.txtAsunto.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtAsunto.Name = "txtAsunto";
            this.txtAsunto.Size = new System.Drawing.Size(485, 20);
            this.txtAsunto.TabIndex = 12;
            this.txtAsunto.Texto = null;
            this.txtAsunto.Texto_inicial = null;
            this.txtAsunto.Visible_px = false;
            // 
            // panelPasos
            // 
            this.panelPasos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPasos.AutoScroll = true;
            this.panelPasos.AutoSize = true;
            this.panelPasos.Location = new System.Drawing.Point(3, 245);
            this.panelPasos.Name = "panelPasos";
            this.panelPasos.Size = new System.Drawing.Size(557, 5);
            this.panelPasos.TabIndex = 13;
            this.panelPasos.Visible = false;
            // 
            // btnAddPaso
            // 
            this.btnAddPaso.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddPaso.BackgroundImage")));
            this.btnAddPaso.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddPaso.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPaso.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAddPaso.FlatAppearance.BorderSize = 0;
            this.btnAddPaso.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnAddPaso.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAddPaso.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddPaso.Location = new System.Drawing.Point(241, 205);
            this.btnAddPaso.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAddPaso.Name = "btnAddPaso";
            this.btnAddPaso.Size = new System.Drawing.Size(35, 34);
            this.btnAddPaso.TabIndex = 16;
            this.toolTip1.SetToolTip(this.btnAddPaso, "Añadir paso");
            this.btnAddPaso.UseVisualStyleBackColor = true;
            this.btnAddPaso.Visible = false;
            // 
            // toolTip1
            // 
            this.toolTip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            // 
            // btnEnviar
            // 
            this.btnEnviar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEnviar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnEnviar.FlatAppearance.BorderSize = 0;
            this.btnEnviar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnEnviar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnEnviar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEnviar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEnviar.Image = ((System.Drawing.Image)(resources.GetObject("btnEnviar.Image")));
            this.btnEnviar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnEnviar.Location = new System.Drawing.Point(6, 202);
            this.btnEnviar.Name = "btnEnviar";
            this.btnEnviar.Size = new System.Drawing.Size(88, 41);
            this.btnEnviar.TabIndex = 17;
            this.btnEnviar.Text = "Enviar";
            this.btnEnviar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEnviar.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // FrmEmailTicket
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(573, 254);
            this.Controls.Add(this.btnEnviar);
            this.Controls.Add(this.btnAddPaso);
            this.Controls.Add(this.panelPasos);
            this.Controls.Add(this.txtAsunto);
            this.Controls.Add(this.chkPasos);
            this.Controls.Add(this.txtDescripcion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFechaHora);
            this.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "FrmEmailTicket";
            this.Text = "Ticket - Enviar pasos";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkPasos;
        private System.Windows.Forms.TextBox txtDescripcion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFechaHora;
        private CustomTextBox txtAsunto;
        private Controles.CustomGridPanel panelPasos;
        public System.Windows.Forms.Button btnAddPaso;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Button btnEnviar;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}