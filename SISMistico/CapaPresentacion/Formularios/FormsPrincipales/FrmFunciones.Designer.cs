namespace CapaPresentacion.Formularios.FormsPrincipales
{
    partial class FrmFunciones
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFunciones));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gbReservas = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPeriodoPrueba = new System.Windows.Forms.Button();
            this.btnLicenciaCompleta = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gbReservas.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.gbReservas);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(417, 287);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Funciones disponibles";
            // 
            // gbReservas
            // 
            this.gbReservas.Controls.Add(this.btnLicenciaCompleta);
            this.gbReservas.Controls.Add(this.btnPeriodoPrueba);
            this.gbReservas.Controls.Add(this.label1);
            this.gbReservas.Location = new System.Drawing.Point(6, 24);
            this.gbReservas.Name = "gbReservas";
            this.gbReservas.Size = new System.Drawing.Size(405, 155);
            this.gbReservas.TabIndex = 0;
            this.gbReservas.TabStop = false;
            this.gbReservas.Text = "Reservas";
            // 
            // groupBox2
            // 
            this.groupBox2.Location = new System.Drawing.Point(6, 185);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(405, 85);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Cocina";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 51);
            this.label1.TabIndex = 0;
            this.label1.Text = "Activando el módulo de reservas tiene \r\nacceso a las funciones de reserva de mesa" +
    "s\r\nconsultas de reservas y alertas.";
            // 
            // btnPeriodoPrueba
            // 
            this.btnPeriodoPrueba.BackColor = System.Drawing.Color.Teal;
            this.btnPeriodoPrueba.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPeriodoPrueba.FlatAppearance.BorderSize = 0;
            this.btnPeriodoPrueba.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPeriodoPrueba.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPeriodoPrueba.ForeColor = System.Drawing.Color.White;
            this.btnPeriodoPrueba.Image = ((System.Drawing.Image)(resources.GetObject("btnPeriodoPrueba.Image")));
            this.btnPeriodoPrueba.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnPeriodoPrueba.Location = new System.Drawing.Point(9, 89);
            this.btnPeriodoPrueba.Name = "btnPeriodoPrueba";
            this.btnPeriodoPrueba.Size = new System.Drawing.Size(124, 48);
            this.btnPeriodoPrueba.TabIndex = 4;
            this.btnPeriodoPrueba.Tag = "RESERVAS";
            this.btnPeriodoPrueba.Text = "Activar periodo \r\nde prueba";
            this.btnPeriodoPrueba.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnPeriodoPrueba.UseVisualStyleBackColor = false;
            // 
            // btnLicenciaCompleta
            // 
            this.btnLicenciaCompleta.BackColor = System.Drawing.Color.Teal;
            this.btnLicenciaCompleta.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLicenciaCompleta.FlatAppearance.BorderSize = 0;
            this.btnLicenciaCompleta.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLicenciaCompleta.Font = new System.Drawing.Font("Segoe UI Semibold", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLicenciaCompleta.ForeColor = System.Drawing.Color.White;
            this.btnLicenciaCompleta.Image = ((System.Drawing.Image)(resources.GetObject("btnLicenciaCompleta.Image")));
            this.btnLicenciaCompleta.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLicenciaCompleta.Location = new System.Drawing.Point(261, 89);
            this.btnLicenciaCompleta.Name = "btnLicenciaCompleta";
            this.btnLicenciaCompleta.Size = new System.Drawing.Size(138, 48);
            this.btnLicenciaCompleta.TabIndex = 5;
            this.btnLicenciaCompleta.Tag = "RESERVAS";
            this.btnLicenciaCompleta.Text = "Introducir clave de licencia completa";
            this.btnLicenciaCompleta.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLicenciaCompleta.UseVisualStyleBackColor = false;
            // 
            // FrmFunciones
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(441, 311);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmFunciones";
            this.Text = "Funciones y extras";
            this.groupBox1.ResumeLayout(false);
            this.gbReservas.ResumeLayout(false);
            this.gbReservas.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox gbReservas;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnPeriodoPrueba;
        private System.Windows.Forms.Button btnLicenciaCompleta;
    }
}