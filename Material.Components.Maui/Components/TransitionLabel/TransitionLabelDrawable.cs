namespace Material.Components.Maui;

internal class TransitionLabelDrawable(TransitionLabel view) : IDrawable
{
    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;
        canvas.ClipPath(view.GetClipPath(rect));

        canvas.DrawBackground(view, rect);

        var font = new Microsoft.Maui.Graphics.Font(
            view.FontFamily,
            (int)view.FontWeight,
            view.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal
        );
        canvas.Font = font;
        canvas.FontSize = view.FontSize;

        var baseColor = view.FontColor ?? Colors.Black;
        float progress = view.TransitionProgress;
        float heightOffset = rect.Height * 0.75f;

        // 1. Draw Old Text (Exiting)
        if (!string.IsNullOrEmpty(view.OldText) && progress < 1f)
        {
            float oldAlpha = 1f - progress;
            canvas.FontColor = baseColor.WithAlpha(oldAlpha);

            float oldY = view.TransitionType switch
            {
                TextTransitionType.SlideUp => rect.Top - (progress * heightOffset),
                TextTransitionType.SlideDown => rect.Top + (progress * heightOffset),
                _ => rect.Top
            };

            canvas.DrawString(view.OldText, new RectF(rect.Left, oldY, rect.Width, rect.Height), HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // 2. Draw New Text (Entering)
        if (!string.IsNullOrEmpty(view.CurrentText))
        {
            float newAlpha = progress;
            canvas.FontColor = baseColor.WithAlpha(newAlpha);

            float newY = view.TransitionType switch
            {
                TextTransitionType.SlideUp => rect.Top + ((1f - progress) * heightOffset),
                TextTransitionType.SlideDown => rect.Top - ((1f - progress) * heightOffset),
                _ => rect.Top
            };

            canvas.DrawString(view.CurrentText, new RectF(rect.Left, newY, rect.Width, rect.Height), HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        canvas.RestoreState();
    }
}