namespace CapaPresentacion.Formularios.FormsPedido.Bebidas
{
    partial class FrmPedidoBebidas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPedidoBebidas));
            this.panelPlatos = new System.Windows.Forms.Panel();
            this.txtBusqueda = new CapaPresentacion.CustomTextBox();
            this.lblResultados = new System.Windows.Forms.Label();
            this.dgvBebidas = new System.Windows.Forms.DataGridView();
            this.panelTipoBebidas = new System.Windows.Forms.Panel();
            this.panelPlatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBebidas)).BeginInit();
            this.SuspendLayout();
            // 
            // panelPlatos
            // 
            this.panelPlatos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPlatos.Controls.Add(this.txtBusqueda);
            this.panelPlatos.Controls.Add(this.lblResultados);
            this.panelPlatos.Controls.Add(this.dgvBebidas);
            this.panelPlatos.Location = new System.Drawing.Point(122, 0);
            this.panelPlatos.Name = "panelPlatos";
            this.panelPlatos.Size = new System.Drawing.Size(546, 461);
            this.panelPlatos.TabIndex = 5;
            // 
            // txtBusqueda
            // 
            this.txtBusqueda.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtBusqueda.BackColor = System.Drawing.Color.White;
            this.txtBusqueda.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBusqueda.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtBusqueda.Location = new System.Drawing.Point(3, 4);
            this.txtBusqueda.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBusqueda.Name = "txtBusqueda";
            this.txtBusqueda.Size = new System.Drawing.Size(320, 20);
            this.txtBusqueda.TabIndex = 2;
            this.txtBusqueda.Texto = "";
            this.txtBusqueda.Texto_inicial = "Ingrese el texto a buscar";
            this.txtBusqueda.Visible_px = false;
            // 
            // lblResultados
            // 
            this.lblResultados.AutoSize = true;
            this.lblResultados.Location = new System.Drawing.Point(325, 3);
            this.lblResultados.Name = "lblResultados";
            this.lblResultados.Size = new System.Drawing.Size(72, 17);
            this.lblResultados.TabIndex = 1;
            this.lblResultados.Text = "Resultados";
            // 
            // dgvBebidas
            // 
            this.dgvBebidas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvBebidas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBebidas.Location = new System.Drawing.Point(0, 27);
            this.dgvBebidas.Name = "dgvBebidas";
            this.dgvBebidas.Size = new System.Drawing.Size(543, 435);
            this.dgvBebidas.TabIndex = 0;
            // 
            // panelTipoBebidas
            // 
            this.panelTipoBebidas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelTipoBebidas.AutoScroll = true;
            this.panelTipoBebidas.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.panelTipoBebidas.Location = new System.Drawing.Point(0, 27);
            this.panelTipoBebidas.Name = "panelTipoBebidas";
            this.panelTipoBebidas.Size = new System.Drawing.Size(121, 435);
            this.panelTipoBebidas.TabIndex = 4;
            // 
            // FrmPedidoBebidas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(669, 463);
            this.Controls.Add(this.panelPlatos);
            this.Controls.Add(this.panelTipoBebidas);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmPedidoBebidas";
            this.Text = "Pedido bebidas";
            this.panelPlatos.ResumeLayout(false);
            this.panelPlatos.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBebidas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelPlatos;
        private System.Windows.Forms.Label lblResultados;
        private System.Windows.Forms.DataGridView dgvBebidas;
        private System.Windows.Forms.Panel panelTipoBebidas;
        private CapaPresentacion.CustomTextBox txtBusqueda;
    }
}