using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui;

internal partial class SliderDrawable(Slider view) : IDrawable
{
    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        if (view.BackgroundColor != null)
        {
            canvas.FillColor = view.BackgroundColor;
            canvas.FillRectangle(rect);
        }

        // 1. Calculate Geometry
        float usableWidth = rect.Width - (view.ThumbRadius * 2);
        double range = view.Maximum - view.Minimum;
        float percent = range > 0 ? (float)((view.DrawValue - view.Minimum) / range) : 0f;

        float thumbX = rect.Left + view.ThumbRadius + (usableWidth * percent);
        float trackY = view.ShowValueBubble ? rect.Bottom - view.ThumbRadius - 8f : rect.Center.Y;

        var minColor = view.IsActive ? view.ActiveMinimumTrackColor : view.MinimumTrackColor;
        var thumbColor = view.IsActive ? view.ActiveThumbColor : view.ThumbColor;
        var maxColor = view.MaximumTrackColor;

        if (!view.IsEnabled)
        {
            minColor = minColor.WithAlpha(0.38f);
            thumbColor = thumbColor.WithAlpha(0.38f);
            maxColor = maxColor.WithAlpha(0.38f);
        }

        // 2. Draw Maximum Track (Unfilled)
        canvas.StrokeColor = maxColor;
        canvas.StrokeSize = view.TrackHeight;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(thumbX, trackY, rect.Right - view.ThumbRadius, trackY);

        // 3. Draw Minimum Track (Filled)
        canvas.StrokeColor = minColor;
        canvas.DrawLine(rect.Left + view.ThumbRadius, trackY, thumbX, trackY);

        // 4. Draw Thumb
        float currentThumbRadius = view.IsDragging ? view.ThumbRadius * 1.25f : view.ThumbRadius;

        canvas.SetShadow(new SizeF(0, 2), 4, Colors.Black.WithAlpha(0.3f));
        canvas.FillColor = thumbColor;
        canvas.FillCircle(thumbX, trackY, currentThumbRadius);
        canvas.SetShadow(SizeF.Zero, 0, Colors.Transparent);

        // 5. Draw Value Bubble (Clamped safely within canvas boundaries)
        if (view.IsDragging && view.ShowValueBubble)
        {
            DrawValueBubble(canvas, rect, thumbX, trackY, minColor);
        }

        canvas.RestoreState();
    }

    private void DrawValueBubble(ICanvas canvas, RectF bounds, float thumbX, float thumbY, Color bubbleColor)
    {
        string text = view.GetFormattedValue();

        var font = new Microsoft.Maui.Graphics.Font("Arial");
        canvas.Font = font;
        canvas.FontSize = 12f;
        var textSize = canvas.GetStringSize(text, font, 12f);

        float bubbleWidth = Math.Max(38f, textSize.Width + 16f);
        float bubbleHeight = 24f;

        float bubbleBottom = thumbY - view.ThumbRadius - 8f;
        float bubbleTop = bubbleBottom - bubbleHeight;

        // CLAMP: Prevents bubble from overflowing off-screen on the left or right!
        float rawLeft = thumbX - (bubbleWidth / 2f);
        float clampedLeft = Math.Clamp(rawLeft, bounds.Left, bounds.Right - bubbleWidth);

        var bubbleRect = new RectF(clampedLeft, bubbleTop, bubbleWidth, bubbleHeight);

        // Draw Pill
        canvas.FillColor = bubbleColor;
        canvas.FillRoundedRectangle(bubbleRect, bubbleHeight / 2f);

        // Draw Arrow pointing to thumb
        using var path = new PathF();
        path.Move(thumbX - 5f, bubbleBottom - 0.5f);
        path.LineTo(thumbX + 5f, bubbleBottom - 0.5f);
        path.LineTo(thumbX, bubbleBottom + 5.5f);
        path.Close();
        canvas.FillPath(path);

        // Draw Text inside the bubble
        canvas.FontColor = Colors.White;
        canvas.DrawString(text, bubbleRect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
}