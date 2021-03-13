
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
            this.btnBebidas = new System.Windows.Forms.Button();
            this.btnPlatos = new System.Windows.Forms.Button();
            this.txtBusqueda = new CapaPresentacion.CustomTextBox();
            this.panelBanner = new System.Windows.Forms.Panel();
            this.lblMesero = new System.Windows.Forms.Label();
            this.lblMistico = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.panelTipos = new CapaPresentacion.Controles.CustomGridPanel();
            this.panelBusqueda = new CapaPresentacion.Controles.CustomGridPanel();
            this.panelPedidos = new CapaPresentacion.Controles.CustomGridPanel();
            this.txtInfoPedido = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.panelBanner.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnBebidas);
            this.groupBox1.Controls.Add(this.btnPlatos);
            this.groupBox1.Controls.Add(this.txtBusqueda);
            this.groupBox1.Location = new System.Drawing.Point(7, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 84);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Platos y bebidas";
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
            // txtBusqueda
            // 
            this.txtBusqueda.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.txtBusqueda.BackColor = System.Drawing.Color.White;
            this.txtBusqueda.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBusqueda.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtBusqueda.Location = new System.Drawing.Point(216, 34);
            this.txtBusqueda.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtBusqueda.Name = "txtBusqueda";
            this.txtBusqueda.Size = new System.Drawing.Size(251, 20);
            this.txtBusqueda.TabIndex = 0;
            this.txtBusqueda.Texto = "Búsqueda";
            this.txtBusqueda.Texto_inicial = "Búsqueda";
            this.txtBusqueda.Visible_px = true;
            // 
            // panelBanner
            // 
            this.panelBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBanner.BackColor = System.Drawing.Color.SteelBlue;
            this.panelBanner.Controls.Add(this.lblMesero);
            this.panelBanner.Controls.Add(this.lblMistico);
            this.panelBanner.Location = new System.Drawing.Point(0, 0);
            this.panelBanner.Name = "panelBanner";
            this.panelBanner.Size = new System.Drawing.Size(872, 54);
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
            // lblMistico
            // 
            this.lblMistico.AutoSize = true;
            this.lblMistico.Font = new System.Drawing.Font("Segoe UI", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMistico.ForeColor = System.Drawing.Color.White;
            this.lblMistico.Location = new System.Drawing.Point(3, 4);
            this.lblMistico.Name = "lblMistico";
            this.lblMistico.Size = new System.Drawing.Size(354, 25);
            this.lblMistico.TabIndex = 11;
            this.lblMistico.Text = "Realizar un nuevo pedido para la mesa";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox2.Controls.Add(this.panelTipos);
            this.groupBox2.Location = new System.Drawing.Point(8, 150);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(166, 422);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tipos";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox3.Controls.Add(this.panelBusqueda);
            this.groupBox3.Location = new System.Drawing.Point(180, 150);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(300, 422);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Búsqueda";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.panelPedidos);
            this.groupBox4.Location = new System.Drawing.Point(486, 150);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(372, 422);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Pedido";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.txtInfoPedido);
            this.groupBox5.Location = new System.Drawing.Point(486, 60);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(372, 84);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Información";
            // 
            // panelTipos
            // 
            this.panelTipos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTipos.AutoScroll = true;
            this.panelTipos.Location = new System.Drawing.Point(5, 24);
            this.panelTipos.Name = "panelTipos";
            this.panelTipos.PageSize = 10;
            this.panelTipos.Size = new System.Drawing.Size(155, 392);
            this.panelTipos.TabIndex = 9;
            // 
            // panelBusqueda
            // 
            this.panelBusqueda.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBusqueda.AutoScroll = true;
            this.panelBusqueda.Location = new System.Drawing.Point(6, 24);
            this.panelBusqueda.Name = "panelBusqueda";
            this.panelBusqueda.PageSize = 10;
            this.panelBusqueda.Size = new System.Drawing.Size(288, 392);
            this.panelBusqueda.TabIndex = 10;
            // 
            // panelPedidos
            // 
            this.panelPedidos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPedidos.AutoScroll = true;
            this.panelPedidos.Location = new System.Drawing.Point(6, 24);
            this.panelPedidos.Name = "panelPedidos";
            this.panelPedidos.PageSize = 10;
            this.panelPedidos.Size = new System.Drawing.Size(360, 392);
            this.panelPedidos.TabIndex = 11;
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
            this.txtInfoPedido.Location = new System.Drawing.Point(6, 24);
            this.txtInfoPedido.Multiline = true;
            this.txtInfoPedido.Name = "txtInfoPedido";
            this.txtInfoPedido.ReadOnly = true;
            this.txtInfoPedido.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInfoPedido.Size = new System.Drawing.Size(360, 54);
            this.txtInfoPedido.TabIndex = 0;
            // 
            // FrmPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(870, 584);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelBanner;
        private System.Windows.Forms.Label lblMesero;
        private System.Windows.Forms.Label lblMistico;
        private CustomTextBox txtBusqueda;
        private System.Windows.Forms.Button btnBebidas;
        private System.Windows.Forms.Button btnPlatos;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private CapaPresentacion.Controles.CustomGridPanel panelTipos;
        private CapaPresentacion.Controles.CustomGridPanel panelBusqueda;
        private CapaPresentacion.Controles.CustomGridPanel panelPedidos;
        private System.Windows.Forms.TextBox txtInfoPedido;
    }
}