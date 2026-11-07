using CapaEntidades.Models;
using CapaNegocio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsPrincipales
{
    public partial class FrmIniciarSesion : Form
    {
        private bool signingIn;
        private int failedAttempts;
        private DateTime retryAfter;
        private readonly Timer retryTimer = new Timer { Interval = 1000 };

        public FrmIniciarSesion()
        {
            InitializeComponent();
            submit.Click += async (s, e) => await LoginAsync();
            pinEntry.Submit += async (s, e) => await LoginAsync();
            pinEntry.TextChanged += (s, e) =>
            {
                candidates.Visible = false;
                if (!signingIn && DateTime.UtcNow >= retryAfter) SetStatus("", false);
            };
            Shown += (s, e) => pinEntry.FocusInput();
            retryTimer.Tick += (s, e) => UpdateRetry();
            FormClosed += (s, e) => Application.Exit();
        }

        internal static bool TryReadPin(string value, out int pin)
        {
            pin = 0;
            return !string.IsNullOrEmpty(value) && value.All(c => c >= '0' && c <= '9')
                && int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out pin) && pin > 0;
        }

        private async Task LoginAsync(int? employeeId = null)
        {
            if (signingIn || IsDisposed || DateTime.UtcNow < retryAfter) return;
            int pin;
            if (!TryReadPin(pinEntry.Text, out pin))
            {
                SetStatus("Escribe tu PIN num\u00e9rico para continuar.", true);
                pinEntry.FocusInput();
                return;
            }
            SetBusy(true);
            SetStatus("Comprobando tu acceso...", false);
            try
            {
                if (!employeeId.HasValue)
                {
                    // Mismo resolvedor de identidad que /auth/login de la tablet.
                    ResultadoPin result = await Task.Run(() => NApiMovil.ResolverPorPin(pin));
                    if (IsDisposed || Disposing) return;
                    if (result.Ninguno) { RegisterFailure(); pinEntry.SelectAll(); return; }
                    if (result.Ambiguo)
                    {
                        ShowCandidates(result.Candidatos);
                        SetStatus("Elige tu nombre para continuar con este PIN.", false);
                        return;
                    }
                    employeeId = result.Unico.IdEmpleado;
                }
                var login = await NEmpleados.LoginPorPin(employeeId.Value, pin, DateTime.Now);
                if (IsDisposed || Disposing) return;
                if (login.rpta == "PIN_INVALIDO") { RegisterFailure(); return; }
                if (login.rpta != "OK" || login.objects == null || login.objects.Count < 2)
                {
                    SetStatus("No pudimos abrir tu sesi\u00f3n. Solicita ayuda a administraci\u00f3n.", true);
                    Trace.TraceError("Inicio de sesion por PIN: {0}", login.rpta);
                    return;
                }
                StartSession((Empleados)login.objects[0], (Turno)login.objects[1]);
            }
            catch (Exception error)
            {
                if (!IsDisposed && !Disposing)
                    SetStatus("No pudimos conectar. Revisa la conexi\u00f3n e int\u00e9ntalo de nuevo.", true);
                Trace.TraceError("Inicio de sesion por PIN: {0}", error.GetType().Name);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    SetBusy(false);
                    if (Visible && candidates.Visible && candidates.Controls.Count > 0) candidates.Controls[0].Focus();
                    else if (Visible) pinEntry.FocusInput();
                }
            }
        }

        private void ShowCandidates(List<EmpleadoMovil> employees)
        {
            foreach (Control old in candidates.Controls.Cast<Control>().ToArray()) old.Dispose();
            foreach (EmpleadoMovil employee in employees)
            {
                var choice = new PinActionButton
                {
                    Text = employee.Nombre + "  |  " + employee.Cargo + "  #" + employee.IdEmpleado,
                    Primary = false, Height = 44, Width = candidates.ClientSize.Width - 22,
                    Margin = new Padding(0, 0, 0, 6), AccessibleName = "Ingresar como " + employee.Nombre,
                };
                int id = employee.IdEmpleado;
                choice.Click += async (s, e) => await LoginAsync(id);
                candidates.Controls.Add(choice);
            }
            candidates.Visible = true;
        }

        private void StartSession(Empleados employee, Turno shift)
        {
            DatosInicioSesion session = DatosInicioSesion.GetInstancia();
            session.Id_empleado = employee.Id_empleado;
            session.Nombre_empleado = employee.Nombre_empleado;
            session.Cargo_empleado = employee.Cargo_empleado;
            session.EmpleadoLogin = employee;
            session.EmpleadoClaveMaestra = employee;
            session.Turno = shift;
            session.ClienteDefault = new Clientes
            {
                Id_cliente = 0, Nombre_cliente = "NINGUNO", Telefono_cliente = "NINGUNO",
                Correo_electronico = "", Direccion_cliente = "NINGUNO", Referencia_ubicacion = "",
                Otras_observaciones = "", Estado_cliente = "ACTIVO",
            };
            var principal = new FrmPrincipal { WindowState = FormWindowState.Maximized };
            principal.Show();
            pinEntry.Text = "";
            Hide();
        }

        private void SetBusy(bool busy)
        {
            signingIn = busy;
            pinEntry.ReadOnly = busy;
            candidates.Enabled = !busy;
            submit.Enabled = !busy && DateTime.UtcNow >= retryAfter;
            submit.Text = busy ? "Ingresando..." : "Ingresar";
            UseWaitCursor = busy;
        }

        private void SetStatus(string message, bool error)
        {
            status.Text = message;
            status.ForeColor = error ? LoginColors.Error : LoginColors.Muted;
            pinEntry.HasError = error;
            status.AccessibleName = message;
        }

        private void RegisterFailure()
        {
            failedAttempts++;
            if (failedAttempts >= 5)
            {
                retryAfter = DateTime.UtcNow.AddSeconds(30);
                retryTimer.Start();
                UpdateRetry();
            }
            else SetStatus("Ese PIN no coincide. Rev\u00edsalo e int\u00e9ntalo de nuevo.", true);
        }

        private void UpdateRetry()
        {
            int seconds = Math.Max(0, (int)Math.Ceiling((retryAfter - DateTime.UtcNow).TotalSeconds));
            if (seconds == 0)
            {
                retryTimer.Stop(); failedAttempts = 0; submit.Enabled = !signingIn;
                submit.Text = "Ingresar";
                SetStatus("Ya puedes intentarlo de nuevo.", false);
            }
            else
            {
                submit.Enabled = false; submit.Text = "Espera " + seconds + " s";
                SetStatus("Varios intentos sin coincidencia. Espera un momento para volver a intentar.", true);
            }
        }
    }
}
