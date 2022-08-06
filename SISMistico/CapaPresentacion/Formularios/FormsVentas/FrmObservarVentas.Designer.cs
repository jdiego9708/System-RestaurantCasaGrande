namespace CapaPresentacion.Formularios.FormsVentas
{
    partial class FrmObservarVentas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarVentas));
            this.date1 = new System.Windows.Forms.DateTimePicker();
            this.date2 = new System.Windows.Forms.DateTimePicker();
            this.btnBuscar = new System.Windows.Forms.Button();
            this.dgvVentas = new System.Windows.Forms.DataGridView();
            this.btnResumen = new System.Windows.Forms.Button();
            this.ListaHora1 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.ListaHora2 = new System.Windows.Forms.ComboBox();
            this.panelFecha2 = new System.Windows.Forms.Panel();
            this.panelFecha1 = new System.Windows.Forms.Panel();
            this.btnFiltros = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnOtrasOpciones = new System.Windows.Forms.Button();
            this.gbOpciones = new System.Windows.Forms.GroupBox();
            this.btnEliminarCuentas = new System.Windows.Forms.Button();
            this.chkEliminarCuentas = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVentas)).BeginInit();
            this.panelFecha2.SuspendLayout();
            this.panelFecha1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gbOpciones.SuspendLayout();
            this.SuspendLayout();
            // 
            // date1
            // 
            this.date1.CalendarForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.date1.CalendarTitleForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.date1.Location = new System.Drawing.Point(3, 20);
            this.date1.Name = "date1";
            this.date1.Size = new System.Drawing.Size(245, 25);
            this.date1.TabIndex = 0;
            // 
            // date2
            // 
            this.date2.CalendarForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.date2.CalendarTitleForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.date2.Location = new System.Drawing.Point(5, 20);
            this.date2.Name = "date2";
            this.date2.Size = new System.Drawing.Size(245, 25);
            this.date2.TabIndex = 2;
            // 
            // btnBuscar
            // 
            this.btnBuscar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBuscar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnBuscar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnBuscar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnBuscar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBuscar.Image = ((System.Drawing.Image)(resources.GetObject("btnBuscar.Image")));
            this.btnBuscar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBuscar.Location = new System.Drawing.Point(6, 102);
            this.btnBuscar.Name = "btnBuscar";
            this.btnBuscar.Size = new System.Drawing.Size(96, 51);
            this.btnBuscar.TabIndex = 16;
            this.btnBuscar.Text = "Buscar";
            this.btnBuscar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBuscar.UseVisualStyleBackColor = true;
            // 
            // dgvVentas
            // 
            this.dgvVentas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVentas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVentas.Location = new System.Drawing.Point(7, 169);
            this.dgvVentas.Name = "dgvVentas";
            this.dgvVentas.Size = new System.Drawing.Size(794, 249);
            this.dgvVentas.TabIndex = 18;
            // 
            // btnResumen
            // 
            this.btnResumen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnResumen.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnResumen.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnResumen.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnResumen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnResumen.Image = ((System.Drawing.Image)(resources.GetObject("btnResumen.Image")));
            this.btnResumen.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnResumen.Location = new System.Drawing.Point(108, 102);
            this.btnResumen.Name = "btnResumen";
            this.btnResumen.Size = new System.Drawing.Size(124, 51);
            this.btnResumen.TabIndex = 19;
            this.btnResumen.Text = "Resumen de resultados";
            this.btnResumen.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnResumen.UseVisualStyleBackColor = true;
            this.btnResumen.Visible = false;
            // 
            // ListaHora1
            // 
            this.ListaHora1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ListaHora1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ListaHora1.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListaHora1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ListaHora1.FormattingEnabled = true;
            this.ListaHora1.Location = new System.Drawing.Point(274, 19);
            this.ListaHora1.Name = "ListaHora1";
            this.ListaHora1.Size = new System.Drawing.Size(84, 28);
            this.ListaHora1.TabIndex = 20;
            this.ListaHora1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(250, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 17);
            this.label2.TabIndex = 21;
            this.label2.Text = "---";
            this.label2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Fecha de inicio:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(2, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "Fecha fin:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(253, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(23, 17);
            this.label5.TabIndex = 25;
            this.label5.Text = "---";
            this.label5.Visible = false;
            // 
            // ListaHora2
            // 
            this.ListaHora2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ListaHora2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ListaHora2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ListaHora2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ListaHora2.FormattingEnabled = true;
            this.ListaHora2.Location = new System.Drawing.Point(277, 19);
            this.ListaHora2.Name = "ListaHora2";
            this.ListaHora2.Size = new System.Drawing.Size(84, 28);
            this.ListaHora2.TabIndex = 24;
            this.ListaHora2.Visible = false;
            // 
            // panelFecha2
            // 
            this.panelFecha2.Controls.Add(this.date2);
            this.panelFecha2.Controls.Add(this.label5);
            this.panelFecha2.Controls.Add(this.label4);
            this.panelFecha2.Controls.Add(this.ListaHora2);
            this.panelFecha2.Location = new System.Drawing.Point(394, 23);
            this.panelFecha2.Name = "panelFecha2";
            this.panelFecha2.Size = new System.Drawing.Size(378, 51);
            this.panelFecha2.TabIndex = 26;
            // 
            // panelFecha1
            // 
            this.panelFecha1.Controls.Add(this.date1);
            this.panelFecha1.Controls.Add(this.ListaHora1);
            this.panelFecha1.Controls.Add(this.label3);
            this.panelFecha1.Controls.Add(this.label2);
            this.panelFecha1.Location = new System.Drawing.Point(6, 23);
            this.panelFecha1.Name = "panelFecha1";
            this.panelFecha1.Size = new System.Drawing.Size(382, 52);
            this.panelFecha1.TabIndex = 27;
            // 
            // btnFiltros
            // 
            this.btnFiltros.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFiltros.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnFiltros.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnFiltros.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnFiltros.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltros.Image = ((System.Drawing.Image)(resources.GetObject("btnFiltros.Image")));
            this.btnFiltros.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnFiltros.Location = new System.Drawing.Point(238, 102);
            this.btnFiltros.Name = "btnFiltros";
            this.btnFiltros.Size = new System.Drawing.Size(117, 51);
            this.btnFiltros.TabIndex = 28;
            this.btnFiltros.Text = "Opciones de filtro";
            this.btnFiltros.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFiltros.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panelFecha1);
            this.groupBox1.Controls.Add(this.panelFecha2);
            this.groupBox1.Location = new System.Drawing.Point(7, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(790, 84);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Búsqueda";
            // 
            // btnOtrasOpciones
            // 
            this.btnOtrasOpciones.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOtrasOpciones.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnOtrasOpciones.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnOtrasOpciones.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnOtrasOpciones.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOtrasOpciones.Image = ((System.Drawing.Image)(resources.GetObject("btnOtrasOpciones.Image")));
            this.btnOtrasOpciones.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnOtrasOpciones.Location = new System.Drawing.Point(361, 102);
            this.btnOtrasOpciones.Name = "btnOtrasOpciones";
            this.btnOtrasOpciones.Size = new System.Drawing.Size(111, 51);
            this.btnOtrasOpciones.TabIndex = 30;
            this.btnOtrasOpciones.Text = "Otras \r\nopciones";
            this.btnOtrasOpciones.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOtrasOpciones.UseVisualStyleBackColor = true;
            // 
            // gbOpciones
            // 
            this.gbOpciones.Controls.Add(this.btnEliminarCuentas);
            this.gbOpciones.Controls.Add(this.chkEliminarCuentas);
            this.gbOpciones.Location = new System.Drawing.Point(479, 99);
            this.gbOpciones.Name = "gbOpciones";
            this.gbOpciones.Size = new System.Drawing.Size(318, 54);
            this.gbOpciones.TabIndex = 31;
            this.gbOpciones.TabStop = false;
            this.gbOpciones.Text = "Eliminación de cuentas";
            this.gbOpciones.Visible = false;
            // 
            // btnEliminarCuentas
            // 
            this.btnEliminarCuentas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(112)))), ((int)(((byte)(88)))));
            this.btnEliminarCuentas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEliminarCuentas.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(112)))), ((int)(((byte)(88)))));
            this.btnEliminarCuentas.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(155)))), ((int)(((byte)(137)))));
            this.btnEliminarCuentas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(253)))), ((int)(((byte)(155)))), ((int)(((byte)(137)))));
            this.btnEliminarCuentas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarCuentas.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminarCuentas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnEliminarCuentas.Image = ((System.Drawing.Image)(resources.GetObject("btnEliminarCuentas.Image")));
            this.btnEliminarCuentas.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnEliminarCuentas.Location = new System.Drawing.Point(160, 13);
            this.btnEliminarCuentas.Name = "btnEliminarCuentas";
            this.btnEliminarCuentas.Size = new System.Drawing.Size(152, 37);
            this.btnEliminarCuentas.TabIndex = 32;
            this.btnEliminarCuentas.Text = "Eliminar cuentas";
            this.btnEliminarCuentas.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEliminarCuentas.UseVisualStyleBackColor = false;
            this.btnEliminarCuentas.Visible = false;
            // 
            // chkEliminarCuentas
            // 
            this.chkEliminarCuentas.AutoSize = true;
            this.chkEliminarCuentas.Location = new System.Drawing.Point(7, 25);
            this.chkEliminarCuentas.Name = "chkEliminarCuentas";
            this.chkEliminarCuentas.Size = new System.Drawing.Size(121, 21);
            this.chkEliminarCuentas.TabIndex = 0;
            this.chkEliminarCuentas.Text = "Eliminar cuentas";
            this.chkEliminarCuentas.UseVisualStyleBackColor = true;
            // 
            // FrmObservarVentas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(809, 430);
            this.Controls.Add(this.gbOpciones);
            this.Controls.Add(this.btnOtrasOpciones);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnFiltros);
            this.Controls.Add(this.btnResumen);
            this.Controls.Add(this.dgvVentas);
            this.Controls.Add(this.btnBuscar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmObservarVentas";
            this.Text = "Observar ventas";
            ((System.ComponentModel.ISupportInitialize)(this.dgvVentas)).EndInit();
            this.panelFecha2.ResumeLayout(false);
            this.panelFecha2.PerformLayout();
            this.panelFecha1.ResumeLayout(false);
            this.panelFecha1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.gbOpciones.ResumeLayout(false);
            this.gbOpciones.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker date1;
        private System.Windows.Forms.DateTimePicker date2;
        public System.Windows.Forms.Button btnBuscar;
        private System.Windows.Forms.DataGridView dgvVentas;
        public System.Windows.Forms.Button btnResumen;
        private System.Windows.Forms.ComboBox ListaHora1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox ListaHora2;
        private System.Windows.Forms.Panel panelFecha2;
        private System.Windows.Forms.Panel panelFecha1;
        public System.Windows.Forms.Button btnFiltros;
        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Button btnOtrasOpciones;
        private System.Windows.Forms.GroupBox gbOpciones;
        private System.Windows.Forms.CheckBox chkEliminarCuentas;
        public System.Windows.Forms.Button btnEliminarCuentas;
    }
}