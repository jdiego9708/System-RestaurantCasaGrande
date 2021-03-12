namespace CapaPresentacion.Formularios.FormsPedido
{
    partial class FrmDetallePedido
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDetallePedido));
            this.label1 = new System.Windows.Forms.Label();
            this.lblIdPedido = new System.Windows.Forms.Label();
            this.dgvPedido = new System.Windows.Forms.DataGridView();
            this.btnEditarPedido = new System.Windows.Forms.Button();
            this.btnInformacionPedido = new System.Windows.Forms.Button();
            this.lblEmpleado = new System.Windows.Forms.Label();
            this.lblMesa = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedido)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pedido número:";
            // 
            // lblIdPedido
            // 
            this.lblIdPedido.AutoSize = true;
            this.lblIdPedido.Location = new System.Drawing.Point(113, 6);
            this.lblIdPedido.Name = "lblIdPedido";
            this.lblIdPedido.Size = new System.Drawing.Size(49, 17);
            this.lblIdPedido.TabIndex = 1;
            this.lblIdPedido.Text = "Pedido";
            // 
            // dgvPedido
            // 
            this.dgvPedido.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPedido.Location = new System.Drawing.Point(12, 66);
            this.dgvPedido.Name = "dgvPedido";
            this.dgvPedido.Size = new System.Drawing.Size(440, 237);
            this.dgvPedido.TabIndex = 2;
            // 
            // btnEditarPedido
            // 
            this.btnEditarPedido.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEditarPedido.BackgroundImage")));
            this.btnEditarPedido.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEditarPedido.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditarPedido.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnEditarPedido.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnEditarPedido.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnEditarPedido.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditarPedido.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditarPedido.Location = new System.Drawing.Point(12, 309);
            this.btnEditarPedido.Name = "btnEditarPedido";
            this.btnEditarPedido.Size = new System.Drawing.Size(58, 55);
            this.btnEditarPedido.TabIndex = 4;
            this.btnEditarPedido.UseVisualStyleBackColor = true;
            // 
            // btnInformacionPedido
            // 
            this.btnInformacionPedido.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnInformacionPedido.BackgroundImage")));
            this.btnInformacionPedido.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnInformacionPedido.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnInformacionPedido.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnInformacionPedido.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnInformacionPedido.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnInformacionPedido.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInformacionPedido.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnInformacionPedido.Location = new System.Drawing.Point(76, 309);
            this.btnInformacionPedido.Name = "btnInformacionPedido";
            this.btnInformacionPedido.Size = new System.Drawing.Size(58, 55);
            this.btnInformacionPedido.TabIndex = 16;
            this.btnInformacionPedido.UseVisualStyleBackColor = true;
            // 
            // lblEmpleado
            // 
            this.lblEmpleado.AutoSize = true;
            this.lblEmpleado.Location = new System.Drawing.Point(11, 46);
            this.lblEmpleado.Name = "lblEmpleado";
            this.lblEmpleado.Size = new System.Drawing.Size(67, 17);
            this.lblEmpleado.TabIndex = 17;
            this.lblEmpleado.Text = "Empleado";
            // 
            // lblMesa
            // 
            this.lblMesa.AutoSize = true;
            this.lblMesa.Location = new System.Drawing.Point(131, 26);
            this.lblMesa.Name = "lblMesa";
            this.lblMesa.Size = new System.Drawing.Size(40, 17);
            this.lblMesa.TabIndex = 19;
            this.lblMesa.Text = "Mesa";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 17);
            this.label2.TabIndex = 18;
            this.label2.Text = "Mesa seleccionada:";
            // 
            // FrmDetallePedido
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(455, 367);
            this.Controls.Add(this.lblMesa);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblEmpleado);
            this.Controls.Add(this.btnInformacionPedido);
            this.Controls.Add(this.btnEditarPedido);
            this.Controls.Add(this.dgvPedido);
            this.Controls.Add(this.lblIdPedido);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmDetallePedido";
            this.Text = "Detalle del pedido";
            ((System.ComponentModel.ISupportInitialize)(this.dgvPedido)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblIdPedido;
        private System.Windows.Forms.DataGridView dgvPedido;
        private System.Windows.Forms.Button btnEditarPedido;
        private System.Windows.Forms.Button btnInformacionPedido;
        private System.Windows.Forms.Label lblEmpleado;
        private System.Windows.Forms.Label lblMesa;
        private System.Windows.Forms.Label label2;
    }
}