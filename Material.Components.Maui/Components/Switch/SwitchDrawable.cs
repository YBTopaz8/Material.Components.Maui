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
        var scale = rect.Height / 40f;
        var drawRect = new RectF(4f * scale, 4f * scale, rect.Width - 8f * scale, 32f * scale);
        canvas.ClipPath(view.GetClipPath(drawRect));
        canvas.DrawBackground(view, drawRect);
        canvas.DrawOutline(view, drawRect);
       
        canvas.ResetState();

#if ANDROID
        canvas.Scale(canvas.DisplayScale, canvas.DisplayScale);
#endif
        this.DrawStateLayer(canvas, rect, scale);

        this.DrawThumb(canvas, drawRect, scale);
        var textSize = view.GetStringSize();
        if (textSize.Width > 0)
        {
            var textRect = new RectF(52f * scale + 8f * scale, 0, rect.Width - 60f * scale, rect.Height);
            canvas.DrawText(view, textRect, HorizontalAlignment.Left, VerticalAlignment.Center);
        }
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
