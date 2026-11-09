namespace Material.Components.Maui;

internal class ProgressIndicatorDrawable(ProgressIndicator view) : IDrawable
{
    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        // Background
        canvas.ClipPath(view.GetClipPath(rect));
        canvas.DrawBackground(view, rect);

        var scale = view.IndicatorType is IndicatorType.Circular ? rect.Height / 48f : 1f;

        if (view.IndicatorType is IndicatorType.Circular)
        {
            if (view.Value == -1d)
                this.DrawCircularIndeterminateAnimation(canvas, rect, scale);
            else
                this.DrawCircularProgress(canvas, rect, scale);
        }
        else
        {
            if (view.Value == -1d)
                this.DrawLinearIndeterminateAnimation(canvas, rect);
            else
                this.DrawLinearProgress(canvas, rect);
        }

        // Draw Value Label if requested
        if (view.ShowValueLabel && view.Value != -1d)
        {
            this.DrawTextLabel(canvas, rect);
        }

        canvas.RestoreState();
    }

    void DrawCircularIndeterminateAnimation(ICanvas canvas, RectF rect, float scale)
    {
        var size = 36f * scale;
        var x = rect.Left + 6f * scale;
        var y = rect.Top + 6f * scale;
        var percent = view.DrawPercent;

        canvas.StrokeColor = view.ActiveIndicatorColor;
        canvas.StrokeSize = view.ActiveIndicatorHeight * scale;
        canvas.StrokeLineCap = LineCap.Round;

        if (view.AnimationIsPositive)
            canvas.DrawArc(x, y, size, size, 360f - 360f * percent, 360f - 360f * percent - 315f * percent, true, false);
        else
            canvas.DrawArc(x, y, size, size, 360f - 360f * (1 - percent) * 2, 360f - 360f * (1 - percent) * 2 - 315f * percent, true, false);
    }

    void DrawCircularProgress(ICanvas canvas, RectF rect, float scale)
    {
        var size = 36f * scale;
        var x = rect.Left + 6f * scale;
        var y = rect.Top + 6f * scale;
        var percent = view.DrawPercent;

        canvas.StrokeSize = view.ActiveIndicatorHeight * scale;
        canvas.StrokeLineCap = LineCap.Round;

        // Draw Track (Unfilled)
        if (view.TrackColor != Colors.Transparent)
        {
            canvas.StrokeColor = view.TrackColor;
            canvas.DrawArc(x, y, size, size, 0, 360, true, false);
        }

        // Draw Active Indicator (Filled)
        canvas.StrokeColor = view.ActiveIndicatorColor;
        if (percent < 1f)
            canvas.DrawArc(x, y, size, size, 90f, 90f - 360f * percent, true, false);
        else
            canvas.DrawCircle(rect.Center, size / 2);
    }

    private void DrawLinearIndeterminateAnimation(ICanvas canvas, RectF rect)
    {
        var percent = view.DrawPercent;
        var startX = rect.Width * percent - rect.Width * (1f - percent) * 0.5f + rect.Left;
        var endX = startX + rect.Width * 0.5f;

        canvas.StrokeColor = view.ActiveIndicatorColor;
        canvas.StrokeSize = view.ActiveIndicatorHeight;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(startX, rect.Center.Y, endX, rect.Center.Y);
    }

    private void DrawLinearProgress(ICanvas canvas, RectF rect)
    {
        var trackHeight = view.ActiveIndicatorHeight;
        var cornerRadius = trackHeight / 2f;
        var drawY = rect.Center.Y - cornerRadius;

        // Draw Track (Unfilled)
        if (view.TrackColor != Colors.Transparent)
        {
            canvas.FillColor = view.TrackColor;
            canvas.FillRoundedRectangle(rect.Left, drawY, rect.Width, trackHeight, cornerRadius);
        }

        // Draw Active Indicator (Filled)
        var endX = rect.Width * view.DrawPercent;
        if (endX > 0)
        {
            canvas.FillColor = view.ActiveIndicatorColor;
            canvas.FillRoundedRectangle(rect.Left, drawY, endX, trackHeight, cornerRadius);
        }
    }

    private void DrawTextLabel(ICanvas canvas, RectF rect)
    {
        // Format string, replacing {0} with the actual value
        string text = string.Format(view.ValueStringFormat ?? "{0:0}", view.Value);

        canvas.FontColor = view.ActiveIndicatorColor; // Could add a LabelFontColor property later if needed
        canvas.FontSize = 12f;

        if (view.IndicatorType == IndicatorType.Circular)
        {
            // Center in the circle
            canvas.DrawString(text, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        else
        {
            // Right-align above the bar, or center inside it if it's thick enough
            if (view.ActiveIndicatorHeight >= 14)
            {
                canvas.FontColor = Colors.White; // High contrast for inside bar
                canvas.DrawString(text, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }
}