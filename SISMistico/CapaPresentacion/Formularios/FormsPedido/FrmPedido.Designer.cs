
namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmPedido
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPedido));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBusqueda = new CapaPresentacion.CustomTextBox();
            this.btnBebidas = new System.Windows.Forms.Button();
            this.btnPlatos = new System.Windows.Forms.Button();
            this.panelBanner = new System.Windows.Forms.Panel();
            this.lblMesero = new System.Windows.Forms.Label();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panelTipo = new CapaPresentacion.Controles.CustomGridPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panelResultados = new CapaPresentacion.Controles.CustomGridPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.panelPedido = new CapaPresentacion.Controles.CustomGridPanel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.gbNumClientes = new System.Windows.Forms.GroupBox();
            this.numericClientes = new System.Windows.Forms.NumericUpDown();
            this.chkPrintComandas = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtInfoPedido = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.numericComandas = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            this.panelBanner.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.gbNumClientes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericClientes)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericComandas)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBusqueda);
            this.groupBox1.Controls.Add(this.btnBebidas);
            this.groupBox1.Controls.Add(this.btnPlatos);
            this.groupBox1.Location = new System.Drawing.Point(7, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(735, 84);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Platos y bebidas";
            // 
            // txtBusqueda
            // 
            this.txtBusqueda.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtBusqueda.BackColor = System.Drawing.Color.White;
            this.txtBusqueda.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBusqueda.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtBusqueda.Location = new System.Drawing.Point(216, 38);
            this.txtBusqueda.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBusqueda.Name = "txtBusqueda";
            this.txtBusqueda.Size = new System.Drawing.Size(513, 20);
            this.txtBusqueda.TabIndex = 3;
            this.txtBusqueda.Texto = "Búsqueda";
            this.txtBusqueda.Texto_inicial = "Búsqueda";
            this.txtBusqueda.Visible_px = true;
            // 
            // btnBebidas
            // 
            this.btnBebidas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBebidas.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnBebidas.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnBebidas.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnBebidas.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBebidas.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBebidas.Location = new System.Drawing.Point(111, 28);
            this.btnBebidas.Name = "btnBebidas";
            this.btnBebidas.Size = new System.Drawing.Size(99, 39);
            this.btnBebidas.TabIndex = 2;
            this.btnBebidas.Text = "Bebidas";
            this.btnBebidas.UseVisualStyleBackColor = true;
            // 
            // btnPlatos
            // 
            this.btnPlatos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlatos.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnPlatos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnPlatos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnPlatos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlatos.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlatos.Location = new System.Drawing.Point(6, 28);
            this.btnPlatos.Name = "btnPlatos";
            this.btnPlatos.Size = new System.Drawing.Size(99, 39);
            this.btnPlatos.TabIndex = 1;
            this.btnPlatos.Text = "Platos";
            this.btnPlatos.UseVisualStyleBackColor = true;
            // 
            // panelBanner
            // 
            this.panelBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBanner.BackColor = System.Drawing.Color.SteelBlue;
            this.panelBanner.Controls.Add(this.lblMesero);
            this.panelBanner.Controls.Add(this.lblTitulo);
            this.panelBanner.Location = new System.Drawing.Point(0, 0);
            this.panelBanner.Name = "panelBanner";
            this.panelBanner.Size = new System.Drawing.Size(1200, 54);
            this.panelBanner.TabIndex = 5;
            // 
            // lblMesero
            // 
            this.lblMesero.AutoSize = true;
            this.lblMesero.Font = new System.Drawing.Font("Segoe UI", 11.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMesero.ForeColor = System.Drawing.Color.White;
            this.lblMesero.Location = new System.Drawing.Point(3, 30);
            this.lblMesero.Name = "lblMesero";
            this.lblMesero.Size = new System.Drawing.Size(60, 20);
            this.lblMesero.TabIndex = 12;
            this.lblMesero.Text = "Mesero";
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(3, 4);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(354, 25);
            this.lblTitulo.TabIndex = 11;
            this.lblTitulo.Text = "Realizar un nuevo pedido para la mesa";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.panelTipo);
            this.groupBox2.Location = new System.Drawing.Point(8, 150);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(192, 422);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tipos";
            // 
            // panelTipo
            // 
            this.panelTipo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTipo.AutoScroll = true;
            this.panelTipo.Location = new System.Drawing.Point(9, 24);
            this.panelTipo.Name = "panelTipo";
            this.panelTipo.PageSize = 10;
            this.panelTipo.Size = new System.Drawing.Size(177, 392);
            this.panelTipo.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.panelResultados);
            this.groupBox3.Location = new System.Drawing.Point(206, 150);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(536, 422);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Búsqueda";
            // 
            // panelResultados
            // 
            this.panelResultados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelResultados.AutoScroll = true;
            this.panelResultados.Location = new System.Drawing.Point(3, 21);
            this.panelResultados.Name = "panelResultados";
            this.panelResultados.PageSize = 10;
            this.panelResultados.Size = new System.Drawing.Size(527, 395);
            this.panelResultados.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.panelPedido);
            this.groupBox4.Location = new System.Drawing.Point(748, 150);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(438, 422);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Pedido";
            // 
            // panelPedido
            // 
            this.panelPedido.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPedido.AutoScroll = true;
            this.panelPedido.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panelPedido.BackgroundImage")));
            this.panelPedido.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panelPedido.Location = new System.Drawing.Point(3, 21);
            this.panelPedido.Name = "panelPedido";
            this.panelPedido.PageSize = 10;
            this.panelPedido.Size = new System.Drawing.Size(429, 395);
            this.panelPedido.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.groupBox6);
            this.groupBox5.Controls.Add(this.gbNumClientes);
            this.groupBox5.Controls.Add(this.chkPrintComandas);
            this.groupBox5.Controls.Add(this.btnSave);
            this.groupBox5.Controls.Add(this.txtInfoPedido);
            this.groupBox5.Location = new System.Drawing.Point(748, 60);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(438, 84);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Información";
            // 
            // gbNumClientes
            // 
            this.gbNumClientes.Controls.Add(this.numericClientes);
            this.gbNumClientes.Location = new System.Drawing.Point(265, 13);
            this.gbNumClientes.Name = "gbNumClientes";
            this.gbNumClientes.Size = new System.Drawing.Size(81, 65);
            this.gbNumClientes.TabIndex = 9;
            this.gbNumClientes.TabStop = false;
            this.gbNumClientes.Text = "# Clientes";
            // 
            // numericClientes
            // 
            this.numericClientes.BackColor = System.Drawing.Color.White;
            this.numericClientes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericClientes.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericClientes.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.numericClientes.Location = new System.Drawing.Point(6, 25);
            this.numericClientes.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numericClientes.Name = "numericClientes";
            this.numericClientes.Size = new System.Drawing.Size(69, 28);
            this.numericClientes.TabIndex = 29;
            this.numericClientes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // chkPrintComandas
            // 
            this.chkPrintComandas.AutoSize = true;
            this.chkPrintComandas.Checked = true;
            this.chkPrintComandas.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPrintComandas.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkPrintComandas.Location = new System.Drawing.Point(70, 32);
            this.chkPrintComandas.Name = "chkPrintComandas";
            this.chkPrintComandas.Size = new System.Drawing.Size(87, 38);
            this.chkPrintComandas.TabIndex = 28;
            this.chkPrintComandas.Text = "Imprimir\r\ncomandas";
            this.chkPrintComandas.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSave.BackgroundImage")));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(8, 25);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(50, 50);
            this.btnSave.TabIndex = 27;
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // txtInfoPedido
            // 
            this.txtInfoPedido.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoPedido.BackColor = System.Drawing.Color.White;
            this.txtInfoPedido.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInfoPedido.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInfoPedido.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtInfoPedido.Location = new System.Drawing.Point(359, 22);
            this.txtInfoPedido.Multiline = true;
            this.txtInfoPedido.Name = "txtInfoPedido";
            this.txtInfoPedido.ReadOnly = true;
            this.txtInfoPedido.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInfoPedido.Size = new System.Drawing.Size(73, 54);
            this.txtInfoPedido.TabIndex = 0;
            this.txtInfoPedido.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.numericComandas);
            this.groupBox6.Location = new System.Drawing.Point(156, 13);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(103, 65);
            this.groupBox6.TabIndex = 30;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "# Comandas";
            // 
            // numericComandas
            // 
            this.numericComandas.BackColor = System.Drawing.Color.White;
            this.numericComandas.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericComandas.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numericComandas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.numericComandas.Location = new System.Drawing.Point(17, 25);
            this.numericComandas.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericComandas.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericComandas.Name = "numericComandas";
            this.numericComandas.Size = new System.Drawing.Size(69, 28);
            this.numericComandas.TabIndex = 29;
            this.numericComandas.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericComandas.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // FrmPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1198, 584);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.panelBanner);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmPedido";
            this.Text = "Pedido";
            this.groupBox1.ResumeLayout(false);
            this.panelBanner.ResumeLayout(false);
            this.panelBanner.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.gbNumClientes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericClientes)).EndInit();
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericComandas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelBanner;
        private System.Windows.Forms.Label lblMesero;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Button btnBebidas;
        private System.Windows.Forms.Button btnPlatos;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtInfoPedido;
        private CapaPresentacion.Controles.CustomGridPanel panelTipo;
        private CapaPresentacion.Controles.CustomGridPanel panelResultados;
        private CapaPresentacion.Controles.CustomGridPanel panelPedido;
        private CustomTextBox txtBusqueda;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.CheckBox chkPrintComandas;
        private System.Windows.Forms.GroupBox gbNumClientes;
        private System.Windows.Forms.NumericUpDown numericClientes;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.NumericUpDown numericComandas;
    }
}