using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using CapaPresentacion.Formularios.FormsPrincipales;
using CapaNegocio;
class DesktopPinLoginTests {
 [STAThread] static int Main(string[] args) {
  Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
  try { using(var form=new FrmIniciarSesion()) {
   form.ShowInTaskbar=false;form.StartPosition=FormStartPosition.Manual;form.Location=new Point(-30000,-30000);form.Show();Application.DoEvents();
   Save(form,"login-wide.png");
   var parse=typeof(FrmIniciarSesion).GetMethod("TryReadPin",BindingFlags.NonPublic|BindingFlags.Static);
   foreach(string invalid in new[]{"","0","-1","1 2","abcd","2147483648","+123"}) {object[] v={invalid,0};Check(!(bool)parse.Invoke(null,v),"Reject invalid PIN");}
   object[] valid={"001234",0};Check((bool)parse.Invoke(null,valid)&&(int)valid[1]==1234,"Tablet-compatible integer PIN");
   var field=typeof(FrmIniciarSesion).GetField("pinEntry",BindingFlags.NonPublic|BindingFlags.Instance);var pin=(Control)field.GetValue(form);
   Check(pin.ContainsFocus,"Initial focus");
   var input=Find<TextBox>(pin);Check(input.UseSystemPasswordChar,"PIN masked initially");
   var reveal=Find<Button>(pin);reveal.PerformClick();Check(!input.UseSystemPasswordChar,"Reveal PIN");reveal.PerformClick();Check(input.UseSystemPasswordChar,"Hide PIN");
   input.Text="abc";
   var login=typeof(FrmIniciarSesion).GetMethod("LoginAsync",BindingFlags.NonPublic|BindingFlags.Instance);
   ((Task)login.Invoke(form,new object[]{null})).GetAwaiter().GetResult();
   var status=(Label)typeof(FrmIniciarSesion).GetField("status",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(form);
   Check(status.Text.Length>0,"Inline validation");
   input.Text=""; Check(status.Text.Length==0,"Editing clears error");
   var show=typeof(FrmIniciarSesion).GetMethod("ShowCandidates",BindingFlags.NonPublic|BindingFlags.Instance);
   show.Invoke(form,new object[]{new System.Collections.Generic.List<EmpleadoMovil>{new EmpleadoMovil{IdEmpleado=1,Nombre="Persona de prueba",Cargo="CAJERO"},new EmpleadoMovil{IdEmpleado=2,Nombre="Persona de prueba",Cargo="MESERO"}}});
   var candidates=(Control)typeof(FrmIniciarSesion).GetField("candidates",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(form);
   Check(candidates.Visible && candidates.Controls.Count==2,"Shared PIN candidates");Save(form,"login-candidates.png");
   input.Text="9"; Check(!candidates.Visible,"PIN changes clear candidate choices");input.Text="";
   form.ClientSize=new Size(780,650);Application.DoEvents();Save(form,"login-compact.png");
   Check(pin.Width>300,"Compact PIN stays usable");
   var failure=typeof(FrmIniciarSesion).GetMethod("RegisterFailure",BindingFlags.NonPublic|BindingFlags.Instance);
   for(int i=0;i<5;i++)failure.Invoke(form,null);
   var submit=(Button)typeof(FrmIniciarSesion).GetField("submit",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(form);
   Check(!submit.Enabled,"Repeated failures pause submission");
   typeof(FrmIniciarSesion).GetField("retryAfter",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(form,DateTime.UtcNow.AddSeconds(-1));
   typeof(FrmIniciarSesion).GetMethod("UpdateRetry",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,null);
   Check(submit.Enabled,"Retry enabled after cooldown");
   form.Hide();
  }
  Check(NEmpleados.LoginPorPin(0,1234,DateTime.Today).GetAwaiter().GetResult().rpta=="PIN_INVALIDO","No employee zero bypass");
  if(args.Length>0 && args[0]=="database") CheckDatabase();
  Console.WriteLine("PASS: rendering, PIN validation, focus, masking, inline errors, shared PIN choices, compact layout, employee-zero rejection.");return 0;
  }catch(Exception e){Console.WriteLine(e);return 1;}
 }
 static void CheckDatabase() {
  // Solo lecturas y rechazos: no se ejecuta un login correcto ni se abren/cierran turnos.
  using(var cn=new System.Data.SqlClient.SqlConnection(CapaDatos.Conexion.Cn)) {
   cn.Open(); var cmd=cn.CreateCommand();
   cmd.CommandText="SELECT TOP 1 Id_empleado, Codigo_maestro FROM Empleados WHERE Id_empleado>0 AND Codigo_maestro>0 AND Estado='ACTIVO' ORDER BY Id_empleado";
   int id,pin;using(var reader=cmd.ExecuteReader()){Check(reader.Read(),"Employee with configured PIN");id=reader.GetInt32(0);pin=Convert.ToInt32(reader[1]);}
   var resolved=NApiMovil.ResolverPorPin(pin);
   Check(resolved.Unico!=null ? resolved.Unico.IdEmpleado==id : resolved.Candidatos.Exists(e=>e.IdEmpleado==id),"Same identity resolution as tablet");
   Check(NApiMovil.ValidarEmpleado(id,null,pin,true)!=null,"Tablet PIN credential accepted");
   Check(NApiMovil.ValidarEmpleado(id,null,0,true)==null,"Zero cannot match configured PIN");
   Check(NEmpleados.LoginPorPin(int.MaxValue,pin,DateTime.Today).GetAwaiter().GetResult().rpta=="PIN_INVALIDO","Unknown employee rejected before turn logic");
  }
  Console.WriteLine("PASS: read-only local database identity and invalid-login checks.");
 }
 static T Find<T>(Control parent) where T:Control {foreach(Control c in parent.Controls){if(c is T)return (T)c;try{return Find<T>(c);}catch{}}throw new Exception("Control missing");}
 static void Save(Form form,string file){using(var bmp=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bmp,form.ClientRectangle);bmp.Save(file);}}
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);}
}