using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui.Components;

public partial class VinylRecordView
    : TouchGraphicsView,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IRippleElement,
        IDisposable
{
    public static readonly BindableProperty IsPlayingProperty = BindableProperty.Create(
        nameof(IsPlaying), typeof(bool), typeof(VinylRecordView), false,
        propertyChanged: (bo, ov, nv) => ((VinylRecordView)bo).OnIsPlayingChanged((bool)nv));

    public static readonly BindableProperty RPMProperty = BindableProperty.Create(
        nameof(RPM), typeof(double), typeof(VinylRecordView), 33.33); // 33 RPM default

    public static readonly BindableProperty CenterColorProperty = BindableProperty.Create(
        nameof(CenterColor), typeof(Color), typeof(VinylRecordView), Colors.Crimson,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public bool IsPlaying { get => (bool)GetValue(IsPlayingProperty); set => SetValue(IsPlayingProperty, value); }
    public double RPM { get => (double)GetValue(RPMProperty); set => SetValue(RPMProperty, value); }
    public Color CenterColor { get => (Color)GetValue(CenterColorProperty); set => SetValue(CenterColorProperty, value); }

    public event EventHandler<double>? Scratched; // Returns delta angle

    internal float CurrentRotationAngle { get; set; } = 0f;

    private IDispatcherTimer? _rotationTimer;
    private PointF _lastDragPoint;

    public VinylRecordView()
    {
        this.SetDynamicResource(StyleProperty, "StandardVinylRecordStyle");
        this.Drawable = new VinylRecordDrawable(this);

        this.StartInteraction += OnTouchStart;
        this.DragInteraction += OnTouchDrag;
    }

    private void OnTouchStart(object? sender, TouchEventArgs e)
    {
        _lastDragPoint = e.Touches[0];
    }

    private void OnTouchDrag(object? sender, TouchEventArgs e)
    {
        var current = e.Touches[0];
        var center = new PointF((float)this.Bounds.Width / 2f, (float)this.Bounds.Height / 2f);

        // Calculate angular delta (Vinyl Scratching!)
        double prevAngle = Math.Atan2(_lastDragPoint.Y - center.Y, _lastDragPoint.X - center.X);
        double newAngle = Math.Atan2(current.Y - center.Y, current.X - center.X);
        double deltaDegrees = (newAngle - prevAngle) * (180.0 / Math.PI);

        this.CurrentRotationAngle = (float)((this.CurrentRotationAngle + deltaDegrees) % 360.0);
        _lastDragPoint = current;

        this.Invalidate();
        Scratched?.Invoke(this, deltaDegrees);
    }

    private void OnIsPlayingChanged(bool isPlaying)
    {
        if (isPlaying) StartSpinning();
        else StopSpinning();
    }

    private void StartSpinning()
    {
        _rotationTimer?.Stop();
        _rotationTimer = Dispatcher.CreateTimer();
        _rotationTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        _rotationTimer.Tick += (s, e) =>
        {
            if (this.isTouching) return; // Don't fight user's finger while scratching!

            // Degrees per frame: (RPM * 360) / (60 * 60)
            float degreesPerFrame = (float)((this.RPM * 360.0) / 3600.0);
            this.CurrentRotationAngle = (this.CurrentRotationAngle + degreesPerFrame) % 360f;
            this.Invalidate();
        };
        _rotationTimer.Start();
    }

    private void StopSpinning()
    {
        _rotationTimer?.Stop();
        _rotationTimer = null;
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        double size = Math.Min(
            double.IsInfinity(widthConstraint) ? 200 : widthConstraint,
            double.IsInfinity(heightConstraint) ? 200 : heightConstraint
        );

        this.DesiredSize = new Size(size, size);
        return this.DesiredSize;
    }

    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            StopSpinning();
            this.StartInteraction -= OnTouchStart;
            this.DragInteraction -= OnTouchDrag;
        }
        base.Dispose(disposing);
    }
}
