
namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmDomicilios
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDomicilios));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gbResultados = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rdEnCurso = new System.Windows.Forms.RadioButton();
            this.rdTerminados = new System.Windows.Forms.RadioButton();
            this.panelDomicilios = new CapaPresentacion.Controles.CustomGridPanel();
            this.btnAddDomicilio = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gbResultados.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rdTerminados);
            this.groupBox1.Controls.Add(this.rdEnCurso);
            this.groupBox1.Location = new System.Drawing.Point(142, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(194, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Búsqueda";
            // 
            // gbResultados
            // 
            this.gbResultados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbResultados.Controls.Add(this.panelDomicilios);
            this.gbResultados.Location = new System.Drawing.Point(12, 92);
            this.gbResultados.Name = "gbResultados";
            this.gbResultados.Size = new System.Drawing.Size(845, 476);
            this.gbResultados.TabIndex = 1;
            this.gbResultados.TabStop = false;
            this.gbResultados.Text = "Resultados";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnAddDomicilio);
            this.groupBox3.Location = new System.Drawing.Point(15, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(121, 70);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Nuevo domicilio";
            // 
            // rdEnCurso
            // 
            this.rdEnCurso.AutoSize = true;
            this.rdEnCurso.Checked = true;
            this.rdEnCurso.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdEnCurso.Location = new System.Drawing.Point(9, 29);
            this.rdEnCurso.Name = "rdEnCurso";
            this.rdEnCurso.Size = new System.Drawing.Size(92, 25);
            this.rdEnCurso.TabIndex = 0;
            this.rdEnCurso.TabStop = true;
            this.rdEnCurso.Tag = "PENDIENTE";
            this.rdEnCurso.Text = "En curso";
            this.rdEnCurso.UseVisualStyleBackColor = true;
            // 
            // rdTerminados
            // 
            this.rdTerminados.AutoSize = true;
            this.rdTerminados.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdTerminados.Location = new System.Drawing.Point(107, 29);
            this.rdTerminados.Name = "rdTerminados";
            this.rdTerminados.Size = new System.Drawing.Size(69, 25);
            this.rdTerminados.TabIndex = 1;
            this.rdTerminados.Tag = "TERMINADO";
            this.rdTerminados.Text = "Otros";
            this.rdTerminados.UseVisualStyleBackColor = true;
            // 
            // panelDomicilios
            // 
            this.panelDomicilios.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelDomicilios.AutoScroll = true;
            this.panelDomicilios.Location = new System.Drawing.Point(3, 21);
            this.panelDomicilios.Name = "panelDomicilios";
            this.panelDomicilios.PageSize = 10;
            this.panelDomicilios.Size = new System.Drawing.Size(836, 449);
            this.panelDomicilios.TabIndex = 0;
            // 
            // btnAddDomicilio
            // 
            this.btnAddDomicilio.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddDomicilio.BackgroundImage")));
            this.btnAddDomicilio.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddDomicilio.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddDomicilio.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAddDomicilio.FlatAppearance.BorderSize = 0;
            this.btnAddDomicilio.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnAddDomicilio.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAddDomicilio.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddDomicilio.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddDomicilio.Location = new System.Drawing.Point(37, 21);
            this.btnAddDomicilio.Name = "btnAddDomicilio";
            this.btnAddDomicilio.Size = new System.Drawing.Size(40, 40);
            this.btnAddDomicilio.TabIndex = 24;
            this.btnAddDomicilio.UseVisualStyleBackColor = true;
            // 
            // FrmDomicilios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(869, 580);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.gbResultados);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FrmDomicilios";
            this.Text = "Domicilios";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbResultados.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbResultados;
        private CapaPresentacion.Controles.CustomGridPanel panelDomicilios;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnAddDomicilio;
        private System.Windows.Forms.RadioButton rdEnCurso;
        private System.Windows.Forms.RadioButton rdTerminados;
    }
}