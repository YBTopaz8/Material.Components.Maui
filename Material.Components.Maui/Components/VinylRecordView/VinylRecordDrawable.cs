namespace Material.Components.Maui;

internal class VinylRecordDrawable(VinylRecordView view) : IDrawable
{
    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        float radius = Math.Min(rect.Width, rect.Height) / 2f;
        PointF center = rect.Center;

        // ROTATION MATRIX
        canvas.Rotate(view.CurrentRotationAngle, center.X, center.Y);

        // 1. Vinyl Body (Matte Black)
        canvas.FillColor = Color.FromArgb("#121212");
        canvas.FillCircle(center, radius);

        // 2. Concentric Grooves
        canvas.StrokeColor = Color.FromArgb("#222222");
        canvas.StrokeSize = 1f;

        for (float r = radius * 0.45f; r < radius * 0.95f; r += 6f)
        {
            canvas.DrawCircle(center, r);
        }

        // 3. Realistic Sheen / Light Reflection Arcs
        canvas.StrokeColor = Colors.White.WithAlpha(0.04f);
        canvas.StrokeSize = 12f;
        canvas.DrawArc(center.X - radius * 0.7f, center.Y - radius * 0.7f, radius * 1.4f, radius * 1.4f, 30, 80, false, false);
        canvas.DrawArc(center.X - radius * 0.7f, center.Y - radius * 0.7f, radius * 1.4f, radius * 1.4f, 210, 80, false, false);

        // 4. Center Label (Album Art / Sticker)
        float labelRadius = radius * 0.38f;
        canvas.FillColor = view.CenterColor;
        canvas.FillCircle(center, labelRadius);

        // 5. Spindle Hole (Center cutout)
        float holeRadius = radius * 0.06f;
        canvas.FillColor = Colors.White;
        canvas.FillCircle(center, holeRadius);
        canvas.StrokeColor = Color.FromArgb("#444444");
        canvas.StrokeSize = 1.5f;
        canvas.DrawCircle(center, holeRadius);

        canvas.RestoreState();
    }
}