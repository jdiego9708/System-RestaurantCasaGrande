
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
            this.panelBanner = new System.Windows.Forms.Panel();
            this.lblMesero = new System.Windows.Forms.Label();
            this.lblMistico = new System.Windows.Forms.Label();
            this.customTextBox1 = new CapaPresentacion.CustomTextBox();
            this.btnPlatos = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.panelBanner.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnPlatos);
            this.groupBox1.Controls.Add(this.customTextBox1);
            this.groupBox1.Location = new System.Drawing.Point(7, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 104);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Platos y bebidas";
            // 
            // panelBanner
            // 
            this.panelBanner.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBanner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(193)))), ((int)(((byte)(95)))));
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
            // customTextBox1
            // 
            this.customTextBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.customTextBox1.BackColor = System.Drawing.Color.White;
            this.customTextBox1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customTextBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.customTextBox1.Location = new System.Drawing.Point(6, 25);
            this.customTextBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.customTextBox1.Name = "customTextBox1";
            this.customTextBox1.Size = new System.Drawing.Size(461, 20);
            this.customTextBox1.TabIndex = 0;
            this.customTextBox1.Texto = "Búsqueda";
            this.customTextBox1.Texto_inicial = "Búsqueda";
            this.customTextBox1.Visible_px = true;
            // 
            // btnPlatos
            // 
            this.btnPlatos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlatos.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnPlatos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnPlatos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnPlatos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlatos.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlatos.Location = new System.Drawing.Point(6, 59);
            this.btnPlatos.Name = "btnPlatos";
            this.btnPlatos.Size = new System.Drawing.Size(99, 39);
            this.btnPlatos.TabIndex = 1;
            this.btnPlatos.Text = "Platos";
            this.btnPlatos.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.button1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(111, 59);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 39);
            this.button1.TabIndex = 2;
            this.button1.Text = "Bebidas";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // FrmPedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(870, 584);
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
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panelBanner;
        private System.Windows.Forms.Label lblMesero;
        private System.Windows.Forms.Label lblMistico;
        private CustomTextBox customTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnPlatos;
    }
}