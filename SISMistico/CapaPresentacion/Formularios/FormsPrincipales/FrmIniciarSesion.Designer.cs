namespace CapaPresentacion.Formularios.FormsPrincipales
{
    partial class FrmIniciarSesion
    {
        private System.ComponentModel.IContainer components;
        protected override void Dispose(bool disposing)
        {
            if (disposing) { retryTimer.Dispose(); components?.Dispose(); }
            base.Dispose(disposing);
            if (disposing) foreach (System.Drawing.Font font in ownedFonts) font.Dispose();
        }
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            BuildPinLayout();
        }
    }
}
