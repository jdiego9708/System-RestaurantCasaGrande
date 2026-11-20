using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// DataGridView con soporte tactil nativo (WM_GESTURE), con la misma tecnica que DeliveryTouchList y
    /// TouchScrollPanel:
    ///  - PAN de un dedo en vertical y horizontal, con inercia: se desplazan las filas y las columnas.
    ///  - El toque que termina un arrastre no ejecuta clics (celda, contenido, cabecera para ordenar).
    ///  - Doble toque tolerante: dos toques seguidos sobre la misma fila (aunque el dedo se mueva unos pixeles)
    ///    equivalen al doble clic, que con el dedo rara vez cae dentro de la tolerancia de Windows.
    /// Con raton se comporta como un DataGridView normal.
    /// </summary>
    public class TouchDataGridView : DataGridView
    {
        private const int WmGesture = 0x0119;
        private const int WmCancelMode = 0x001F;
        private const uint GestureBegin = 1, GestureEnd = 4;
        private const uint GidEnd = 2, GidPan = 4;
        private const int ClickGuardMilliseconds = 300;
        private const int DoubleTapMilliseconds = 650;
        private const int DoubleTapTolerance = 28;

        private readonly Stopwatch lastPan = new Stopwatch();
        private readonly Stopwatch lastTap = new Stopwatch();
        private readonly Stopwatch lastDouble = new Stopwatch();
        private bool panning;
        private int previousX, previousY;
        private int pendingY;            // pixeles verticales por aplicar (menos de una fila)
        private Point lastTapPoint;
        private int lastTapRow = -1;

        public TouchDataGridView()
        {
            this.DoubleBuffered = true;
        }

        /// <summary>Activa el desplazamiento con el dedo y el doble toque. Activado por defecto.</summary>
        public bool TouchEnabled { get; set; } = true;

        // True si el ultimo mensaje de raton viene del dedo o lapiz y no de un raton fisico.
        private static bool IsTouchInput =>
            (unchecked((ulong)GetMessageExtraInfo().ToInt64()) & 0xFFFFFF80UL) == 0xFF515780UL;

        // El final de un arrastre tactil no debe contar como clic.
        private bool ClickSuppressed =>
            this.TouchEnabled && IsTouchInput
            && (this.panning || (this.lastPan.IsRunning && this.lastPan.ElapsedMilliseconds < ClickGuardMilliseconds));

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (this.TouchEnabled)
            {
                // PAN con un dedo en vertical y horizontal, direccion estable (GUTTER) e inercia.
                GestureConfig config = new GestureConfig { Id = GidPan, Want = 0x01 | 0x02 | 0x04 | 0x08 | 0x10, Block = 0 };
                SetGestureConfig(this.Handle, 0, 1, ref config, (uint)Marshal.SizeOf(typeof(GestureConfig)));
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (this.TouchEnabled)
            {
                if (m.Msg == WmCancelMode)
                    this.CancelPan();
                if (m.Msg == WmGesture && this.ReadGesture(ref m))
                    return;
            }
            base.WndProc(ref m);
        }

        private bool ReadGesture(ref Message message)
        {
            GestureInfo info = new GestureInfo { Size = (uint)Marshal.SizeOf(typeof(GestureInfo)) };
            if (!GetGestureInfo(message.LParam, ref info))
                return false;
            if (info.Id == GidEnd)
                this.panning = false;
            // BEGIN/END y los gestos que no se tratan se dejan a DefWindowProc, que libera su handle.
            if (info.Id != GidPan)
                return false;
            try
            {
                this.ApplyPan(info.X, info.Y, info.Flags);
                message.Result = IntPtr.Zero;
                return true;
            }
            finally { CloseGestureInfoHandle(message.LParam); }
        }

        // Coordenadas de pantalla: al desplazarse la tabla no cambia la referencia del dedo.
        private void ApplyPan(int screenX, int screenY, uint flags)
        {
            if ((flags & GestureBegin) != 0 || !this.panning)
            {
                this.previousX = screenX;
                this.previousY = screenY;
                this.pendingY = 0;
                this.panning = true;
            }
            else
            {
                int deltaX = this.previousX - screenX;
                int deltaY = this.previousY - screenY;
                this.previousX = screenX;
                this.previousY = screenY;
                this.ScrollBy(deltaX, deltaY);
            }
            this.lastPan.Restart();
            if ((flags & GestureEnd) != 0)
                this.panning = false;
        }

        // Horizontal: en pixeles. Vertical: la tabla se desplaza por filas, asi que los pixeles se acumulan
        // hasta completar una fila (el resto se conserva para el siguiente movimiento).
        private void ScrollBy(int deltaX, int deltaY)
        {
            if (deltaX != 0 && this.ColumnCount > 0)
            {
                int total = 0;
                foreach (DataGridViewColumn column in this.Columns)
                    if (column.Visible)
                        total += column.Width;
                int visible = this.DisplayRectangle.Width;
                int max = Math.Max(0, total - visible);
                this.HorizontalScrollingOffset = Math.Max(0, Math.Min(max, this.HorizontalScrollingOffset + deltaX));
            }

            if (deltaY != 0 && this.RowCount > 0)
            {
                this.pendingY += deltaY;
                int rowHeight = Math.Max(1, this.RowTemplate.Height);
                int rows = this.pendingY / rowHeight;
                if (rows != 0)
                {
                    this.pendingY -= rows * rowHeight;
                    int first = this.FirstDisplayedScrollingRowIndex;
                    if (first < 0)
                        first = 0;
                    int target = Math.Max(0, Math.Min(this.RowCount - 1, first + rows));
                    try
                    {
                        if (target != first)
                            this.FirstDisplayedScrollingRowIndex = target;
                    }
                    catch (InvalidOperationException)
                    {
                        // La fila no se puede mostrar como primera (por ejemplo, oculta o congelada): se ignora.
                    }
                }
            }
        }

        private void CancelPan()
        {
            if (!this.panning)
                return;
            this.panning = false;
            this.lastPan.Restart();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.CancelPan();
            base.OnHandleDestroyed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!this.Visible)
                this.CancelPan();
            base.OnVisibleChanged(e);
        }

        // ---- Clics: el toque que termina un arrastre no se reenvia -------------------------------------------

        protected override void OnCellClick(DataGridViewCellEventArgs e) { if (!this.ClickSuppressed) base.OnCellClick(e); }
        protected override void OnCellContentClick(DataGridViewCellEventArgs e) { if (!this.ClickSuppressed) base.OnCellContentClick(e); }
        protected override void OnCellMouseClick(DataGridViewCellMouseEventArgs e) { if (!this.ClickSuppressed) base.OnCellMouseClick(e); }
        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e) { if (!this.ClickSuppressed) base.OnColumnHeaderMouseClick(e); }

        // ---- Doble toque tolerante --------------------------------------------------------------------------

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (this.TouchEnabled && e.Button == MouseButtons.Left && IsTouchInput && !this.ClickSuppressed)
            {
                int row = this.HitTest(e.X, e.Y).RowIndex;
                bool second = row >= 0 && row == this.lastTapRow && this.lastTap.IsRunning
                    && this.lastTap.ElapsedMilliseconds <= DoubleTapMilliseconds
                    && Math.Abs(e.X - this.lastTapPoint.X) <= DoubleTapTolerance
                    && Math.Abs(e.Y - this.lastTapPoint.Y) <= DoubleTapTolerance + this.RowTemplate.Height;
                if (second)
                {
                    this.lastTapRow = -1;
                    this.lastTap.Reset();
                    base.OnMouseDown(e);
                    this.OnMouseDoubleClick(e);
                    return;
                }
                this.lastTapRow = row;
                this.lastTapPoint = e.Location;
                this.lastTap.Restart();
            }
            base.OnMouseDown(e);
        }

        // Windows tambien puede generar su propio doble clic: se descarta el segundo para no abrir dos veces.
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            // Solo con el dedo: puede llegar el doble toque propio y ademas el doble clic de Windows.
            if (this.TouchEnabled && IsTouchInput)
            {
                if (this.lastDouble.IsRunning && this.lastDouble.ElapsedMilliseconds < DoubleTapMilliseconds)
                    return;
                this.lastDouble.Restart();
            }
            base.OnMouseDoubleClick(e);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GestureConfig { public uint Id, Want, Block; }

        [StructLayout(LayoutKind.Sequential)]
        private struct GestureInfo
        {
            public uint Size, Flags, Id;
            public IntPtr Target;
            public short X, Y;
            public uint Instance, Sequence;
            public ulong Arguments;
            public uint ExtraSize;
        }

        [DllImport("user32.dll")]
        private static extern bool SetGestureConfig(IntPtr hwnd, uint reserved, uint count, ref GestureConfig config, uint size);
        [DllImport("user32.dll")]
        private static extern bool GetGestureInfo(IntPtr handle, ref GestureInfo info);
        [DllImport("user32.dll")]
        private static extern bool CloseGestureInfoHandle(IntPtr handle);
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();
    }
}
