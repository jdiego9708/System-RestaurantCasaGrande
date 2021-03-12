using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapaPresentacion.Formularios.FormsNotas
{
    public partial class NotaSmall : UserControl
    {
        public NotaSmall()
        {
            InitializeComponent();
        }

        ENotas ENotas;

        public void AsignarDatos(ENotas eNotas)
        {
            this.ENotas = eNotas;
            this.txtDescripcion.Text = eNotas.Descripcion_nota;
            this.txtTitulo.Text = eNotas.Titulo_nota;
            this.txtFecha.Text = eNotas.Fecha_nota.ToString("dddd dd - MMMM");
            this.Refresh();
        }
    }
}
