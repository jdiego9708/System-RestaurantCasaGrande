﻿namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmObservarMesas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarMesas));
            this.panelMesas = new System.Windows.Forms.Panel();
            this.txtNumeroMesas = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelDetallePedido = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnCargar = new System.Windows.Forms.Button();
            this.btnDomicilio = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelMesas
            // 
            this.panelMesas.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panelMesas.AutoScroll = true;
            this.panelMesas.Location = new System.Drawing.Point(12, 44);
            this.panelMesas.Name = "panelMesas";
            this.panelMesas.Size = new System.Drawing.Size(733, 579);
            this.panelMesas.TabIndex = 0;
            // 
            // txtNumeroMesas
            // 
            this.txtNumeroMesas.Location = new System.Drawing.Point(131, 10);
            this.txtNumeroMesas.Name = "txtNumeroMesas";
            this.txtNumeroMesas.Size = new System.Drawing.Size(83, 25);
            this.txtNumeroMesas.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Número de mesas";
            // 
            // panelDetallePedido
            // 
            this.panelDetallePedido.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDetallePedido.Location = new System.Drawing.Point(751, 44);
            this.panelDetallePedido.Name = "panelDetallePedido";
            this.panelDetallePedido.Size = new System.Drawing.Size(325, 579);
            this.panelDetallePedido.TabIndex = 5;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightGreen;
            this.panel1.Location = new System.Drawing.Point(330, 9);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(29, 26);
            this.panel1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(361, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "DISPONIBLE";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(475, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "OCUPADA";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Yellow;
            this.panel2.Location = new System.Drawing.Point(444, 9);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(29, 26);
            this.panel2.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(578, 13);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "SALIENDO";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DarkOrange;
            this.panel3.Location = new System.Drawing.Point(547, 9);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(29, 26);
            this.panel3.TabIndex = 10;
            // 
            // btnCargar
            // 
            this.btnCargar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCargar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnCargar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnCargar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnCargar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCargar.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCargar.Location = new System.Drawing.Point(221, 8);
            this.btnCargar.Name = "btnCargar";
            this.btnCargar.Size = new System.Drawing.Size(102, 29);
            this.btnCargar.TabIndex = 16;
            this.btnCargar.Text = "Cargar mesas";
            this.btnCargar.UseVisualStyleBackColor = true;
            // 
            // btnDomicilio
            // 
            this.btnDomicilio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDomicilio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDomicilio.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnDomicilio.FlatAppearance.BorderSize = 0;
            this.btnDomicilio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnDomicilio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnDomicilio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDomicilio.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDomicilio.Image = ((System.Drawing.Image)(resources.GetObject("btnDomicilio.Image")));
            this.btnDomicilio.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDomicilio.Location = new System.Drawing.Point(652, 5);
            this.btnDomicilio.Name = "btnDomicilio";
            this.btnDomicilio.Size = new System.Drawing.Size(200, 35);
            this.btnDomicilio.TabIndex = 32;
            this.btnDomicilio.Text = "Domicilio";
            this.btnDomicilio.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDomicilio.UseVisualStyleBackColor = true;
            // 
            // FrmObservarMesas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1088, 635);
            this.Controls.Add(this.btnDomicilio);
            this.Controls.Add(this.btnCargar);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelDetallePedido);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtNumeroMesas);
            this.Controls.Add(this.panelMesas);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmObservarMesas";
            this.Text = "Nuevo pedido";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelMesas;
        private System.Windows.Forms.TextBox txtNumeroMesas;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelDetallePedido;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.Button btnCargar;
        private System.Windows.Forms.Button btnDomicilio;
    }
}