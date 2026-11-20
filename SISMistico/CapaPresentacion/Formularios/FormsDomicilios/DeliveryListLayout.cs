using System;
using System.Drawing;
using System.Windows.Forms;
using CapaPresentacion.UI;

namespace CapaPresentacion.Formularios.FormsDomicilios
{
    // Dos lineas y dos acciones de 44px: detalle y menu. El resto de acciones vive en el menu tactil.
    internal static class DeliveryListLayout
    {
        public const int Height = 72;
        public const int ActionsWidth = 104;
        public static readonly TextFormatFlags TextFlags = TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

        public static string Time(TimeSpan remaining)
        {
            int seconds = (int)Math.Ceiling(Math.Max(0, remaining.TotalSeconds));
            return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public static int DrawContact(Graphics graphics, string phone, Rectangle bounds)
        {
            if (string.IsNullOrWhiteSpace(phone)) return 0;
            if (graphics != null && bounds.Width > 18)
            {
                LucideIcons.Draw(graphics, "phone", Theme.Lighten(Theme.TextMuted, 0.2f),
                    new Rectangle(bounds.X, bounds.Y + (bounds.Height - 12) / 2, 12, 12));
                TextRenderer.DrawText(graphics, phone.Trim(), Theme.Small,
                    new Rectangle(bounds.X + 18, bounds.Y, bounds.Width - 18, bounds.Height), Theme.TextMuted, TextFlags);
            }
            return bounds.Height;
        }

        public static void Draw(Graphics graphics, int width, Color accent, string number, string timer,
            string customer, string summary, string phone = null, string headerContact = null,
            int rowHeight = Height, bool moveTimerToSummary = true, string numberBadge = null, bool headerPhoneIcon = false)
        {
            if (graphics == null) return;
            int right = width - ActionsWidth - 8;
            bool hasContact = !string.IsNullOrWhiteSpace(headerContact);
            string rightText = hasContact ? headerContact : timer;
            int availableHeader = Math.Max(10, right - 18);
            int rightTextWidth = TextRenderer.MeasureText(rightText, Theme.SmallBold).Width + 4;
            if (hasContact && headerPhoneIcon) rightTextWidth = TextRenderer.MeasureText(rightText.Trim(), Theme.Small).Width + 20;
            if (hasContact && rightTextWidth > availableHeader - 52)
            {
                // En columnas estrechas priorizar el numero de contacto; el rotulo se conserva en las fichas.
                rightText = headerContact.Replace("Contacto: ", string.Empty);
                rightTextWidth = TextRenderer.MeasureText(rightText, headerPhoneIcon ? Theme.Small : Theme.SmallBold).Width + (headerPhoneIcon ? 20 : 4);
            }
            int timeWidth = Math.Min(hasContact ? Math.Max(10, availableHeader - 52) : 74, rightTextWidth);
            using (var stripe = new SolidBrush(accent)) graphics.FillRectangle(stripe, 0, 10, 3, rowHeight - 20);
            using (var line = new Pen(Theme.Border)) graphics.DrawLine(line, 10, rowHeight - 1, width - 8, rowHeight - 1);
            int numberLeft = 12;
            int numberWidth = Math.Max(10, right - 18 - timeWidth);
            if (!string.IsNullOrWhiteSpace(numberBadge))
            {
                int badgeWidth = Math.Min(TextRenderer.MeasureText(numberBadge, Theme.SmallBold).Width + 14,
                    Math.Max(24, numberWidth - 20));
                var badge = new Rectangle(numberLeft, 10, badgeWidth, 22);
                using (var path = Theme.RoundedPath(badge, 7))
                using (var fill = new SolidBrush(Theme.Lighten(accent, 0.88f))) graphics.FillPath(fill, path);
                TextRenderer.DrawText(graphics, numberBadge, Theme.SmallBold, badge, Theme.Darken(accent, 0.2f),
                    TextFlags | TextFormatFlags.HorizontalCenter);
                numberLeft += badgeWidth + 8;
                numberWidth = Math.Max(10, numberWidth - badgeWidth - 8);
            }
            TextRenderer.DrawText(graphics, number, Theme.SmallBold, new Rectangle(numberLeft, 10, numberWidth, 22), Theme.TextPrimary, TextFlags);
            if (hasContact && headerPhoneIcon)
                DrawContact(graphics, rightText, new Rectangle(right - timeWidth, 10, timeWidth, 22));
            else
                TextRenderer.DrawText(graphics, rightText, Theme.SmallBold, new Rectangle(right - timeWidth, 10, timeWidth, 22), hasContact ? Theme.TextMuted : accent, TextFlags | TextFormatFlags.Right);
            if (hasContact && moveTimerToSummary) summary = timer;
            int summaryWidth = string.IsNullOrEmpty(summary) ? 0 : Math.Min(85, TextRenderer.MeasureText(summary, Theme.Small).Width + 4);
            int customerWidth = Math.Max(10, right - 18 - summaryWidth);
            if (string.IsNullOrWhiteSpace(phone))
                TextRenderer.DrawText(graphics, customer, Theme.Small, new Rectangle(12, 35, customerWidth, 22), Theme.TextMuted, TextFlags);
            else
            {
                // Reservar el telefono antes de recortar un nombre largo; nunca inventar un contacto.
                string contact = (string.IsNullOrWhiteSpace(customer) ? string.Empty : " - ") + phone.Trim();
                int phoneWidth = Math.Min(customerWidth - 12, TextRenderer.MeasureText(contact, Theme.Small, Size.Empty,
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + 2);
                int nameWidth = Math.Min(Math.Max(0, customerWidth - phoneWidth), TextRenderer.MeasureText(customer ?? string.Empty,
                    Theme.Small, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width);
                TextRenderer.DrawText(graphics, customer, Theme.Small, new Rectangle(12, 35, nameWidth, 22), Theme.TextMuted, TextFlags);
                TextRenderer.DrawText(graphics, contact, Theme.Small, new Rectangle(12 + nameWidth, 35, customerWidth - nameWidth, 22), Theme.TextMuted, TextFlags);
            }
            if (summaryWidth > 0)
                TextRenderer.DrawText(graphics, summary, Theme.Small, new Rectangle(right - summaryWidth, 35, summaryWidth, 22), hasContact && moveTimerToSummary ? accent : Theme.TextMuted, TextFlags | TextFormatFlags.Right);
        }

        public static void DrawFinished(Graphics graphics, int width, Color accent, string badgeText,
            string number, string customer, string phone, string address, string stage)
        {
            if (graphics == null) return;
            int right = width - ActionsWidth - 8;
            using (var stripe = new SolidBrush(accent)) graphics.FillRectangle(stripe, 0, 10, 3, Height - 20);
            using (var line = new Pen(Theme.Border)) graphics.DrawLine(line, 10, Height - 1, width - 8, Height - 1);
            int x = 12;
            if (!string.IsNullOrWhiteSpace(badgeText))
            {
                int badgeWidth = Math.Min(TextRenderer.MeasureText(badgeText, Theme.SmallBold).Width + 14, Math.Max(32, (right - 12) / 3));
                var badge = new Rectangle(x, 10, badgeWidth, 22);
                using (var path = Theme.RoundedPath(badge, 7))
                using (var fill = new SolidBrush(Theme.Lighten(Theme.Primary, 0.9f))) graphics.FillPath(fill, path);
                TextRenderer.DrawText(graphics, badgeText, Theme.SmallBold, badge, Theme.Darken(Theme.Primary, 0.15f),
                    TextFlags | TextFormatFlags.HorizontalCenter);
                x += badgeWidth + 8;
            }
            int orderWidth = Math.Min(TextRenderer.MeasureText(number, Theme.SmallBold).Width + 2, Math.Max(30, right - x - 44));
            TextRenderer.DrawText(graphics, number, Theme.SmallBold, new Rectangle(x, 10, orderWidth, 22), Theme.TextPrimary, TextFlags);
            x += orderWidth + 8;
            TextRenderer.DrawText(graphics, customer, Theme.Small, new Rectangle(x, 10, Math.Max(10, right - x), 22), Theme.TextPrimary, TextFlags);

            int stageWidth = string.IsNullOrEmpty(stage) ? 0 : Math.Min(85, TextRenderer.MeasureText(stage, Theme.Small).Width + 4);
            int contactRight = right - (stageWidth > 0 ? stageWidth + 8 : 0);
            x = 12;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                int phoneWidth = Math.Min(TextRenderer.MeasureText(phone.Trim(), Theme.Small).Width + 20, Math.Max(20, contactRight - x));
                DrawContact(graphics, phone, new Rectangle(x, 35, phoneWidth, 22));
                x += phoneWidth + 8;
            }
            if (!string.IsNullOrWhiteSpace(address) && contactRight > x)
                TextRenderer.DrawText(graphics, address, Theme.Small, new Rectangle(x, 35, contactRight - x, 22), Theme.TextMuted, TextFlags);
            if (stageWidth > 0)
                TextRenderer.DrawText(graphics, stage, Theme.Small, new Rectangle(right - stageWidth, 35, stageWidth, 22), accent, TextFlags | TextFormatFlags.Right);
        }

        public static ToolStripMenuItem Action(string text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(text) { AutoSize = false, Size = new Size(230, 44), Font = Theme.Body };
            item.Click += handler;
            return item;
        }
    }
}
