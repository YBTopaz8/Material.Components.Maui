#if ANDROID
using Android.Content.Res;
using Android.Views;
using Material.Components.Maui.Primitives;
#endif


namespace Material.Components.Maui;

internal class SwitchDrawable(Switch view) : IDrawable
{
    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        float scale = rect.Height / 40f;
        float trackWidth = 52f * scale;
        float trackHeight = 32f * scale;
        float gap = 8f * scale;
        var textSize = view.GetStringSize();

        // Determine layout positions based on LabelPlacement
        float trackX = (view.LabelPlacement == LabelPlacement.End)
            ? rect.Left + (4f * scale)
            : rect.Right - trackWidth - (4f * scale);

        var drawRect = new RectF(trackX, rect.Center.Y - trackHeight / 2f, trackWidth, trackHeight);

        // 1. Draw Track
        canvas.ClipPath(view.GetClipPath(drawRect));
        canvas.DrawBackground(view, drawRect);
        canvas.DrawOutline(view, drawRect);
        canvas.RestoreState();

        canvas.SaveState();
        // 2. Draw Thumb
        this.DrawThumb(canvas, drawRect, scale);

        // 3. Draw Text
        if (textSize.Width > 0)
        {
            float textX = (view.LabelPlacement == LabelPlacement.End)
                ? drawRect.Right + gap
                : rect.Left + (4f * scale);

            float textWidth = rect.Width - trackWidth - gap - (8f * scale);
            var textRect = new RectF(textX, rect.Top, textWidth, rect.Height);

            canvas.Font = new Microsoft.Maui.Graphics.Font(view.FontFamily, (int)view.FontWeight, view.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal);
            canvas.FontSize = view.FontSize;
            canvas.FontColor = view.FontColor;
            canvas.DrawString(view.Text, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }

        canvas.RestoreState();
    }

    void DrawStateLayer(ICanvas canvas, RectF rect, float scale)
    {
        canvas.SaveState();

        var drawRect = new PathF();
        var x = view.IsSelected ? rect.Width - 20f * scale : 20f * scale;
        drawRect.AppendCircle(x, rect.Center.Y, rect.Height / 2f);
        canvas.ClipPath(drawRect);
        canvas.DrawStateLayer(view, rect, view.ViewState);

        canvas.RestoreState();
    }

    void DrawThumb(ICanvas canvas, RectF rect, float scale)
    {
        var thumbSize =
            (
                view.ViewState is ViewState.Pressed
                    ? 28f
                    : view.IsSelected || !string.IsNullOrEmpty(view.IconData)
                        ? 24f
                        : 16f
            ) * scale;

        var x =
            (view.IsSelected ? rect.Width - 16f * scale : 16f * scale)
            - thumbSize / 2f
            + rect.X;
        var thumbRect = new RectF(x, 16f * scale - thumbSize / 2f + rect.Y, thumbSize, thumbSize);

        canvas.FillColor = view.ThumbColor;
        canvas.FillCircle(thumbRect.Center, thumbSize / 2);

        if (!string.IsNullOrEmpty(view.IconData))
            canvas.DrawIcon(view, thumbRect, 16, scale);
    }
}
