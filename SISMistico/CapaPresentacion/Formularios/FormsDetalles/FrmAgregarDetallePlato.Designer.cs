namespace CapaPresentacion.Formularios.FormsDetalles
{
    partial class FrmAgregarDetallePlato
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAgregarDetallePlato));
            this.txtPlato = new System.Windows.Forms.TextBox();
            this.dgvInsumos = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInsumo = new System.Windows.Forms.TextBox();
            this.txtCantidad = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnQuitarInsumos = new System.Windows.Forms.Button();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.dgvInsumosUsados = new System.Windows.Forms.DataGridView();
            this.lblins = new System.Windows.Forms.Label();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnAyuda = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnQuitarInsumosUsados = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnCancelar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumosUsados)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtPlato
            // 
            this.txtPlato.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtPlato.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtPlato.Location = new System.Drawing.Point(7, 5);
            this.txtPlato.Name = "txtPlato";
            this.txtPlato.ReadOnly = true;
            this.txtPlato.Size = new System.Drawing.Size(447, 25);
            this.txtPlato.TabIndex = 1;
            this.txtPlato.Text = "Seleccione un plato";
            // 
            // dgvInsumos
            // 
            this.dgvInsumos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInsumos.Location = new System.Drawing.Point(460, 110);
            this.dgvInsumos.Name = "dgvInsumos";
            this.dgvInsumos.Size = new System.Drawing.Size(336, 96);
            this.dgvInsumos.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Insumos";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.Location = new System.Drawing.Point(7, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(447, 382);
            this.panel1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(457, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Nombre insumo";
            // 
            // txtInsumo
            // 
            this.txtInsumo.Location = new System.Drawing.Point(460, 25);
            this.txtInsumo.Name = "txtInsumo";
            this.txtInsumo.ReadOnly = true;
            this.txtInsumo.Size = new System.Drawing.Size(309, 25);
            this.txtInsumo.TabIndex = 6;
            // 
            // txtCantidad
            // 
            this.txtCantidad.Location = new System.Drawing.Point(460, 75);
            this.txtCantidad.Name = "txtCantidad";
            this.txtCantidad.Size = new System.Drawing.Size(96, 25);
            this.txtCantidad.TabIndex = 8;
            this.txtCantidad.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(457, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "Cantidad usada (Porciones)";
            // 
            // btnQuitarInsumos
            // 
            this.btnQuitarInsumos.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnQuitarInsumos.BackgroundImage")));
            this.btnQuitarInsumos.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuitarInsumos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuitarInsumos.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnQuitarInsumos.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnQuitarInsumos.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnQuitarInsumos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitarInsumos.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuitarInsumos.Location = new System.Drawing.Point(688, 53);
            this.btnQuitarInsumos.Name = "btnQuitarInsumos";
            this.btnQuitarInsumos.Size = new System.Drawing.Size(51, 46);
            this.btnQuitarInsumos.TabIndex = 15;
            this.toolTip1.SetToolTip(this.btnQuitarInsumos, "Quitar un insumo de la lista");
            this.btnQuitarInsumos.UseVisualStyleBackColor = true;
            // 
            // btnAgregar
            // 
            this.btnAgregar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAgregar.BackgroundImage")));
            this.btnAgregar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAgregar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAgregar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnAgregar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnAgregar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnAgregar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregar.Location = new System.Drawing.Point(631, 53);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(51, 46);
            this.btnAgregar.TabIndex = 14;
            this.toolTip1.SetToolTip(this.btnAgregar, "Agregar el insumo");
            this.btnAgregar.UseVisualStyleBackColor = true;
            // 
            // dgvInsumosUsados
            // 
            this.dgvInsumosUsados.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvInsumosUsados.Location = new System.Drawing.Point(460, 249);
            this.dgvInsumosUsados.Name = "dgvInsumosUsados";
            this.dgvInsumosUsados.Size = new System.Drawing.Size(336, 136);
            this.dgvInsumosUsados.TabIndex = 16;
            // 
            // lblins
            // 
            this.lblins.AutoSize = true;
            this.lblins.Location = new System.Drawing.Point(594, 229);
            this.lblins.Name = "lblins";
            this.lblins.Size = new System.Drawing.Size(102, 17);
            this.lblins.TabIndex = 17;
            this.lblins.Text = "Insumos usados";
            // 
            // btnEditar
            // 
            this.btnEditar.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnEditar.BackgroundImage")));
            this.btnEditar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnEditar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnEditar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnEditar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnEditar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditar.Location = new System.Drawing.Point(460, 212);
            this.btnEditar.Name = "btnEditar";
            this.btnEditar.Size = new System.Drawing.Size(40, 34);
            this.btnEditar.TabIndex = 18;
            this.toolTip1.SetToolTip(this.btnEditar, "Editar el insumo seleccionado");
            this.btnEditar.UseVisualStyleBackColor = true;
            // 
            // btnAyuda
            // 
            this.btnAyuda.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnAyuda.BackgroundImage")));
            this.btnAyuda.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnAyuda.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAyuda.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnAyuda.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnAyuda.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnAyuda.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAyuda.Location = new System.Drawing.Point(562, 71);
            this.btnAyuda.Name = "btnAyuda";
            this.btnAyuda.Size = new System.Drawing.Size(30, 30);
            this.btnAyuda.TabIndex = 19;
            this.btnAyuda.UseVisualStyleBackColor = true;
            // 
            // btnQuitarInsumosUsados
            // 
            this.btnQuitarInsumosUsados.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnQuitarInsumosUsados.BackgroundImage")));
            this.btnQuitarInsumosUsados.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnQuitarInsumosUsados.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuitarInsumosUsados.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnQuitarInsumosUsados.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Silver;
            this.btnQuitarInsumosUsados.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnQuitarInsumosUsados.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnQuitarInsumosUsados.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnQuitarInsumosUsados.Location = new System.Drawing.Point(506, 212);
            this.btnQuitarInsumosUsados.Name = "btnQuitarInsumosUsados";
            this.btnQuitarInsumosUsados.Size = new System.Drawing.Size(40, 34);
            this.btnQuitarInsumosUsados.TabIndex = 30;
            this.toolTip1.SetToolTip(this.btnQuitarInsumosUsados, "Quitar un insumo de la lista");
            this.btnQuitarInsumosUsados.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // btnCancelar
            // 
            this.btnCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnCancelar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.ForeColor = System.Drawing.Color.Gray;
            this.btnCancelar.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelar.Image")));
            this.btnCancelar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancelar.Location = new System.Drawing.Point(684, 391);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(112, 44);
            this.btnCancelar.TabIndex = 29;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancelar.UseVisualStyleBackColor = true;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGuardar.FlatAppearance.BorderColor = System.Drawing.Color.Silver;
            this.btnGuardar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnGuardar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGuardar.ForeColor = System.Drawing.Color.Gray;
            this.btnGuardar.Image = ((System.Drawing.Image)(resources.GetObject("btnGuardar.Image")));
            this.btnGuardar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnGuardar.Location = new System.Drawing.Point(460, 391);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(112, 44);
            this.btnGuardar.TabIndex = 28;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGuardar.UseVisualStyleBackColor = true;
            // 
            // FrmAgregarDetallePlato
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(808, 441);
            this.Controls.Add(this.btnQuitarInsumosUsados);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnAyuda);
            this.Controls.Add(this.btnEditar);
            this.Controls.Add(this.lblins);
            this.Controls.Add(this.dgvInsumosUsados);
            this.Controls.Add(this.btnQuitarInsumos);
            this.Controls.Add(this.btnAgregar);
            this.Controls.Add(this.txtCantidad);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtInsumo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvInsumos);
            this.Controls.Add(this.txtPlato);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FrmAgregarDetallePlato";
            this.Text = "Detalle plato";
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvInsumosUsados)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtPlato;
        private System.Windows.Forms.DataGridView dgvInsumos;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInsumo;
        private System.Windows.Forms.TextBox txtCantidad;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnQuitarInsumos;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.DataGridView dgvInsumosUsados;
        private System.Windows.Forms.Label lblins;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnAyuda;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnQuitarInsumosUsados;
    }
}