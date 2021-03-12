namespace CapaPresentacion.Formularios.FormsEmpleados
{
    partial class FrmObservarEmpleados
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarEmpleados));
            this.lblResultados = new System.Windows.Forms.Label();
            this.dgvEmpleados = new System.Windows.Forms.DataGridView();
            this.btnConfiguracionColumnas = new System.Windows.Forms.Button();
            this.panelColumnas = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdActivo = new System.Windows.Forms.RadioButton();
            this.rdInactivo = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnBuscar = new System.Windows.Forms.Button();
            this.txtBusqueda = new CapaPresentacion.CustomTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmpleados)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblResultados
            // 
            this.lblResultados.AutoSize = true;
            this.lblResultados.Location = new System.Drawing.Point(6, 73);
            this.lblResultados.Name = "lblResultados";
            this.lblResultados.Size = new System.Drawing.Size(72, 17);
            this.lblResultados.TabIndex = 1;
            this.lblResultados.Text = "Resultados";
            // 
            // dgvEmpleados
            // 
            this.dgvEmpleados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvEmpleados.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvEmpleados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEmpleados.Location = new System.Drawing.Point(12, 157);
            this.dgvEmpleados.Name = "dgvEmpleados";
            this.dgvEmpleados.Size = new System.Drawing.Size(867, 308);
            this.dgvEmpleados.TabIndex = 2;
            // 
            // btnConfiguracionColumnas
            // 
            this.btnConfiguracionColumnas.BackColor = System.Drawing.Color.White;
            this.btnConfiguracionColumnas.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnConfiguracionColumnas.BackgroundImage")));
            this.btnConfiguracionColumnas.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnConfiguracionColumnas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfiguracionColumnas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnConfiguracionColumnas.FlatAppearance.BorderSize = 0;
            this.btnConfiguracionColumnas.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnConfiguracionColumnas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnConfiguracionColumnas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfiguracionColumnas.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfiguracionColumnas.ForeColor = System.Drawing.Color.Gray;
            this.btnConfiguracionColumnas.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfiguracionColumnas.Location = new System.Drawing.Point(6, 97);
            this.btnConfiguracionColumnas.Name = "btnConfiguracionColumnas";
            this.btnConfiguracionColumnas.Size = new System.Drawing.Size(30, 30);
            this.btnConfiguracionColumnas.TabIndex = 14;
            this.btnConfiguracionColumnas.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnConfiguracionColumnas, "Configuración de columnas");
            this.btnConfiguracionColumnas.UseVisualStyleBackColor = false;
            // 
            // panelColumnas
            // 
            this.panelColumnas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelColumnas.Location = new System.Drawing.Point(43, 102);
            this.panelColumnas.Name = "panelColumnas";
            this.panelColumnas.Size = new System.Drawing.Size(820, 25);
            this.panelColumnas.TabIndex = 15;
            this.panelColumnas.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnBuscar);
            this.groupBox1.Controls.Add(this.rdInactivo);
            this.groupBox1.Controls.Add(this.rdActivo);
            this.groupBox1.Controls.Add(this.txtBusqueda);
            this.groupBox1.Controls.Add(this.panelColumnas);
            this.groupBox1.Controls.Add(this.lblResultados);
            this.groupBox1.Controls.Add(this.btnConfiguracionColumnas);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(867, 139);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Búsqueda";
            // 
            // rdActivo
            // 
            this.rdActivo.AutoSize = true;
            this.rdActivo.Checked = true;
            this.rdActivo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdActivo.Location = new System.Drawing.Point(9, 24);
            this.rdActivo.Name = "rdActivo";
            this.rdActivo.Size = new System.Drawing.Size(85, 25);
            this.rdActivo.TabIndex = 16;
            this.rdActivo.TabStop = true;
            this.rdActivo.Text = "ACTIVO";
            this.rdActivo.UseVisualStyleBackColor = true;
            // 
            // rdInactivo
            // 
            this.rdInactivo.AutoSize = true;
            this.rdInactivo.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdInactivo.Location = new System.Drawing.Point(100, 24);
            this.rdInactivo.Name = "rdInactivo";
            this.rdInactivo.Size = new System.Drawing.Size(102, 25);
            this.rdInactivo.TabIndex = 17;
            this.rdInactivo.Text = "INACTIVO";
            this.rdInactivo.UseVisualStyleBackColor = true;
            // 
            // btnBuscar
            // 
            this.btnBuscar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBuscar.BackColor = System.Drawing.Color.White;
            this.btnBuscar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBuscar.BackgroundImage")));
            this.btnBuscar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBuscar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuscar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnBuscar.FlatAppearance.BorderSize = 0;
            this.btnBuscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnBuscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Font = new System.Drawing.Font("Segoe UI Semibold", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBuscar.ForeColor = System.Drawing.Color.Gray;
            this.btnBuscar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBuscar.Location = new System.Drawing.Point(826, 47);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(30, 30);
            this.btnBuscar.TabIndex = 18;
            this.btnBuscar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnBuscar, "Configuración de columnas");
            this.btnBuscar.UseVisualStyleBackColor = false;
            // 
            // txtBusqueda
            // 
            this.txtBusqueda.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBusqueda.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtBusqueda.BackColor = System.Drawing.Color.White;
            this.txtBusqueda.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBusqueda.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtBusqueda.Location = new System.Drawing.Point(9, 48);
            this.txtBusqueda.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBusqueda.Name = "txtBusqueda";
            this.txtBusqueda.Size = new System.Drawing.Size(811, 20);
            this.txtBusqueda.TabIndex = 3;
            this.txtBusqueda.Texto = null;
            this.txtBusqueda.Texto_inicial = null;
            this.txtBusqueda.Visible_px = false;
            // 
            // FrmObservarEmpleados
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(891, 477);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dgvEmpleados);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmObservarEmpleados";
            this.Text = "Observar empleados";
            ((System.ComponentModel.ISupportInitialize)(this.dgvEmpleados)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblResultados;
        private System.Windows.Forms.DataGridView dgvEmpleados;
        private CustomTextBox txtBusqueda;
        private System.Windows.Forms.Button btnConfiguracionColumnas;
        private System.Windows.Forms.Panel panelColumnas;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdActivo;
        private System.Windows.Forms.RadioButton rdInactivo;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnBuscar;
    }
}