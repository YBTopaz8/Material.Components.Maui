namespace Material.Components.Maui;

internal class CheckBoxDrawable(CheckBox view) : IDrawable
{
    readonly PathF checkPath = PathBuilder.Build("M9.55,18 L3.85,12.3 5.275,10.875 9.55,15.15 18.725,5.975 20.15,7.4Z");

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        float scale = rect.Height / 40f;
        float boxSize = 18f * scale;
        float gap = 8f * scale;
        var textSize = view.GetStringSize();

        float boxX = (view.LabelPlacement == LabelPlacement.End)
            ? rect.Left + 8f * scale
            : rect.Right - boxSize - (8f * scale);

        var boxRect = new RectF(boxX, rect.Center.Y - boxSize / 2f, boxSize, boxSize);

        // 1. Draw the CheckBox (Box + Content)
        if (view.IsChecked == true || view.IsChecked == null)
        {
            // Filled background for Checked and Indeterminate
            canvas.FillColor = view.BoxColor;
            canvas.FillRoundedRectangle(boxRect, 4f * scale);

            if (view.IsChecked == true)
            {
                // Draw Checkmark
                canvas.FillColor = view.CheckColor;
                using var path = this.checkPath.AsScaledPath(boxSize / 24f);
                path.Move(boxRect.X, boxRect.Y);
                canvas.FillPath(path);
            }
            else
            {
                // Draw Material 3 Indeterminate Dash (-)
                canvas.StrokeColor = view.CheckColor;
                canvas.StrokeSize = 2.5f * scale;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawLine(boxRect.Left + 4f * scale, boxRect.Center.Y, boxRect.Right - 4f * scale, boxRect.Center.Y);
            }
        }
        else
        {
            // Unchecked: Draw Border Outline
            canvas.StrokeColor = view.OutlineColor ?? Colors.Gray;
            canvas.StrokeSize = view.OutlineWidth > 0 ? view.OutlineWidth : 2f * scale;
            canvas.DrawRoundedRectangle(boxRect, 4f * scale);
        }

        // 2. Draw Text (if present)
        if (textSize.Width > 0)
        {
            float textX = (view.LabelPlacement == LabelPlacement.End)
                ? boxRect.Right + gap
                : rect.Left + 8f * scale;

            float textWidth = rect.Width - boxSize - gap - 16f * scale;
            var textRect = new RectF(textX, rect.Top, textWidth, rect.Height);

            canvas.Font = new Microsoft.Maui.Graphics.Font(view.FontFamily, (int)view.FontWeight, view.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal);
            canvas.FontSize = view.FontSize;
            canvas.FontColor = view.FontColor;
            canvas.DrawString(view.Text, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        canvas.RestoreState();
    }

    public void Dispose()
    {
        this.checkPath.Dispose();
    }
}
