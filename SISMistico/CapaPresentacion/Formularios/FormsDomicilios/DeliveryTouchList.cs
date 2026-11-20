using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    // Solo se usa en Domicilios. Los gestos llegan tambien a las fichas y sus botones hijos.
    internal sealed class DeliveryTouchList : FlowLayoutPanel
    {
        private const int WmGesture = 0x0119;
        private const uint Pan = 4, Begin = 1, End = 4;
        private readonly Dictionary<Control, GestureWindow> windows = new Dictionary<Control, GestureWindow>();
        private readonly Stopwatch lastPan = new Stopwatch();
        private bool panning;
        private int previousY;

        public DeliveryTouchList()
        {
            DoubleBuffered = true;
        }

        internal bool CanActivate(bool touchInput) => !touchInput || (!panning && (!lastPan.IsRunning || lastPan.ElapsedMilliseconds >= 300));

        internal static bool IsTouchInput => (unchecked((ulong)GetMessageExtraInfo().ToInt64()) & 0xFFFFFF80UL) == 0xFF515780UL;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ConfigureGestures(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (ReadGesture(ref m)) return;
            base.WndProc(ref m);
        }

        private bool ReadGesture(ref Message message)
        {
            if (message.Msg == 0x001F) CancelPan(); // WM_CANCELMODE: interrupcion por otra ventana o captura.
            if (message.Msg != WmGesture) return false;
            GestureInfo info = new GestureInfo { Size = (uint)Marshal.SizeOf(typeof(GestureInfo)) };
            if (!GetGestureInfo(message.LParam, ref info)) return false;
            if (info.Id == 2) panning = false;
            // BEGIN/END y los mensajes no tratados se dejan a DefWindowProc, que libera su handle.
            if (info.Id != Pan) return false;
            try
            {
                ApplyPan(info.Y, info.Flags);
                message.Result = IntPtr.Zero;
                return true;
            }
            finally { CloseGestureInfoHandle(message.LParam); }
        }

        // Coordenadas de pantalla: al desplazarse las fichas no cambia la referencia del dedo.
        internal void ApplyPan(int screenY, uint flags)
        {
            if ((flags & Begin) != 0 || !panning)
            {
                previousY = screenY;
                panning = true;
            }
            else
            {
                int delta = previousY - screenY;
                previousY = screenY;
                int maximum = Math.Max(0, VerticalScroll.Maximum - VerticalScroll.LargeChange + 1);
                int position = Math.Max(0, Math.Min(maximum, -AutoScrollPosition.Y + delta));
                AutoScrollPosition = new Point(Math.Max(0, -AutoScrollPosition.X), position);
            }
            lastPan.Restart();
            if ((flags & End) != 0) panning = false;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            AttachTree(e.Control);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            CancelPan();
            DetachTree(e.Control);
            base.OnControlRemoved(e);
        }

        private void ChildAdded(object sender, ControlEventArgs e) => AttachTree(e.Control);
        private void ChildRemoved(object sender, ControlEventArgs e) => DetachTree(e.Control);

        private void CancelPan()
        {
            if (!panning) return;
            panning = false;
            lastPan.Restart();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            CancelPan();
            base.OnHandleDestroyed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!Visible) CancelPan();
            base.OnVisibleChanged(e);
        }

        private void AttachTree(Control control)
        {
            if (windows.ContainsKey(control)) return;
            windows.Add(control, new GestureWindow(this, control));
            control.ControlAdded += ChildAdded;
            control.ControlRemoved += ChildRemoved;
            foreach (Control child in control.Controls) AttachTree(child);
        }

        private void DetachTree(Control control)
        {
            foreach (Control child in control.Controls) DetachTree(child);
            if (!windows.TryGetValue(control, out GestureWindow window)) return;
            control.ControlAdded -= ChildAdded;
            control.ControlRemoved -= ChildRemoved;
            window.Dispose();
            windows.Remove(control);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Control control in windows.Keys.ToArray()) DetachTree(control);
                lastPan.Stop();
            }
            base.Dispose(disposing);
        }

        private static void ConfigureGestures(IntPtr handle)
        {
            // PAN, un dedo vertical, direccion estable e inercia; sin arrastre horizontal.
            GestureConfig config = new GestureConfig { Id = Pan, Want = 0x01 | 0x02 | 0x08 | 0x10, Block = 0x04 };
            SetGestureConfig(handle, 0, 1, ref config, (uint)Marshal.SizeOf(typeof(GestureConfig)));
        }

        private sealed class GestureWindow : NativeWindow, IDisposable
        {
            private readonly DeliveryTouchList owner;
            private readonly Control control;

            public GestureWindow(DeliveryTouchList owner, Control control)
            {
                this.owner = owner;
                this.control = control;
                control.HandleCreated += Created;
                control.HandleDestroyed += Destroyed;
                if (control.IsHandleCreated) Created(control, EventArgs.Empty);
            }

            private void Created(object sender, EventArgs e)
            {
                AssignHandle(control.Handle);
                ConfigureGestures(control.Handle);
            }

            private void Destroyed(object sender, EventArgs e)
            {
                owner.CancelPan();
                if (Handle != IntPtr.Zero) ReleaseHandle();
            }

            protected override void WndProc(ref Message m)
            {
                if (!owner.ReadGesture(ref m)) base.WndProc(ref m);
            }

            public void Dispose()
            {
                control.HandleCreated -= Created;
                control.HandleDestroyed -= Destroyed;
                Destroyed(control, EventArgs.Empty);
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

    // La liberacion del dedo tras desplazar una ficha nunca debe ejecutar una accion de su boton.
    internal sealed class DeliveryTouchButton : MinimalIconButton
    {
        public DeliveryTouchButton() { MinimalStyle = false; }
        protected override void OnClick(EventArgs e)
        {
            for (Control parent = Parent; parent != null; parent = parent.Parent)
                if (parent is DeliveryTouchList list && !list.CanActivate(DeliveryTouchList.IsTouchInput)) return;
            base.OnClick(e);
        }
    }
}
