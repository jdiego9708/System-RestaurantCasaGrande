using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapaNegocio;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsProductos
{
    public class FrmProducto : Form
    {
        private readonly DataRow original;
        private readonly ModernRadioButton plate = new ModernRadioButton { Text = "Plato" };
        private readonly ModernRadioButton beverage = new ModernRadioButton { Text = "Bebida" };
        private readonly FlowLayoutPanel type = new FlowLayoutPanel { Height = 44, WrapContents = false };
        private readonly ModernComboBox category = new ModernComboBox();
        private readonly ModernTextBox name = new ModernTextBox { MaxLength = 200, PlaceholderText = "Ej. Lomo a la parrilla" };
        private readonly ModernTextBox price = new ModernTextBox { PlaceholderText = "Ej. 25000 o 25000,50" };
        private readonly ModernTextBox description = new ModernTextBox { PlaceholderText = "Descripción opcional" };
        private readonly ModernCheckBox active = new ModernCheckBox { Text = "Producto activo", Checked = true };
        private readonly ModernCheckBox menu = new ModernCheckBox { Text = "Disponible en la carta", Checked = true };
        private readonly ModernCheckBox ingredients = new ModernCheckBox { Text = "Habilitar ingredientes (sopa, arroz, acompañante...)", Checked = false };
        private readonly Label error = new Label { ForeColor = Theme.Danger, AutoSize = false };
        private readonly ModernButton save = new ModernButton { Text = "Guardar producto" };
        private bool loading;
        private bool saving;

        public FrmProducto(DataRow product = null)
        {
            original = product;
            Text = product == null ? "Nuevo producto" : "Editar producto";
            BackColor = Theme.Background; Font = Theme.Body;
            StartPosition = FormStartPosition.CenterParent; ClientSize = new Size(620, 710);
            MinimumSize = new Size(560, 670); MaximizeBox = false; MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;
            FrmProductos.ApplyWindowIcon(this);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 1, RowCount = 4 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label { Text = Text, Font = Theme.Title, ForeColor = Theme.TextPrimary, AutoSize = true });
            header.Controls.Add(new Label { Text = "Datos del catálogo · Precios en pesos colombianos", Font = Theme.Small,
                ForeColor = Theme.TextMuted, AutoSize = true, Top = 42 });
            root.Controls.Add(header, 0, 0);
            var card = new ModernCard { Dock = DockStyle.Fill, Padding = new Padding(22), AccentColor = Theme.Primary };
            var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoScroll = true };
            plate.Margin = beverage.Margin = Padding.Empty;
            type.Controls.Add(plate); type.Controls.Add(beverage);
            AddField(fields, "Tipo de producto *", type);
            AddField(fields, "Categoría *", category);
            AddField(fields, "Nombre del producto *", name);
            AddField(fields, "Precio de venta (COP) *", price);
            AddField(fields, "Descripción", description);
            active.Dock = DockStyle.Top; menu.Dock = DockStyle.Top; ingredients.Dock = DockStyle.Top;
            active.Margin = menu.Margin = ingredients.Margin = Padding.Empty;
            fields.Controls.Add(active); fields.Controls.Add(menu); fields.Controls.Add(ingredients);
            error.Dock = DockStyle.Fill; error.Margin = new Padding(4, 4, 4, 0); root.Controls.Add(error, 0, 2);
            card.Controls.Add(fields); root.Controls.Add(card, 0, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 16, 0, 0) };
            save.Width = 192;
            var cancel = new ModernButton { Text = "Cancelar", Kind = ButtonKind.Outline, Width = 116, DialogResult = DialogResult.Cancel };
            footer.Controls.Add(save); footer.Controls.Add(cancel); root.Controls.Add(footer, 0, 3);
            Controls.Add(root); CancelButton = cancel; AcceptButton = save;
            beverage.Checked = product != null && Convert.ToString(product["Tipo"]) == "BEBIDA";
            plate.Checked = !beverage.Checked;
            plate.CheckedChanged += async (s, e) => { if (plate.Checked) await LoadCategories(); };
            beverage.CheckedChanged += async (s, e) => { if (beverage.Checked) await LoadCategories(); };
            save.Click += async (s, e) => await Save();
            Shown += async (s, e) =>
            {
                if (original != null)
                {
                    name.Text = Convert.ToString(original["Nombre"]);
                    price.Text = Convert.ToDecimal(original["Precio"]).ToString("0.##", CultureInfo.GetCultureInfo("es-CO"));
                    description.Text = Convert.ToString(original["Descripcion"]);
                    active.Checked = Convert.ToString(original["Estado"]) == "ACTIVO";
                    menu.Checked = Convert.ToString(original["Carta"]) == "ACTIVO";
                    ingredients.Checked = Convert.ToString(original["Detallado"]) == "ACTIVO";
                }
                await LoadCategories(); name.Focus();
            };
            FormClosing += (s, e) => { if (saving || loading) e.Cancel = true; };
        }

        private static void AddField(TableLayoutPanel panel, string title, Control input)
        {
            panel.Controls.Add(new Label { Text = title, Font = Theme.Small, ForeColor = Theme.TextMuted,
                Dock = DockStyle.Top, Height = 21, Margin = new Padding(0, 3, 0, 0) });
            input.Dock = DockStyle.Top; input.Margin = new Padding(0, 0, 0, 3);
            panel.Controls.Add(input);
        }

        private async Task LoadCategories()
        {
            loading = true; save.Enabled = false; type.Enabled = false;
            bool isBeverage = beverage.Checked;
            menu.Visible = ingredients.Visible = !isBeverage; description.MaxLength = isBeverage ? 200 : 500;
            try
            {
                error.Text = "";
                category.DataSource = null;
                DataTable rows = await Task.Run(() => NProductos.Categorias(isBeverage));
                category.DisplayMember = "Nombre"; category.ValueMember = "Id"; category.DataSource = rows;
                if (original != null && (Convert.ToString(original["Tipo"]) == "BEBIDA") == isBeverage)
                    category.SelectedValue = original["CategoriaId"];
                else category.SelectedIndex = -1;
                if (rows.Rows.Count == 0) error.Text = "No hay categorías configuradas para este tipo de producto.";
            }
            catch (Exception ex) { error.Text = "No se pudieron cargar las categorías: " + ex.Message; }
            finally { loading = false; type.Enabled = true; save.Enabled = true; }
        }

        private async Task Save()
        {
            if (loading || saving) return;
            decimal amount;
            name.HasError = string.IsNullOrWhiteSpace(name.Text);
            price.HasError = !decimal.TryParse(price.Text.Trim(), NumberStyles.AllowDecimalPoint,
                CultureInfo.GetCultureInfo("es-CO"), out amount) || amount <= 0 || decimal.Round(amount, 2) != amount;
            if (name.HasError || price.HasError || category.SelectedValue == null)
            { error.Text = "Completa nombre y categoría. Usa un precio positivo sin separadores de miles, con hasta dos decimales."; return; }
            int? id = original == null ? (int?)null : Convert.ToInt32(original["Id"]);
            bool isBeverage = beverage.Checked;
            bool? originalBeverage = original == null ? (bool?)null : Convert.ToString(original["Tipo"]) == "BEBIDA";
            int categoryId = Convert.ToInt32(category.SelectedValue);
            string productName = name.Text, details = description.Text;
            bool enabled = active.Checked, inMenu = menu.Checked, withIngredients = ingredients.Checked && !isBeverage;
            saving = true; save.Enabled = false; type.Enabled = false; UseWaitCursor = true;
            try
            {
                await Task.Run(() => NProductos.Guardar(id, isBeverage, productName, categoryId, amount, details, enabled, inMenu, originalBeverage, withIngredients));
                saving = false; DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) { error.Text = "No se pudo guardar: " + ex.Message; }
            finally { saving = false; save.Enabled = true; type.Enabled = true; UseWaitCursor = false; }
        }
    }
}
