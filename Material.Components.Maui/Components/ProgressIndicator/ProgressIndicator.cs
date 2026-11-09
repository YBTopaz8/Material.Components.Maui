using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Animations;

using ValueChangedEventArgs = Material.Components.Maui.Primitives.ValueChangedEventArgs;
namespace Material.Components.Maui;

public partial class ProgressIndicator
    : GraphicsView,
        IActiveIndicatorElement,
        IBackgroundElement,
        IICommandElement,
        IElement,
        IStyleElement
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ViewState ViewState => ViewState.Normal;

    void IElement.OnPropertyChanged()
    {
        if (this.Handler != null)
            this.Invalidate();
    }

    // ==========================================
    // BINDABLE PROPERTIES
    // ==========================================
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(ProgressIndicator), -1d,
        propertyChanged: OnValuePropertyChanged);

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(ProgressIndicator), 0d,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(ProgressIndicator), 100d,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor), typeof(Color), typeof(ProgressIndicator), Colors.Transparent,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty ShowValueLabelProperty = BindableProperty.Create(
        nameof(ShowValueLabel), typeof(bool), typeof(ProgressIndicator), false,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty ValueStringFormatProperty = BindableProperty.Create(
        nameof(ValueStringFormat), typeof(string), typeof(ProgressIndicator), "{0:0}",
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty AnimatedProgressProperty = BindableProperty.Create(
        nameof(AnimatedProgress), typeof(bool), typeof(ProgressIndicator), true);

    public static readonly BindableProperty IndicatorTypeProperty = BindableProperty.Create(
        nameof(IndicatorType), typeof(IndicatorType), typeof(ProgressIndicator), IndicatorType.Circular,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration), typeof(float), typeof(ProgressIndicator), 1.5f);

    public static new readonly BindableProperty IsEnabledProperty = IElement.IsEnabledProperty;
    public static readonly BindableProperty ActiveIndicatorHeightProperty = IActiveIndicatorElement.ActiveIndicatorHeightProperty;
    public static readonly BindableProperty ActiveIndicatorColorProperty = IActiveIndicatorElement.ActiveIndicatorColorProperty;
    public static new readonly BindableProperty BackgroundColorProperty = IBackgroundElement.BackgroundColorProperty;
    public static readonly BindableProperty ShapeProperty = IShapeElement.ShapeProperty;
    public static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public static readonly BindableProperty CommandParameterProperty = IICommandElement.CommandParameterProperty;
    public static readonly BindableProperty DynamicStyleProperty = IStyleElement.DynamicStyleProperty;

    // ==========================================
    // C# ACCESSORS
    // ==========================================
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public Color TrackColor { get => (Color)GetValue(TrackColorProperty); set => SetValue(TrackColorProperty, value); }
    public bool ShowValueLabel { get => (bool)GetValue(ShowValueLabelProperty); set => SetValue(ShowValueLabelProperty, value); }
    public string ValueStringFormat { get => (string)GetValue(ValueStringFormatProperty); set => SetValue(ValueStringFormatProperty, value); }
    public bool AnimatedProgress { get => (bool)GetValue(AnimatedProgressProperty); set => SetValue(AnimatedProgressProperty, value); }
    public IndicatorType IndicatorType { get => (IndicatorType)GetValue(IndicatorTypeProperty); set => SetValue(IndicatorTypeProperty, value); }
    public float AnimationDuration { get => (float)GetValue(AnimationDurationProperty); set => SetValue(AnimationDurationProperty, value); }
    public new bool IsEnabled { get => (bool)GetValue(IsEnabledProperty); set => SetValue(IsEnabledProperty, value); }
    public int ActiveIndicatorHeight { get => (int)GetValue(ActiveIndicatorHeightProperty); set => SetValue(ActiveIndicatorHeightProperty, value); }
    public Color ActiveIndicatorColor { get => (Color)GetValue(ActiveIndicatorColorProperty); set => SetValue(ActiveIndicatorColorProperty, value); }
    public new Color BackgroundColor { get => (Color)GetValue(BackgroundColorProperty); set => SetValue(BackgroundColorProperty, value); }
    public Shape Shape { get => (Shape)GetValue(ShapeProperty); set => SetValue(ShapeProperty, value); }
    public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }
    public string DynamicStyle { get => (string)GetValue(DynamicStyleProperty); set => SetValue(DynamicStyleProperty, value); }

    public event EventHandler<ValueChangedEventArgs>? ValueChanged;

    private IAnimationManager? animationManager;

    // Used by Canvas to draw the current frame of the animation
    internal float DrawPercent { get; private set; } = 0f;
    internal bool AnimationIsPositive = true;

    public ProgressIndicator()
    {
        this.SetDynamicResource(StyleProperty, "CircularProgressIndicatorStyle");
        this.Drawable = new ProgressIndicatorDrawable(this);
    }

    // Normalized value from 0.0 to 1.0 based on Min/Max
    internal double GetNormalizedValue()
    {
        if (Maximum <= Minimum) return 0;
        return Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0.0, 1.0);
    }

    private static void OnValuePropertyChanged(BindableObject bo, object ov, object nv)
    {
        var view = bo as ProgressIndicator;
        if (view == null) return;

        double oldVal = (double)ov;
        double newVal = (double)nv;

        if (newVal == -1d)
        {
            view.AnimationIsPositive = true;
            view.StartIndeterminateAnimation();
        }
        else
        {
            if (view.AnimatedProgress && view.Handler != null && oldVal != -1d)
            {
                view.AnimateValueTransition(oldVal, newVal);
            }
            else
            {
                view.DrawPercent = (float)view.GetNormalizedValue();
                ((IElement)view).OnPropertyChanged();
            }
        }

        view.ValueChanged?.Invoke(view, new ValueChangedEventArgs(oldVal, newVal));

        if (view.Command?.CanExecute(view.CommandParameter ?? newVal) is true)
            view.Command?.Execute(view.CommandParameter ?? newVal);
    }

    private void AnimateValueTransition(double startValue, double endValue)
    {
        this.animationManager ??= this.Handler?.MauiContext?.Services.GetRequiredService<IAnimationManager>();

        double startNorm = Math.Clamp((startValue - Minimum) / (Maximum - Minimum), 0.0, 1.0);
        double endNorm = Math.Clamp((endValue - Minimum) / (Maximum - Minimum), 0.0, 1.0);

        var transitionAnim = new Microsoft.Maui.Animations.Animation(
            callback: (progress) =>
            {
                this.DrawPercent = (float)startNorm.Lerp(endNorm, progress);
                this.Invalidate();
            },
            duration: 0.3f, // 300ms smooth transition
            easing: Easing.CubicOut
        );

        this.animationManager?.Add(transitionAnim);
    }

    private void StartIndeterminateAnimation()
    {
        if (this.Value != -1d)
        {
            this.Invalidate();
            return;
        }

        this.animationManager ??= this.Handler?.MauiContext?.Services.GetRequiredService<IAnimationManager>();

        var animation = new Microsoft.Maui.Animations.Animation(
            callback: (progress) =>
            {
                this.DrawPercent = this.AnimationIsPositive
                    ? 0f.Lerp(1f, progress)
                    : 1f - 0f.Lerp(1f, progress);

                this.Invalidate();
            }
        )
        {
            Duration = this.AnimationDuration,
            Easing = Easing.Linear,
            Repeats = true
        };
        animation.Finished = () =>
        {
            if (this.Value != -1d)
            {
                this.animationManager?.Remove(animation);
            }
            else if (this.IndicatorType is IndicatorType.Circular)
            {
                this.AnimationIsPositive = !this.AnimationIsPositive;
            }
        };
        this.animationManager?.Add(animation);
    }

    protected override void OnHandlerChanged()
    {
        if (this.Value == -1d)
        {
            this.AnimationIsPositive = true;
            this.StartIndeterminateAnimation();
        }
        else
        {
            this.DrawPercent = (float)this.GetNormalizedValue();
        }
    }
}