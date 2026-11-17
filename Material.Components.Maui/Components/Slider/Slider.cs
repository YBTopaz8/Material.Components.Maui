using Microsoft.Maui.Animations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using ValueChangedEventArgs = Material.Components.Maui.Primitives.ValueChangedEventArgs;
namespace Material.Components.Maui;

public partial class Slider : TouchGraphicsView, IElement, IBackgroundElement
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new ViewState ViewState => ViewState.Normal;
    void IElement.OnPropertyChanged() => Invalidate();

    // ==========================================
    // CORE PROPERTIES
    // ==========================================
    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(Slider), 0.0, propertyChanged: RequestRedraw);

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(Slider), 100.0, propertyChanged: RequestRedraw);

    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(Slider), 0.0, BindingMode.TwoWay, propertyChanged: OnValuePropertyChanged);

    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(double), typeof(Slider), 0.0);

    public static readonly BindableProperty AnimatedProgressProperty = BindableProperty.Create(
    nameof(AnimatedProgress), typeof(bool), typeof(Slider), true);

    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration), typeof(uint), typeof(Slider), 250u);

    public bool AnimatedProgress { get => (bool)GetValue(AnimatedProgressProperty); set => SetValue(AnimatedProgressProperty, value); }
    public uint AnimationDuration { get => (uint)GetValue(AnimationDurationProperty); set => SetValue(AnimationDurationProperty, value); }

    internal double DrawValue { get; private set; }
    private new IAnimationManager? animationManager;
    // ==========================================
    // VISUAL PROPERTIES
    // ==========================================
    public static readonly BindableProperty MinimumTrackColorProperty = BindableProperty.Create(
        nameof(MinimumTrackColor), typeof(Color), typeof(Slider), Colors.Gray, propertyChanged: RequestRedraw);

    public static readonly BindableProperty MaximumTrackColorProperty = BindableProperty.Create(
        nameof(MaximumTrackColor), typeof(Color), typeof(Slider), Colors.LightGray, propertyChanged: RequestRedraw);

    public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(
        nameof(ThumbColor), typeof(Color), typeof(Slider), Colors.Gray, propertyChanged: RequestRedraw);

    public static readonly BindableProperty TrackHeightProperty = BindableProperty.Create(
        nameof(TrackHeight), typeof(float), typeof(Slider), 4f, propertyChanged: RequestRedraw);

    public static readonly BindableProperty ThumbRadiusProperty = BindableProperty.Create(
        nameof(ThumbRadius), typeof(float), typeof(Slider), 8f, propertyChanged: RequestRedraw);

    // ==========================================
    // "IS ACTIVE" (MUSIC PLAYING) PROPERTIES
    // ==========================================
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(Slider), false, propertyChanged: RequestRedraw);

    public static readonly BindableProperty ActiveMinimumTrackColorProperty = BindableProperty.Create(
        nameof(ActiveMinimumTrackColor), typeof(Color), typeof(Slider), Colors.DodgerBlue, propertyChanged: RequestRedraw);

    public static readonly BindableProperty ActiveThumbColorProperty = BindableProperty.Create(
        nameof(ActiveThumbColor), typeof(Color), typeof(Slider), Colors.DodgerBlue, propertyChanged: RequestRedraw);

    // ==========================================
    // BEHAVIOR PROPERTIES
    // ==========================================
    public static readonly BindableProperty UpdateValueOnReleaseProperty = BindableProperty.Create(
        nameof(UpdateValueOnRelease), typeof(bool), typeof(Slider), true);

    public static readonly BindableProperty ShowValueBubbleProperty = BindableProperty.Create(
        nameof(ShowValueBubble), typeof(bool), typeof(Slider), true);
    public static readonly BindableProperty FormatTypeProperty = BindableProperty.Create(
      nameof(FormatType), typeof(SliderFormatType), typeof(Slider), SliderFormatType.Custom, propertyChanged: RequestRedraw);
    public static readonly BindableProperty ValueBubbleFormatProperty = BindableProperty.Create(
        nameof(ValueBubbleFormat), typeof(string), typeof(Slider), "{0:F0}");
   
    // ==========================================
    // MVVM COMMAND PROPERTIES
    // ==========================================
    public static readonly BindableProperty DragCompletedCommandProperty = BindableProperty.Create(
        nameof(DragCompletedCommand), typeof(ICommand), typeof(Slider), null);

    public static readonly BindableProperty DragCompletedCommandParameterProperty = BindableProperty.Create(
        nameof(DragCompletedCommandParameter), typeof(object), typeof(Slider), null);
    // EVENTS
    public event EventHandler? DragStarted;
    public event EventHandler<SliderDragCompletedEventArgs>? DragCompleted;
    public event EventHandler<ValueChangedEventArgs>? ValueChanged;
    public event EventHandler<SliderFormatLabelEventArgs>? FormatValueBubble;

    // ACCESSORS
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Color MinimumTrackColor { get => (Color)GetValue(MinimumTrackColorProperty); set => SetValue(MinimumTrackColorProperty, value); }
    public Color MaximumTrackColor { get => (Color)GetValue(MaximumTrackColorProperty); set => SetValue(MaximumTrackColorProperty, value); }
    public Color ThumbColor { get => (Color)GetValue(ThumbColorProperty); set => SetValue(ThumbColorProperty, value); }
    public float TrackHeight { get => (float)GetValue(TrackHeightProperty); set => SetValue(TrackHeightProperty, value); }
    public float ThumbRadius { get => (float)GetValue(ThumbRadiusProperty); set => SetValue(ThumbRadiusProperty, value); }
    public bool IsActive { get => (bool)GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public Color ActiveMinimumTrackColor { get => (Color)GetValue(ActiveMinimumTrackColorProperty); set => SetValue(ActiveMinimumTrackColorProperty, value); }
    public Color ActiveThumbColor { get => (Color)GetValue(ActiveThumbColorProperty); set => SetValue(ActiveThumbColorProperty, value); }
    public bool UpdateValueOnRelease { get => (bool)GetValue(UpdateValueOnReleaseProperty); set => SetValue(UpdateValueOnReleaseProperty, value); }
    public bool ShowValueBubble { get => (bool)GetValue(ShowValueBubbleProperty); set => SetValue(ShowValueBubbleProperty, value); }
    public string ValueBubbleFormat { get => (string)GetValue(ValueBubbleFormatProperty); set => SetValue(ValueBubbleFormatProperty, value); }
    public new Color BackgroundColor { get => (Color)GetValue(IBackgroundElement.BackgroundColorProperty); set => SetValue(IBackgroundElement.BackgroundColorProperty, value); }
    public double Step { get => (double)GetValue(StepProperty); set => SetValue(StepProperty, value); }
    public SliderFormatType FormatType { get => (SliderFormatType)GetValue(FormatTypeProperty); set => SetValue(FormatTypeProperty, value); }
    public  ICommand? DragCompletedCommand { get => (ICommand?)GetValue(DragCompletedCommandProperty); set => SetValue(DragCompletedCommandProperty, value); }
    public object? DragCompletedCommandParameter { get => GetValue(DragCompletedCommandParameterProperty); set => SetValue(DragCompletedCommandParameterProperty, value); }

    // INTERNAL STATE
    internal bool IsDragging { get; private set; }
    internal double DragValue { get; private set; }

    private static void RequestRedraw(BindableObject bo, object ov, object nv) => ((IElement)bo).OnPropertyChanged();

    public Slider()
    {
        this.Drawable = new SliderDrawable(this);
        this.StartInteraction += OnTouchStart;
        this.DragInteraction += OnTouchDrag;
        this.EndInteraction += OnTouchEnd;
        this.CancelInteraction += OnTouchEnd;
    }

    private static void OnValuePropertyChanged(BindableObject bo, object ov, object nv)
    {
        var slider = (Slider)bo;
        if (slider.IsDragging) return;

        double oldVal = (double)ov;
        double newVal = Math.Clamp((double)nv, slider.Minimum, slider.Maximum);

        if (slider.AnimatedProgress && slider.Handler != null && slider.Width > 0)
        {
            slider.AnimateValueTransition(oldVal, newVal);
        }
        else
        {
            slider.DrawValue = newVal;
            slider.Invalidate();
        }

        slider.ValueChanged?.Invoke(slider, new ValueChangedEventArgs(oldVal, newVal));
    }

    private void AnimateValueTransition(double startValue, double endValue)
    {
        this.animationManager ??= this.Handler?.MauiContext?.Services.GetRequiredService<IAnimationManager>();

        var transitionAnim = new Microsoft.Maui.Animations.Animation(
            callback: (progress) =>
            {
                this.DrawValue = startValue + ((endValue - startValue) * progress);
                this.Invalidate();
            },
            duration: this.AnimationDuration,
            easing: Easing.CubicOut
        );

        this.animationManager?.Add(transitionAnim);
    }

    private double CalculateValueFromPosition(float x)
    {
        float usableWidth = (float)this.Bounds.Width - (this.ThumbRadius * 2);
        if (usableWidth <= 0) return Minimum;

        float touchX = Math.Clamp(x - this.ThumbRadius, 0, usableWidth);
        double percentage = touchX / usableWidth;
       
        double rawVal = Minimum + (percentage * (Maximum - Minimum));

        // Snap to step if specified
        if (this.Step > 0)
        {
            rawVal = Math.Round(rawVal / this.Step) * this.Step;
        }

        return Math.Clamp(rawVal, Minimum, Maximum);
    }

    private void OnTouchStart(object? sender, TouchEventArgs e)
    {
        if (!this.IsEnabled) return;

        IsDragging = true;
        DragValue = CalculateValueFromPosition(e.Touches[0].X);
        Invalidate();

        if (!UpdateValueOnRelease)
            this.Value = DragValue;

        DragStarted?.Invoke(this, EventArgs.Empty);
    }

    private void OnTouchDrag(object? sender, TouchEventArgs e)
    {
        if (!IsDragging || !this.IsEnabled) return;
        DragValue = CalculateValueFromPosition(e.Touches[0].X);
        Invalidate();

        if (!UpdateValueOnRelease)
            this.Value = DragValue;
    }

    private void OnTouchEnd(object? sender, EventArgs e)
    {
        if (!IsDragging) return;

        IsDragging = false;
        this.Value = DragValue; // Sync the final value to the ViewModel!
        Invalidate();

        DragCompleted?.Invoke(this, new SliderDragCompletedEventArgs(DragValue));

        var param = this.DragCompletedCommandParameter ?? DragValue;
        if (this.DragCompletedCommand?.CanExecute(param) is true)
        {
            this.DragCompletedCommand.Execute(param);
        }
    }

    internal string GetFormattedValue()
    {
        string formatted;

        switch (this.FormatType)
        {
            case SliderFormatType.Time:
                var ts = TimeSpan.FromSeconds(Math.Max(0, DragValue));
                formatted = ts.TotalHours >= 1
                    ? ts.ToString(@"h\:mm\:ss")
                    : ts.ToString(@"mm\:ss");
                break;

            case SliderFormatType.Percentage:
                double range = Maximum - Minimum;
                double pct = range > 0 ? ((DragValue - Minimum) / range) * 100 : 0;
                formatted = $"{pct:F0}%";
                break;

            case SliderFormatType.Integer:
                formatted = $"{DragValue:F0}";
                break;

            case SliderFormatType.Decimal:
                formatted = $"{DragValue:F2}";
                break;

            case SliderFormatType.Custom:
            default:
                formatted = string.Format(ValueBubbleFormat ?? "{0:F0}", DragValue);
                break;
        }

        // Custom Event Override (highest priority)
        if (this.FormatValueBubble != null)
        {
            var args = new SliderFormatLabelEventArgs(DragValue, formatted);
            this.FormatValueBubble.Invoke(this, args);
            return args.FormattedLabel;
        }

        return formatted;
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // Require at least enough height for the track, thumb, and the floating bubble (if enabled)
        float requiredHeight = (ThumbRadius * 2);
        if (ShowValueBubble)
            requiredHeight += 38f; // Space for the bubble popup above the thumb

        var width = this.WidthRequest != -1 ? this.WidthRequest : (double.IsInfinity(widthConstraint) ? 200 : widthConstraint);
        var height = this.HeightRequest != -1 ? this.HeightRequest : Math.Max(requiredHeight, 48); // 48 is standard touch target size

        this.DesiredSize = new Size(width, height);
        return this.DesiredSize;
    }
    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            // Event unhooking
            this.StartInteraction -= OnTouchStart;
            this.DragInteraction -= OnTouchDrag;
            this.EndInteraction -= OnTouchEnd;
            this.CancelInteraction -= OnTouchEnd;
        }
        base.Dispose(disposing);
    }
}