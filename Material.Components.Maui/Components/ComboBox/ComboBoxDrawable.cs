namespace Material.Components.Maui;

internal partial class ComboBoxDrawable(ComboBox view) : IDrawable, IDisposable
{
    readonly PathF arrowDropDownIcon = PathBuilder.Build("M12,15 L7,10H17Z");
    readonly PathF arrowDropUpIcon = PathBuilder.Build("M7,14 L12,9 17,14Z");

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        var containerRect = view.ShowFloatingLabel
         ? new RectF(rect.Left, rect.Top + 8f * (rect.Height / 64f), rect.Width, rect.Height - 8f * (rect.Height / 64f))
         : rect;
        var scale = rect.Height / 64f;



        canvas.DrawBackground(view, containerRect);
        this.DrawItem(canvas, containerRect, scale);

        if (view.ShowDropDownArrow)
        {
            this.DrawDrapIcon(canvas, containerRect, scale);
        }
        this.DrawActiveIndicator(canvas, containerRect);
        
        if (view.ShowFloatingLabel && !string.IsNullOrEmpty(view.LabelText))
            this.DrawLabelText(canvas, containerRect, rect.Height / 64f);

        canvas.DrawOutline(view, containerRect);

        canvas.RestoreState();
    }
    void DrawItem(ICanvas canvas, RectF rect, float scale)
    {
        if (view.SelectedIndex >= 0 && view.SelectedIndex < view.Items.Count)
        {
            var selectedItemText = view.Items[view.SelectedIndex]?.Text ?? string.Empty;

            float leftPad = 16f * scale;
            float rightPad = view.ShowDropDownArrow ? (24f + 16f + 8f) * scale : 16f * scale;

            var textRect = new RectF
            {
                Left = rect.Left + leftPad,
                Width = Math.Max(0, rect.Width - leftPad - rightPad),
                Top = rect.Top,
                Height = rect.Height
            };

            var verticalAlign = (!view.ShowFloatingLabel || view.OutlineWidth != 0)
                ? VerticalAlignment.Center
                : VerticalAlignment.Bottom;

            canvas.DrawText(
                view,
                selectedItemText,
                textRect,
                HorizontalAlignment.Left,
                verticalAlign
            );
        }
    }

    void DrawDrapIcon(ICanvas canvas, RectF rect, float scale)
    {
        using var path = (
            view.IsDropDown ? this.arrowDropUpIcon : this.arrowDropDownIcon
        ).AsScaledPath(scale);
        var sx = rect.Right - 40f * scale;
        var sy = rect.Center.Y - 24f / 2 * scale;
        path.Move(sx, sy);
        canvas.FillColor = MaterialColors.OnSurface;
        canvas.FillPath(path);
    }

    private void DrawActiveIndicator(ICanvas canvas, RectF rect)
    {
        if (view.ActiveIndicatorHeight == 0)
            return;

        canvas.FillColor = view.ActiveIndicatorColor.MultiplyAlpha(
            view.ViewState is ViewState.Disabled ? 0.38f : 1f
        );

        canvas.FillRectangle(
            rect.Left,
            rect.Bottom - view.ActiveIndicatorHeight,
            rect.Width,
            view.ActiveIndicatorHeight
        );
    }

    void DrawLabelText(ICanvas canvas, RectF rect, float scale)
    {
        var percent =
            view.SelectedIndex != -1 || view.IsDropDown
                ? 1f - view.LabelAnimationPercent
                : view.LabelAnimationPercent;

        var fontSize = 12f + (view.FontSize - 12f) * percent;

        canvas.FontColor = view.LabelFontColor?.MultiplyAlpha(
            view.ViewState is ViewState.Disabled ? 0.38f : 1f
        );

        var labelSize = view.GetStringSize(view.LabelText, fontSize);
        var minLabelSize = view.GetStringSize(view.LabelText, 12f);
        var maxLabelSize = view.GetStringSize(view.LabelText);

        if (view.OutlineWidth == 0)
        {
            var minY = rect.Top + 8f * scale;
            var maxY = rect.Center.Y - maxLabelSize.Height / 2;

            var labelRect = new RectF
            {
                Left = rect.Left + 16f * scale,
                Top = minY + (maxY - minY) * percent,
                Width = rect.Width - (16f - 24f - 16f) * scale,
                Height = labelSize.Height
            };

            canvas.DrawText(
                view,
                view.LabelText,
                view.LabelFontColor,
                fontSize,
                labelRect,
                HorizontalAlignment.Left,
                VerticalAlignment.Center
            );
        }
        else
        {
            var minY = rect.Top - minLabelSize.Height / 2;
            var maxY = rect.Center.Y - maxLabelSize.Height / 2;

            var labelRect = new RectF
            {
                Left = rect.Left + 16f * scale,
                Top = minY + (maxY - minY) * percent,
                Width = rect.Width - (16f - 24f - 16f) * scale,
                Height = labelSize.Height
            };

            canvas.DrawText(
                view,
                view.LabelText,
                view.LabelFontColor,
                fontSize,
                labelRect,
                HorizontalAlignment.Left,
                VerticalAlignment.Center
            );

            var clipRect = new RectF
            {
                Left =
                    rect.Left
                    + 16f * scale
                    + minLabelSize.Width / 2
                    - (minLabelSize.Width / 2 + 4f * scale) * (1 - percent),
                Top = rect.Top - minLabelSize.Height / 2,
                Width = (minLabelSize.Width + 8f * scale) * (1 - percent),
                Height = minLabelSize.Height
            };

            //clip rect in outline
            canvas.SubtractFromClip(clipRect);
        }
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.arrowDropDownIcon.Dispose();
                this.arrowDropUpIcon.Dispose();
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
