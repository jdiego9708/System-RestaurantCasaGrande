using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CapaPresentacion.Controles
{
    public partial class CustomGridPanel : Panel
    {
        public List<UserControl> controlsUser;
        public List<Control> controlsDefault;

        public CustomGridPanel()
        {
            controlsUser = new List<UserControl>();
            controlsDefault = new List<Control>();
            this.AutoScroll = true;
            this.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
                | AnchorStyles.Left) | AnchorStyles.Right)));
        }

        public void AddControl(Control control)
        {
            //Si la lista de controles está null creamos una nueva
            if (controlsDefault == null)
                controlsDefault = new List<Control>();

            //Agregamos el control a la lista
            this.controlsDefault.Add(control);
            this.RefreshPanel(control);
        }

        public void AddControl(UserControl control)
        {
            //Si la lista de controles está null creamos una nueva
            if (controlsUser == null)
                controlsUser = new List<UserControl>();

            //Agregamos el control a la lista
            this.controlsUser.Add(control);
            this.RefreshPanel(control);
        }

        public void AddArrayControl(List<Control> controls)
        {
            //Si la lista de controles está null creamos una nueva
            if (controls == null)
                controls = new List<Control>();

            Control control = null;
            foreach (Control control1 in controls)
            {
                control = control1;
                //Agregamos el control a las listas
                this.controlsDefault.Add(control);
            }

            if (control != null)
                this.RefreshPanel(control);
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
                this.controlsUser.Add(userControl);
            }

            if (user != null)
                this.RefreshPanel(user);
        }

        public void RemoveControl(UserControl control)
        {
            //Si la lista de controles está null o no hay controles retornamos
            if (controlsUser == null ||
                controlsUser.Count == 0)
                return;

            //Agregamos el control a la lista
            this.controlsUser.Remove(control);
            this.RefreshPanel(control);
        }

        public void RemoveControl(Control control)
        {
            //Si la lista de controles está null o no hay controles retornamos
            if (controlsDefault == null ||
                controlsDefault.Count == 0)
                return;

            //Agregamos el control a la lista
            this.controlsDefault.Remove(control);
            this.RefreshPanel(control);
        }

        public void Limpiar()
        {
            this.controlsDefault.Clear();
            this.controlsUser.Clear();
            this.Controls.Clear();
        }

        private void RefreshPanel(UserControl control)
        {
            //Limpiar todos los controles que tengamos
            this.Controls.Clear();
            //Si la cantidad de controles es mayor que cero, iniciamos
            if (this.controlsUser.Count > 0)
            {
                //Ancho del panel
                int ancho_panel = this.Width;
                //Cantidad de controles de la lista
                int cantidad_controles = controlsUser.Count;
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
                    Control user = (Control)controlsUser[0];
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
                    foreach (Control con in controlsUser)
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

        private void RefreshPanel(Control control)
        {
            //Limpiar todos los controles que tengamos
            this.Controls.Clear();
            //Si la cantidad de controles es mayor que cero, iniciamos
            if (this.controlsDefault.Count > 0)
            {
                //Ancho del panel
                int ancho_panel = this.Width;
                //Cantidad de controles de la lista
                int cantidad_controles = controlsDefault.Count;
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
                    Control user = (Control)controlsDefault[0];
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
                    foreach (Control con in controlsDefault)
                    {
                        //Casteo el UserControl
                        Control control1 = (Control)con;
                        //Si positionColumn es menor que la cantidad de columnas
                        //continuamos
                        if (column <= cantidad_columnas)
                        {
                            control1.Location = new Point(positionX, positionY);
                            positionX += control1.Width;

                            //Sumar uno a la positionColumn
                            column += 1;
                        }
                        else
                        {
                            positionY += control1.Height;
                            column = 1;
                            positionX = 0;

                            control1.Location =
                                    new Point(positionX, positionY);
                            positionX += control1.Width;
                            column += 1;
                        }
                        this.Controls.Add(control1);
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
        BindingList<List<UserControl>> UserControls = new BindingList<List<UserControl>>();
        BindingList<List<Control>> ControlsDefault = new BindingList<List<Control>>();
        BindingList<List<object>> objetos = new BindingList<List<object>>();

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
            bs.PositionChanged += bs_PositionChangedDataTable;
            bs_PositionChangedDataTable(bs, EventArgs.Empty);
        }

        public void SetPagedDataSource(List<UserControl> controls,
            BindingNavigator bnav)
        {
            this.clearDataSource();
            int counter = 0;
            List<UserControl> list = new List<UserControl>();

            foreach (UserControl c in controls)
            {
                list.Add(c);
                counter += 1;
                if (counter > PageSize)
                {
                    UserControls.Add(list);
                    counter = 1;
                    list = new List<UserControl>();
                }
            }

            if (list.Count > 0)
                UserControls.Add(list);

            bnav.BindingSource = bs;
            bs.DataSource = UserControls;
            bs.PositionChanged += bs_PositionChangedList;
            bs_PositionChangedList(bs, EventArgs.Empty);
        }

        public void SetPagedDataSource(List<object> controls,
           BindingNavigator bnav)
        {
            this.clearDataSource();
            int counter = 0;
            List<object> list = new List<object>();

            foreach (object c in controls)
            {
                list.Add(c);
                counter += 1;
                if (counter > PageSize)
                {
                    objetos.Add(list);
                    counter = 1;
                    list = new List<object>();
                }
            }

            if (list.Count > 0)
                objetos.Add(list);

            bnav.BindingSource = bs;
            bs.DataSource = objetos;
            bs.PositionChanged += bs_PositionChangedList;
            bs_PositionChangedObject(bs, EventArgs.Empty);
        }

        private void bs_PositionChangedObject(object sender, EventArgs e)
        {
            List<object> list = objetos[bs.Position];

            if (this.Controls.Count > 0)
                this.Limpiar();

            OnBsPositionChanged?.Invoke(list, e);
        }

        private void bs_PositionChangedList(object sender, EventArgs e)
        {
            List<UserControl> list = UserControls[bs.Position];

            if (this.Controls.Count > 0)
                this.Limpiar();

            OnBsPositionChanged?.Invoke(list, e);
        }

        private void bs_PositionChangedDataTable(object sender, EventArgs e)
        {
            DataTable dt = tables[bs.Position];

            if (this.Controls.Count > 0)
                this.Limpiar();

            OnBsPositionChanged?.Invoke(dt, e);
        }

        public void clearDataSource()
        {
            this.Limpiar();
            this.tables = new BindingList<DataTable>();
            this.UserControls = new BindingList<List<UserControl>>();
            this.ControlsDefault = new BindingList<List<Control>>();
            this.objetos = new BindingList<List<object>>();
            this.bs = new BindingSource();
        }

        public event EventHandler OnBsPositionChanged;
    }
}
