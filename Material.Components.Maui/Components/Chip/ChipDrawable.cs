namespace Material.Components.Maui;

internal partial class ChipDrawable(Chip view) : IDrawable, IDisposable
{
    readonly PathF closePath = PathBuilder.Build(
        "M6.4,19 L5,17.6 10.6,12 5,6.4 6.4,5 12,10.6 17.6,5 19,6.4 13.4,12 19,17.6 17.6,19 12,13.4Z"
    );
    readonly PathF checkPath = PathBuilder.Build("M9.55,18 L3.85,12.3 5.275,10.875 9.55,15.15 18.725,5.975 20.15,7.4Z");

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;
        canvas.ClipPath(view.GetClipPath(rect));

        canvas.DrawBackground(view, rect);
        canvas.DrawOutline(view, rect);
        canvas.DrawOverlayLayer(view, rect);

        if (view.RipplePercent is not 0f and not 1f)
            canvas.DrawRipple(view, view.LastTouchPoint, view.RippleSize, view.RipplePercent);
        else
            canvas.DrawStateLayer(view, rect, view.ViewState);

        var scale = rect.Height / 32f;
        float leftPadding = 12f * scale;
        float indicatorSize = 18f * scale;
        float currentX = rect.Left + leftPadding;

        // 1. Draw Indicator based on ChipType & State
        if (view.ChipType == ChipType.Filter)
        {
            // Checked State: Draw Checkmark
            if (view.IsChecked == true)
            {
                canvas.FillColor = view.IconColor ?? view.FontColor;
                using var path = this.checkPath.AsScaledPath(indicatorSize / 24f);
                path.Move(currentX, rect.Center.Y - indicatorSize / 2f);
                canvas.FillPath(path);
                currentX += indicatorSize + (6f * scale);
            }
            // Indeterminate State: Draw Dot •
            else if (view.IsChecked == null)
            {
                canvas.FillColor = view.IndeterminateColor;
                canvas.FillCircle(currentX + indicatorSize / 2f, rect.Center.Y, 4f * scale);
                currentX += indicatorSize + (6f * scale);
            }
            // Unchecked: Draw leading icon if specified
            else if (!string.IsNullOrEmpty(view.IconData))
            {
                canvas.DrawIcon(view, new RectF(currentX, rect.Center.Y - indicatorSize / 2f, indicatorSize, indicatorSize), 18, scale);
                currentX += indicatorSize + (6f * scale);
            }
        }
        else if (!string.IsNullOrEmpty(view.IconData))
        {
            canvas.DrawIcon(view, new RectF(currentX, rect.Center.Y - indicatorSize / 2f, indicatorSize, indicatorSize), 18, scale);
            currentX += indicatorSize + (6f * scale);
        }

        // 2. Draw Text
        float closeButtonWidth = (view.HasCloseButton || view.ChipType == ChipType.Input) ? (24f * scale) : 0f;
        float textWidth = rect.Right - currentX - closeButtonWidth - (12f * scale);

        if (!string.IsNullOrEmpty(view.Text) && textWidth > 0)
        {
            canvas.Font = new Microsoft.Maui.Graphics.Font(view.FontFamily, (int)view.FontWeight, view.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal);
            canvas.FontSize = view.FontSize;
            canvas.FontColor = view.FontColor.WithAlpha(view.ViewState is ViewState.Disabled ? 0.38f : 1f);
            canvas.DrawString(view.Text, new RectF(currentX, rect.Top, textWidth, rect.Height), HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        // 3. Draw Close Button (for Input Chips)
        if (view.HasCloseButton || view.ChipType == ChipType.Input)
        {
            var closeRect = new RectF(rect.Right - 24f * scale, rect.Center.Y - indicatorSize / 2f, indicatorSize, indicatorSize);
            canvas.FillColor = view.IconColor.WithAlpha(view.ViewState is ViewState.Disabled ? 0.38f : 1f);
            using var path = this.closePath.AsScaledPath(indicatorSize / 24f);
            path.Move(closeRect.X, closeRect.Y);
            canvas.FillPath(path);
        }

        canvas.RestoreState();
    }
    void DrawCloseIcon(ICanvas canvas, IIconElement element, RectF rect, float scale)
    {
        canvas.FillColor = element.IconColor.WithAlpha(
            element.ViewState is ViewState.Disabled ? 0.38f : 1f
        );

        var path = this.closePath.AsScaledPath(18f / 24f * scale);
        var sx = rect.Center.X - 18f / 2 * scale;
        var sy = rect.Center.Y - 18f / 2 * scale;
        path.Move(sx, sy);
        canvas.FillPath(path);
    }


    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.closePath?.Dispose();
                this.checkPath?.Dispose();
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
