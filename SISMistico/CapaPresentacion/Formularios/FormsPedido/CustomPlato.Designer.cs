
namespace CapaPresentacion.Formularios.FormsPedido
{
    // El contenido se construye en codigo (CustomPlato.cs); aqui solo queda el ciclo de vida del control.
    partial class CustomPlato
    {
        private System.ComponentModel.IContainer components = null;

        // Sin controles de diseñador: la interfaz se arma en el constructor de CustomPlato.
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "CustomPlato";
            this.Size = new System.Drawing.Size(640, 500);
            this.ResumeLayout(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
