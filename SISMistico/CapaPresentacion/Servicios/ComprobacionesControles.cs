using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Data;

namespace CapaPresentacion
{
    public class ComprobacionesControles
    {
        public static List<Control> ComprobacionesInsertar(List<Control> controles)
        {
            List<Control> controlesVacios = new List<Control>(); 
            bool comprobacion = true;
            foreach (Control control in controles)
            {
                if (control is TextBox)
                {
                    if (control.Text.Equals(""))
                    {
                        comprobacion = false;
                        controlesVacios.Add(control);
                    }
                }
                else if (control is Panel)
                {
                    bool comprobacionPanel = false;
                    Panel panel = (Panel)control;
                    foreach (Control con in panel.Controls)
                    {
                        if (con is RadioButton)
                        {
                            RadioButton rd = (RadioButton)con;
                            if (rd.Checked)
                            {
                                comprobacionPanel = true;
                                break;
                            }
                        }
                    }
                    comprobacion = comprobacionPanel;
                    if (comprobacionPanel == false)
                    {
                        controlesVacios.Add(control);
                    }
                }
                else if (control is ComboBox)
                {
                    if (control.Text.Equals(""))
                    {
                        comprobacion = false;
                        controlesVacios.Add(control);
                    }
                }
            }
            return controlesVacios;
        }
    }
}
