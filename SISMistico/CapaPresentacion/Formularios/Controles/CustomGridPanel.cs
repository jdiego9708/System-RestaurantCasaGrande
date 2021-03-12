using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CapaPresentacion.Controles
{
    public partial class CustomGridPanel : Panel
    {
        public List<UserControl> controls;
        public CustomGridPanel()
        {
            controls = new List<UserControl>();
            this.AutoScroll = true;
            this.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left) | AnchorStyles.Right)));
        }

        public void AddArrayControl(List<UserControl> controls)
        {
            //Si la lista de controles está null creamos una nueva
            if (controls == null)
                controls = new List<UserControl>();

            UserControl user = null;
            foreach (UserControl userControl in controls)
            {
                user = userControl;
                //Agregamos el control a las listas
                this.controls.Add(userControl);
            }

            if (user != null)
                this.RefreshPanel(user);
        }

        public void AddControl(UserControl control)
        {
            //Si la lista de controles está null creamos una nueva
            if (controls == null)
                controls = new List<UserControl>();

            //Agregamos el control a la lista
            this.controls.Add(control);
            this.RefreshPanel(control);
        }

        public void RemoveControl(UserControl control)
        {
            //Si la lista de controles está null o no hay controles retornamos
            if (controls == null | controls.Count == 0)
                return;

            //Agregamos el control a la lista
            this.controls.Remove(control);
            this.RefreshPanel(control);
        }

        private void Limpiar()
        {
            this.controls.Clear();
            this.Controls.Clear();
        }

        private void RefreshPanel(UserControl control)
        {
            //Limpiar todos los controles que tengamos
            this.Controls.Clear();
            //Si la cantidad de controles es mayor que cero, iniciamos
            if (this.controls.Count > 0)
            {
                //Ancho del panel
                int ancho_panel = this.Width;
                //Cantidad de controles de la lista
                int cantidad_controles = controls.Count;
                //Ancho del control que recibo
                int ancho_x_control = control.Width;
                //Cantidad de columnas en double, división entre ancho panel y ancho por control
                double cantidad_columns = ancho_panel / ancho_x_control;
                //Cantidad de columnas en entero, redondeando el double
                int cantidad_columnas = Convert.ToInt32(Math.Round(cantidad_columns, MidpointRounding.AwayFromZero));
                //Cantidad de filas
                int cantidad_filas = cantidad_controles / cantidad_columnas;

                //Si la cantidad de controles es igual a 1, agregaremos el primer control al panel
                if (cantidad_controles == 1)
                {
                    UserControl user = (UserControl)controls[0];
                    user.Location = new Point(0, 0);
                    this.Controls.Add(user);
                }
                else
                {
                    //Se usa para saber cuantos elementos se debe poner por fila
                    //No puede ser mayor que el número de columnas
                    int column = 1;
                    int positionX = 0;
                    int positionY = 0;
                    foreach (UserControl con in controls)
                    {
                        //Casteo el UserControl
                        UserControl user = (UserControl)con;
                        //Si positionColumn es menor que la cantidad de columnas
                        //continuamos
                        if (column <= cantidad_columnas)
                        {
                            user.Location = new Point(positionX, positionY);
                            positionX += user.Width;

                            //Sumar uno a la positionColumn
                            column += 1;
                        }
                        else
                        {
                            positionY += user.Height;
                            column = 1;
                            positionX = 0;

                            user.Location =
                                    new Point(positionX, positionY);
                            positionX += user.Width;
                            column += 1;
                        }
                        this.Controls.Add(user);
                    }
                }
            }
        }

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

        public int _pageSize = 10;

        public BindingSource bs = new BindingSource();
        BindingList<DataTable> tables = new BindingList<DataTable>();

        public void SetPagedDataSource(DataTable dataTable,
            BindingNavigator bnav)
        {
            DataTable dt = null;
            this.clearDataSource();
            int counter = 1;
            foreach (DataRow dr in dataTable.Rows)
            {
                if (counter == 1)
                {
                    dt = dataTable.Clone();
                    tables.Add(dt);
                }
                dt.Rows.Add(dr.ItemArray);
                if (PageSize < ++counter)
                {
                    counter = 1;
                }
            }
            bnav.BindingSource = bs;
            bs.DataSource = tables;
            bs.PositionChanged += bs_PositionChanged;
            bs_PositionChanged(bs, EventArgs.Empty);
        }

        public event EventHandler OnBsPositionChanged;

        void bs_PositionChanged(object sender, EventArgs e)
        {
            DataTable dt = tables[bs.Position];
            this.Limpiar();
            OnBsPositionChanged?.Invoke(dt, e);
        }

        public void clearDataSource()
        {
            this.Limpiar();
            this.tables = new BindingList<DataTable>();
            this.bs = new BindingSource();
        }

    }
}
