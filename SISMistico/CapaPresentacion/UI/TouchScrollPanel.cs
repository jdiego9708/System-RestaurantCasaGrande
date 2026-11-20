using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CapaPresentacion.UI
{
    /// <summary>
    /// Panel con desplazamiento tactil nativo de Windows Forms: gesto PAN de un dedo (vertical y horizontal), con inercia,
    /// mediante WM_GESTURE. Es la misma tecnica de DeliveryTouchList (Domicilios), en una clase reutilizable:
    /// los gestos llegan tambien a las fichas y a sus controles hijos, porque cada ventana hija se engancha con
    /// un NativeWindow. Tras un arrastre se bloquea durante 300 ms el clic de los controles que consulten
    /// <see cref="IsClickSuppressed"/> (MinimalIconButton y las fotos lo hacen), para que levantar el dedo
    /// al terminar de desplazar no pulse un boton.
    /// El gesto esta apagado por defecto: cada pantalla lo activa con <see cref="TouchPanning"/>.
    /// </summary>
    public class TouchScrollPanel : Panel
    {
        private const int WmGesture = 0x0119;
        private const int WmCancelMode = 0x001F;
        private const uint GestureBegin = 1, GestureEnd = 4;
        private const uint GidEnd = 2, GidPan = 4;
        private const uint ClickGuardMilliseconds = 300;

        private readonly Dictionary<Control, GestureWindow> windows = new Dictionary<Control, GestureWindow>();
        private readonly Stopwatch lastPan = new Stopwatch();
        private bool touchPanning;
        private bool panning;
        private int previousX, previousY;

        public TouchScrollPanel()
        {
            this.DoubleBuffered = true;
        }

        /// <summary>Activa el desplazamiento con el dedo en este panel y en todo lo que contiene.</summary>
        public bool TouchPanning
        {
            get => this.touchPanning;
            set
            {
                if (this.touchPanning == value)
                    return;
                this.touchPanning = value;
                if (value)
                {
                    if (this.IsHandleCreated)
                        ConfigureGestures(this.Handle);
                    foreach (Control child in this.Controls)
                        this.AttachTree(child);
                }
                else
                {
                    this.CancelPan();
                    foreach (Control child in this.Controls.Cast<Control>().ToArray())
                        this.DetachTree(child);
                }
            }
        }

        /// <summary>True si el ultimo mensaje de raton viene del dedo (o lapiz) y no de un raton fisico.</summary>
        public static bool IsTouchInput =>
            (unchecked((ulong)GetMessageExtraInfo().ToInt64()) & 0xFFFFFF80UL) == 0xFF515780UL;

        /// <summary>
        /// True si un clic de <paramref name="control"/> debe ignorarse porque es el final de un arrastre tactil
        /// de alguno de sus paneles contenedores. Un clic con raton nunca se bloquea.
        /// </summary>
        public static bool IsClickSuppressed(Control control)
        {
            if (!IsTouchInput)
                return false;
            for (Control parent = control.Parent; parent != null; parent = parent.Parent)
                if (parent is TouchScrollPanel panel && panel.touchPanning && !panel.CanActivate())
                    return true;
            return false;
        }

        private bool CanActivate() =>
            !this.panning && (!this.lastPan.IsRunning || this.lastPan.ElapsedMilliseconds >= ClickGuardMilliseconds);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (this.touchPanning)
                ConfigureGestures(this.Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (this.ReadGesture(ref m))
                return;
            base.WndProc(ref m);
        }

        private bool ReadGesture(ref Message message)
        {
            if (!this.touchPanning)
                return false;
            if (message.Msg == WmCancelMode)
                this.CancelPan(); // interrupcion por otra ventana o captura
            if (message.Msg != WmGesture)
                return false;

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

        // Coordenadas de pantalla: al desplazarse las fichas no cambia la referencia del dedo.
        private void ApplyPan(int screenX, int screenY, uint flags)
        {
            if ((flags & GestureBegin) != 0 || !this.panning)
            {
                this.previousX = screenX;
                this.previousY = screenY;
                this.panning = true;
            }
            else
            {
                int deltaX = this.previousX - screenX;
                int deltaY = this.previousY - screenY;
                this.previousX = screenX;
                this.previousY = screenY;
                int maxY = Math.Max(0, this.VerticalScroll.Maximum - this.VerticalScroll.LargeChange + 1);
                int maxX = Math.Max(0, this.HorizontalScroll.Maximum - this.HorizontalScroll.LargeChange + 1);
                int y = Math.Max(0, Math.Min(maxY, -this.AutoScrollPosition.Y + deltaY));
                int x = Math.Max(0, Math.Min(maxX, -this.AutoScrollPosition.X + deltaX));
                this.AutoScrollPosition = new Point(x, y);
            }
            this.lastPan.Restart();
            if ((flags & GestureEnd) != 0)
                this.panning = false;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (this.touchPanning)
                this.AttachTree(e.Control);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            this.CancelPan();
            this.DetachTree(e.Control);
            base.OnControlRemoved(e);
        }

        private void ChildAdded(object sender, ControlEventArgs e) => this.AttachTree(e.Control);
        private void ChildRemoved(object sender, ControlEventArgs e) => this.DetachTree(e.Control);

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

        private void AttachTree(Control control)
        {
            if (this.windows.ContainsKey(control))
                return;
            this.windows.Add(control, new GestureWindow(this, control));
            control.ControlAdded += this.ChildAdded;
            control.ControlRemoved += this.ChildRemoved;
            foreach (Control child in control.Controls)
                this.AttachTree(child);
        }

        private void DetachTree(Control control)
        {
            foreach (Control child in control.Controls)
                this.DetachTree(child);
            if (!this.windows.TryGetValue(control, out GestureWindow window))
                return;
            control.ControlAdded -= this.ChildAdded;
            control.ControlRemoved -= this.ChildRemoved;
            window.Dispose();
            this.windows.Remove(control);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control control in this.windows.Keys.ToArray())
                    this.DetachTree(control);
                this.lastPan.Stop();
            }
            base.Dispose(disposing);
        }

        private static void ConfigureGestures(IntPtr handle)
        {
            // PAN con un dedo en vertical y horizontal, direccion estable (GUTTER) e inercia.
            GestureConfig config = new GestureConfig { Id = GidPan, Want = 0x01 | 0x02 | 0x04 | 0x08 | 0x10, Block = 0 };
            SetGestureConfig(handle, 0, 1, ref config, (uint)Marshal.SizeOf(typeof(GestureConfig)));
        }

        // Escucha los gestos de una ventana hija y los entrega al panel.
        private sealed class GestureWindow : NativeWindow, IDisposable
        {
            private readonly TouchScrollPanel owner;
            private readonly Control control;

            public GestureWindow(TouchScrollPanel owner, Control control)
            {
                this.owner = owner;
                this.control = control;
                control.HandleCreated += this.Created;
                control.HandleDestroyed += this.Destroyed;
                if (control.IsHandleCreated)
                    this.Created(control, EventArgs.Empty);
            }

            private void Created(object sender, EventArgs e)
            {
                this.AssignHandle(this.control.Handle);
                ConfigureGestures(this.control.Handle);
            }

            private void Destroyed(object sender, EventArgs e)
            {
                this.owner.CancelPan();
                if (this.Handle != IntPtr.Zero)
                    this.ReleaseHandle();
            }

            protected override void WndProc(ref Message m)
            {
                if (!this.owner.ReadGesture(ref m))
                    base.WndProc(ref m);
            }

            public void Dispose()
            {
                this.control.HandleCreated -= this.Created;
                this.control.HandleDestroyed -= this.Destroyed;
                this.Destroyed(this.control, EventArgs.Empty);
            }
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
