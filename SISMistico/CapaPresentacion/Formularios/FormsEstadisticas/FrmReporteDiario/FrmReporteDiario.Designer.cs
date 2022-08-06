
namespace CapaPresentacion.Formularios.FormsEstadisticas
{
    partial class FrmReporteDiario
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmReporteDiario));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chkIngresos = new System.Windows.Forms.CheckBox();
            this.chkDeletePedidos = new System.Windows.Forms.CheckBox();
            this.chkInfoPagos = new System.Windows.Forms.CheckBox();
            this.chkInfoDetalleVentas = new System.Windows.Forms.CheckBox();
            this.chkInfoNomina = new System.Windows.Forms.CheckBox();
            this.chkInfoGastos = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.date2 = new System.Windows.Forms.DateTimePicker();
            this.date1 = new System.Windows.Forms.DateTimePicker();
            this.gbReporte = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnRefresh);
            this.groupBox1.Controls.Add(this.chkIngresos);
            this.groupBox1.Controls.Add(this.chkDeletePedidos);
            this.groupBox1.Controls.Add(this.chkInfoPagos);
            this.groupBox1.Controls.Add(this.chkInfoDetalleVentas);
            this.groupBox1.Controls.Add(this.chkInfoNomina);
            this.groupBox1.Controls.Add(this.chkInfoGastos);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.date2);
            this.groupBox1.Controls.Add(this.date1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1339, 76);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Búsqueda por rango de fechas";
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRefresh.BackgroundImage")));
            this.btnRefresh.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnRefresh.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1272, 24);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(35, 35);
            this.btnRefresh.TabIndex = 30;
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // chkIngresos
            // 
            this.chkIngresos.AutoSize = true;
            this.chkIngresos.Checked = true;
            this.chkIngresos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIngresos.Location = new System.Drawing.Point(790, 45);
            this.chkIngresos.Name = "chkIngresos";
            this.chkIngresos.Size = new System.Drawing.Size(169, 21);
            this.chkIngresos.TabIndex = 32;
            this.chkIngresos.Text = "Información de ingresos";
            this.chkIngresos.UseVisualStyleBackColor = true;
            // 
            // chkDeletePedidos
            // 
            this.chkDeletePedidos.AutoSize = true;
            this.chkDeletePedidos.Location = new System.Drawing.Point(1021, 17);
            this.chkDeletePedidos.Name = "chkDeletePedidos";
            this.chkDeletePedidos.Size = new System.Drawing.Size(245, 21);
            this.chkDeletePedidos.TabIndex = 31;
            this.chkDeletePedidos.Text = "Información de comandas eliminadas";
            this.chkDeletePedidos.UseVisualStyleBackColor = true;
            // 
            // chkInfoPagos
            // 
            this.chkInfoPagos.AutoSize = true;
            this.chkInfoPagos.Checked = true;
            this.chkInfoPagos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInfoPagos.Location = new System.Drawing.Point(1021, 43);
            this.chkInfoPagos.Name = "chkInfoPagos";
            this.chkInfoPagos.Size = new System.Drawing.Size(225, 21);
            this.chkInfoPagos.TabIndex = 5;
            this.chkInfoPagos.Text = "Información de métodos de pago";
            this.chkInfoPagos.UseVisualStyleBackColor = true;
            // 
            // chkInfoDetalleVentas
            // 
            this.chkInfoDetalleVentas.AutoSize = true;
            this.chkInfoDetalleVentas.Location = new System.Drawing.Point(790, 16);
            this.chkInfoDetalleVentas.Name = "chkInfoDetalleVentas";
            this.chkInfoDetalleVentas.Size = new System.Drawing.Size(214, 21);
            this.chkInfoDetalleVentas.TabIndex = 4;
            this.chkInfoDetalleVentas.Text = "Información detallada de ventas";
            this.chkInfoDetalleVentas.UseVisualStyleBackColor = true;
            // 
            // chkInfoNomina
            // 
            this.chkInfoNomina.AutoSize = true;
            this.chkInfoNomina.Checked = true;
            this.chkInfoNomina.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInfoNomina.Location = new System.Drawing.Point(622, 16);
            this.chkInfoNomina.Name = "chkInfoNomina";
            this.chkInfoNomina.Size = new System.Drawing.Size(162, 21);
            this.chkInfoNomina.TabIndex = 2;
            this.chkInfoNomina.Text = "Información de nómina";
            this.chkInfoNomina.UseVisualStyleBackColor = true;
            // 
            // chkInfoGastos
            // 
            this.chkInfoGastos.AutoSize = true;
            this.chkInfoGastos.Checked = true;
            this.chkInfoGastos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInfoGastos.Location = new System.Drawing.Point(622, 46);
            this.chkInfoGastos.Name = "chkInfoGastos";
            this.chkInfoGastos.Size = new System.Drawing.Size(158, 21);
            this.chkInfoGastos.TabIndex = 2;
            this.chkInfoGastos.Text = "Información de gastos";
            this.chkInfoGastos.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(301, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Fecha 2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Fecha 1";
            // 
            // date2
            // 
            this.date2.Location = new System.Drawing.Point(304, 40);
            this.date2.Name = "date2";
            this.date2.Size = new System.Drawing.Size(291, 25);
            this.date2.TabIndex = 1;
            // 
            // date1
            // 
            this.date1.Location = new System.Drawing.Point(7, 40);
            this.date1.Name = "date1";
            this.date1.Size = new System.Drawing.Size(291, 25);
            this.date1.TabIndex = 0;
            // 
            // gbReporte
            // 
            this.gbReporte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbReporte.Location = new System.Drawing.Point(12, 94);
            this.gbReporte.Name = "gbReporte";
            this.gbReporte.Size = new System.Drawing.Size(1339, 450);
            this.gbReporte.TabIndex = 1;
            this.gbReporte.TabStop = false;
            this.gbReporte.Text = "Reporte";
            // 
            // FrmReporteDiario
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1363, 556);
            this.Controls.Add(this.gbReporte);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmReporteDiario";
            this.Text = "Reporte diario";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DateTimePicker date1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker date2;
        private System.Windows.Forms.GroupBox gbReporte;
        private System.Windows.Forms.CheckBox chkInfoNomina;
        private System.Windows.Forms.CheckBox chkInfoGastos;
        private System.Windows.Forms.CheckBox chkInfoDetalleVentas;
        private System.Windows.Forms.CheckBox chkInfoPagos;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.CheckBox chkDeletePedidos;
        private System.Windows.Forms.CheckBox chkIngresos;
    }
}