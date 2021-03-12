namespace CapaPresentacion.Formularios.FormsReservas
{
    partial class FrmObservarReservas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmObservarReservas));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rdMesa = new System.Windows.Forms.RadioButton();
            this.btnNuevaReserva = new System.Windows.Forms.Button();
            this.date1 = new System.Windows.Forms.DateTimePicker();
            this.rdFecha = new System.Windows.Forms.RadioButton();
            this.rdCliente = new System.Windows.Forms.RadioButton();
            this.lblResultados = new System.Windows.Forms.Label();
            this.gbEstado = new System.Windows.Forms.GroupBox();
            this.rdCancelada = new System.Windows.Forms.RadioButton();
            this.rdTerminada = new System.Windows.Forms.RadioButton();
            this.rdPendiente = new System.Windows.Forms.RadioButton();
            this.panelReservas = new CapaPresentacion.Controles.CustomGridPanel();
            this.btnSeleccionar = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.gbEstado.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnSeleccionar);
            this.groupBox1.Controls.Add(this.rdMesa);
            this.groupBox1.Controls.Add(this.btnNuevaReserva);
            this.groupBox1.Controls.Add(this.date1);
            this.groupBox1.Controls.Add(this.rdFecha);
            this.groupBox1.Controls.Add(this.rdCliente);
            this.groupBox1.Location = new System.Drawing.Point(242, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(669, 142);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Busqueda de reservas";
            // 
            // rdMesa
            // 
            this.rdMesa.AutoSize = true;
            this.rdMesa.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdMesa.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdMesa.Location = new System.Drawing.Point(218, 23);
            this.rdMesa.Name = "rdMesa";
            this.rdMesa.Size = new System.Drawing.Size(68, 27);
            this.rdMesa.TabIndex = 5;
            this.rdMesa.TabStop = true;
            this.rdMesa.Text = "Mesa";
            this.rdMesa.UseVisualStyleBackColor = true;
            // 
            // btnNuevaReserva
            // 
            this.btnNuevaReserva.BackColor = System.Drawing.Color.White;
            this.btnNuevaReserva.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNuevaReserva.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnNuevaReserva.FlatAppearance.BorderSize = 0;
            this.btnNuevaReserva.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnNuevaReserva.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnNuevaReserva.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNuevaReserva.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNuevaReserva.Image = ((System.Drawing.Image)(resources.GetObject("btnNuevaReserva.Image")));
            this.btnNuevaReserva.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnNuevaReserva.Location = new System.Drawing.Point(312, 24);
            this.btnNuevaReserva.Name = "btnNuevaReserva";
            this.btnNuevaReserva.Size = new System.Drawing.Size(112, 57);
            this.btnNuevaReserva.TabIndex = 4;
            this.btnNuevaReserva.Text = "Nueva reserva";
            this.btnNuevaReserva.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnNuevaReserva.UseVisualStyleBackColor = false;
            // 
            // date1
            // 
            this.date1.Location = new System.Drawing.Point(6, 54);
            this.date1.Name = "date1";
            this.date1.Size = new System.Drawing.Size(280, 25);
            this.date1.TabIndex = 3;
            // 
            // rdFecha
            // 
            this.rdFecha.AutoSize = true;
            this.rdFecha.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdFecha.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdFecha.Location = new System.Drawing.Point(113, 24);
            this.rdFecha.Name = "rdFecha";
            this.rdFecha.Size = new System.Drawing.Size(72, 27);
            this.rdFecha.TabIndex = 2;
            this.rdFecha.TabStop = true;
            this.rdFecha.Text = "Fecha";
            this.rdFecha.UseVisualStyleBackColor = true;
            // 
            // rdCliente
            // 
            this.rdCliente.AutoSize = true;
            this.rdCliente.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdCliente.Font = new System.Drawing.Font("Segoe UI", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdCliente.Location = new System.Drawing.Point(6, 24);
            this.rdCliente.Name = "rdCliente";
            this.rdCliente.Size = new System.Drawing.Size(81, 27);
            this.rdCliente.TabIndex = 1;
            this.rdCliente.TabStop = true;
            this.rdCliente.Text = "Cliente";
            this.rdCliente.UseVisualStyleBackColor = true;
            // 
            // lblResultados
            // 
            this.lblResultados.AutoSize = true;
            this.lblResultados.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblResultados.Location = new System.Drawing.Point(12, 145);
            this.lblResultados.Name = "lblResultados";
            this.lblResultados.Size = new System.Drawing.Size(81, 20);
            this.lblResultados.TabIndex = 1;
            this.lblResultados.Text = "Resultados";
            // 
            // gbEstado
            // 
            this.gbEstado.Controls.Add(this.rdCancelada);
            this.gbEstado.Controls.Add(this.rdTerminada);
            this.gbEstado.Controls.Add(this.rdPendiente);
            this.gbEstado.Location = new System.Drawing.Point(12, 0);
            this.gbEstado.Name = "gbEstado";
            this.gbEstado.Size = new System.Drawing.Size(224, 142);
            this.gbEstado.TabIndex = 3;
            this.gbEstado.TabStop = false;
            this.gbEstado.Text = "Estado de las reservas a buscar";
            // 
            // rdCancelada
            // 
            this.rdCancelada.AutoSize = true;
            this.rdCancelada.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdCancelada.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdCancelada.Location = new System.Drawing.Point(6, 94);
            this.rdCancelada.Name = "rdCancelada";
            this.rdCancelada.Size = new System.Drawing.Size(147, 29);
            this.rdCancelada.TabIndex = 8;
            this.rdCancelada.TabStop = true;
            this.rdCancelada.Tag = "CANCELADA";
            this.rdCancelada.Text = "CANCELADAS";
            this.rdCancelada.UseVisualStyleBackColor = true;
            // 
            // rdTerminada
            // 
            this.rdTerminada.AutoSize = true;
            this.rdTerminada.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdTerminada.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdTerminada.Location = new System.Drawing.Point(6, 59);
            this.rdTerminada.Name = "rdTerminada";
            this.rdTerminada.Size = new System.Drawing.Size(144, 29);
            this.rdTerminada.TabIndex = 7;
            this.rdTerminada.TabStop = true;
            this.rdTerminada.Tag = "TERMINADA";
            this.rdTerminada.Text = "TERMINADAS";
            this.rdTerminada.UseVisualStyleBackColor = true;
            // 
            // rdPendiente
            // 
            this.rdPendiente.AutoSize = true;
            this.rdPendiente.Cursor = System.Windows.Forms.Cursors.Hand;
            this.rdPendiente.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rdPendiente.Location = new System.Drawing.Point(6, 24);
            this.rdPendiente.Name = "rdPendiente";
            this.rdPendiente.Size = new System.Drawing.Size(137, 29);
            this.rdPendiente.TabIndex = 6;
            this.rdPendiente.TabStop = true;
            this.rdPendiente.Tag = "PENDIENTE";
            this.rdPendiente.Text = "PENDIENTES";
            this.rdPendiente.UseVisualStyleBackColor = true;
            // 
            // panelReservas
            // 
            this.panelReservas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelReservas.AutoScroll = true;
            this.panelReservas.Location = new System.Drawing.Point(12, 168);
            this.panelReservas.Name = "panelReservas";
            this.panelReservas.Size = new System.Drawing.Size(899, 314);
            this.panelReservas.TabIndex = 2;
            // 
            // btnSeleccionar
            // 
            this.btnSeleccionar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSeleccionar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSeleccionar.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionar.FlatAppearance.BorderSize = 0;
            this.btnSeleccionar.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
            this.btnSeleccionar.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.btnSeleccionar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSeleccionar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSeleccionar.Image = ((System.Drawing.Image)(resources.GetObject("btnSeleccionar.Image")));
            this.btnSeleccionar.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnSeleccionar.Location = new System.Drawing.Point(6, 88);
            this.btnSeleccionar.Name = "btnSeleccionar";
            this.btnSeleccionar.Size = new System.Drawing.Size(418, 44);
            this.btnSeleccionar.TabIndex = 6;
            this.btnSeleccionar.Text = "Seleccionar";
            this.btnSeleccionar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSeleccionar.UseVisualStyleBackColor = false;
            // 
            // FrmObservarReservas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(918, 494);
            this.Controls.Add(this.gbEstado);
            this.Controls.Add(this.panelReservas);
            this.Controls.Add(this.lblResultados);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "FrmObservarReservas";
            this.Text = "Observar reservas";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.gbEstado.ResumeLayout(false);
            this.gbEstado.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdCliente;
        private System.Windows.Forms.RadioButton rdFecha;
        private System.Windows.Forms.DateTimePicker date1;
        private System.Windows.Forms.Label lblResultados;
        private CapaPresentacion.Controles.CustomGridPanel panelReservas;
        private System.Windows.Forms.Button btnNuevaReserva;
        private System.Windows.Forms.RadioButton rdMesa;
        private System.Windows.Forms.GroupBox gbEstado;
        private System.Windows.Forms.RadioButton rdTerminada;
        private System.Windows.Forms.RadioButton rdPendiente;
        private System.Windows.Forms.RadioButton rdCancelada;
        private System.Windows.Forms.Button btnSeleccionar;
    }
}