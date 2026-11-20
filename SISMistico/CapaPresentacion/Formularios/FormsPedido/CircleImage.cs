namespace CapaPresentacion.Formularios.FormsPedido
{
    using CapaPresentacion.UI;
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>Foto recortada en circulo; sin foto muestra un circulo gris. Compartida por las fichas de producto.</summary>
    internal sealed class CircleImage : Control
    {
        private Image picture;

        public CircleImage()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
        }

        // Igual que los botones: el final de un arrastre tactil no cuenta como toque sobre la foto.
        protected override void OnClick(EventArgs e)
        {
            if (TouchScrollPanel.IsClickSuppressed(this)) return;
            base.OnClick(e);
        }

        public Image Picture
        {
            get => this.picture;
            set
            {
                // Las fotos del pedido se cargan en otro hilo: la asignacion pasa al hilo de la ventana.
                if (this.IsHandleCreated && this.InvokeRequired)
                {
                    this.BeginInvoke((Action)(() => this.Picture = value));
                    return;
                }
                this.picture = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath circle = new GraphicsPath())
            {
                circle.AddEllipse(bounds);
                if (this.picture != null)
                {
                    GraphicsState state = g.Save();
                    g.SetClip(circle);
                    g.DrawImage(this.picture, CoverRectangle(this.picture, bounds));
                    g.Restore(state);
                }
                else
                {
                    using (SolidBrush fill = new SolidBrush(Theme.Border))
                        g.FillPath(fill, circle);
                }
                using (Pen border = new Pen(Theme.BorderStrong))
                    g.DrawPath(border, circle);
            }
        }

        // Escala la foto para cubrir el circulo sin deformarla (recorte centrado).
        private static Rectangle CoverRectangle(Image source, Rectangle target)
        {
            float scale = Math.Max((float)target.Width / source.Width, (float)target.Height / source.Height);
            int w = (int)Math.Ceiling(source.Width * scale);
            int h = (int)Math.Ceiling(source.Height * scale);
            return new Rectangle(target.X + (target.Width - w) / 2, target.Y + (target.Height - h) / 2, w, h);
        }
    }
}
