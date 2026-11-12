namespace Material.Components.Maui;

internal partial class SegmentedButtonDrawable(SegmentedButton view) : IDrawable, IDisposable
{
    readonly PathF markPath = PathBuilder.Build(
        "M9.55,18 L3.85,12.3 5.275,10.875 9.55,15.15 18.725,5.975 20.15,7.4Z"
    );

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (view.Items.Count == 0) return;

        canvas.Antialias = true;
        var itemWidth = rect.Width / view.Items.Count;

        // 1. Draw each segment
        for (var i = 0; i < view.Items.Count; i++)
        {
            var itm = view.Items[i];
            if (itm == null) continue;

            this.DrawItem(canvas, new RectF(rect.Left + (itemWidth * i), rect.Top, itemWidth, rect.Height), itm);
        }

        // 2. Draw outer outline
        canvas.SaveState();
        canvas.DrawOutline(view, rect);

        // 3. Draw inner segment dividers
        if (view.OutlineWidth > 0 && view.OutlineColor != null)
        {
            canvas.StrokeSize = view.OutlineWidth;
            canvas.StrokeColor = view.OutlineColor;

            for (var i = 0; i < view.Items.Count - 1; i++)
            {
                var x = rect.Left + (itemWidth * (i + 1));
                canvas.DrawLine(x - view.OutlineWidth / 2f, rect.Top, x - view.OutlineWidth / 2f, rect.Bottom);
            }
        }
        canvas.RestoreState();
    }

    void DrawItem(ICanvas canvas, RectF rect, SegmentedItem item)
    {
        canvas.SaveState();

        // SINGLE CLIP FOR THIS SEGMENT
        this.ClipItemRect(canvas, rect, item);

        // 1. Background
        if (item.IsSelected && view.SelectedBackgroundColor != null)
        {
            canvas.FillColor = view.SelectedBackgroundColor;
            canvas.FillRectangle(rect);
        }
        else
        {
            canvas.DrawBackground(item, rect);
        }

        // 2. Ripple & State Layer
        if (rect.Contains(view.LastTouchPoint) && item.ViewState is ViewState.Hovered or ViewState.Pressed && view.RipplePercent is not (0f or 1f))
        {
            canvas.DrawRipple(view, view.LastTouchPoint, view.RippleSize / 3, view.RipplePercent);
        }
        else
        {
            canvas.DrawStateLayer(item, rect, item.ViewState);
        }

        // 3. Text & Icons Setup
        var scale = rect.Height / 40f;
        var hasText = !string.IsNullOrEmpty(item.Text);
        var hasIcon = !string.IsNullOrEmpty(item.IconData);
        var showCheck = item.IsSelected && view.MultiSelectMode;

        var currentFontColor = (item.IsSelected && view.SelectedFontColor != null)
            ? view.SelectedFontColor : (item.FontColor ?? view.FontColor);

        if (currentFontColor is not null)
            canvas.FontColor = currentFontColor.WithAlpha(view.ViewState is ViewState.Disabled ? 0.38f : 1f);

        // Calculate Text Size Early for Centering Math
        var actualFontSize = item.FontSize > 0 ? item.FontSize : view.FontSize;
        float textW = hasText ? view.GetStringSize(item.Text).Width : 0;

        float contentLeft = rect.Left;

        // 4. Layout Icon and Text precisely!
        if (showCheck || hasIcon)
        {
            var baseIconSize = 18f * scale;
            var activeIconSize = showCheck ? (baseIconSize * item.CheckAnimationPercent) : baseIconSize;
            var gap = 8f * scale * (showCheck ? item.CheckAnimationPercent : 1f);

            // Calculate total width of the icon + gap + text to center the whole block
            float totalBlockWidth = activeIconSize + (hasText ? gap + textW : 0);

            // Find perfect starting X coordinate
            float startX = rect.Left + (rect.Width / 2f) - (totalBlockWidth / 2f);

            var iconRect = new RectF(
                startX,
                rect.Center.Y - activeIconSize / 2f,
                activeIconSize,
                activeIconSize
            );

            if (showCheck && activeIconSize > 0)
            {
                canvas.FillColor = currentFontColor;
                using var path = this.markPath.AsScaledPath(activeIconSize / 24f);
                path.Move(iconRect.X, iconRect.Y);
                canvas.FillPath(path);
            }
            else if (hasIcon)
            {
                canvas.DrawIcon(item, iconRect, 18, scale);
            }

            // Text starts right after the icon + gap
            contentLeft = iconRect.Right + gap;
        }
        else
        {
            // No Icon. Just center the text perfectly!
            contentLeft = rect.Left + (rect.Width / 2f) - (textW / 2f);
        }

        // 5. Draw Text
        if (hasText)
        {
            var safeTextHeight = actualFontSize + 2f;

            // FAKE WIDTH 10,000 prevents wrapping. 
            var textRect = new RectF(contentLeft, rect.Center.Y - (safeTextHeight / 2f), 10000, safeTextHeight);

            var font = new Microsoft.Maui.Graphics.Font(
                item.FontFamily ?? view.FontFamily,
                (int)(item.FontWeight),
                item.FontIsItalic || view.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal
            );

            canvas.Font = font;
            canvas.FontSize = actualFontSize;

            // ALWAYS USE LEFT ALIGNMENT! (We already calculated the exact X coordinate in 'contentLeft')
            canvas.DrawString(item.Text, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        canvas.RestoreState();
    }

    void ClipItemRect(ICanvas canvas, RectF rect, SegmentedItem item)
    {
        using var path = new PathF();
        rect = new RectF(
            rect.X + view.OutlineWidth / 2f,
            rect.Y + view.OutlineWidth / 2f,
            rect.Width - view.OutlineWidth,
            rect.Height - view.OutlineWidth
        );

        var index = view.Items.IndexOf(item);
        var radii = view.GetRadii(rect.Width, rect.Height);

        if (view.Items.Count == 1)
            path.AppendRoundedRectangle(rect, (float)radii[0], (float)radii[1], (float)radii[2], (float)radii[3], true);
        else if (index == 0)
            path.AppendRoundedRectangle(rect, (float)radii[0], 0, (float)radii[2], 0, true);
        else if (index == view.Items.Count - 1)
            path.AppendRoundedRectangle(rect, 0, (float)radii[1], 0, (float)radii[3], true);
        else
            path.AppendRoundedRectangle(rect, 0, 0, 0, 0, true);

        canvas.ClipPath(path);
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.markPath.Dispose();
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