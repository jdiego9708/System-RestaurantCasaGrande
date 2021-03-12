
namespace CapaPresentacion.Formularios.FormsMovimientos
{
    partial class FrmObservarMovimientos
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarMovimientos));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAddIngreso = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnAddEgreso = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.dateBusqueda = new System.Windows.Forms.DateTimePicker();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.txtResultados = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAddIngreso);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(70, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ingreso";
            // 
            // btnAddIngreso
            // 
            this.btnAddIngreso.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddIngreso.BackgroundImage")));
            this.btnAddIngreso.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddIngreso.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddIngreso.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAddIngreso.FlatAppearance.BorderSize = 0;
            this.btnAddIngreso.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnAddIngreso.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAddIngreso.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddIngreso.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddIngreso.Location = new System.Drawing.Point(14, 21);
            this.btnAddIngreso.Name = "btnAddIngreso";
            this.btnAddIngreso.Size = new System.Drawing.Size(40, 40);
            this.btnAddIngreso.TabIndex = 24;
            this.toolTip1.SetToolTip(this.btnAddIngreso, "Registrar un ingreso");
            this.btnAddIngreso.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnAddEgreso);
            this.groupBox2.Location = new System.Drawing.Point(88, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(70, 70);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Egreso";
            // 
            // btnAddEgreso
            // 
            this.btnAddEgreso.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAddEgreso.BackgroundImage")));
            this.btnAddEgreso.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAddEgreso.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddEgreso.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAddEgreso.FlatAppearance.BorderSize = 0;
            this.btnAddEgreso.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnAddEgreso.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAddEgreso.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddEgreso.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddEgreso.Location = new System.Drawing.Point(14, 21);
            this.btnAddEgreso.Name = "btnAddEgreso";
            this.btnAddEgreso.Size = new System.Drawing.Size(40, 40);
            this.btnAddEgreso.TabIndex = 25;
            this.toolTip1.SetToolTip(this.btnAddEgreso, "Registrar un egreso");
            this.btnAddEgreso.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.txtResultados);
            this.groupBox3.Location = new System.Drawing.Point(12, 88);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(455, 295);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Resultados";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dateBusqueda);
            this.groupBox4.Location = new System.Drawing.Point(164, 12);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(303, 70);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Búsqueda";
            // 
            // dateBusqueda
            // 
            this.dateBusqueda.Location = new System.Drawing.Point(6, 27);
            this.dateBusqueda.Name = "dateBusqueda";
            this.dateBusqueda.Size = new System.Drawing.Size(291, 25);
            this.dateBusqueda.TabIndex = 0;
            // 
            // txtResultados
            // 
            this.txtResultados.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtResultados.BackColor = System.Drawing.Color.White;
            this.txtResultados.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtResultados.Font = new System.Drawing.Font("Segoe UI Emoji", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResultados.ForeColor = System.Drawing.Color.Black;
            this.txtResultados.Location = new System.Drawing.Point(6, 24);
            this.txtResultados.Multiline = true;
            this.txtResultados.Name = "txtResultados";
            this.txtResultados.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtResultados.Size = new System.Drawing.Size(443, 265);
            this.txtResultados.TabIndex = 0;
            // 
            // FrmObservarMovimientos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(479, 395);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmObservarMovimientos";
            this.Text = "Movimientos";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnAddIngreso;
        private System.Windows.Forms.Button btnAddEgreso;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DateTimePicker dateBusqueda;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtResultados;
    }
}