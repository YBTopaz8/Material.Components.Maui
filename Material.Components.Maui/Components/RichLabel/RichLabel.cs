using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Material.Components.Maui;

[ContentProperty(nameof(Inlines))]
public partial class RichLabel
    : TouchGraphicsView,
        ITextElement,
        IFontElement,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IRippleElement,
        IDisposable
{
    // ==========================================
    // BINDABLE PROPERTIES
    // ==========================================
    public static readonly BindableProperty TextProperty = ITextElement.TextProperty;
    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty = IFontElement.FontIsItalicProperty;

    public static readonly BindableProperty LineSpacingProperty = BindableProperty.Create(
       nameof(LineSpacing), typeof(float), typeof(RichLabel), 4f,
       propertyChanged: (b, o, n) => ((IElement)b).InvalidateMeasure());

    public static readonly BindableProperty LinkColorProperty = BindableProperty.Create(
        nameof(LinkColor), typeof(Color), typeof(RichLabel), Colors.DodgerBlue,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty EnableMarqueeProperty = BindableProperty.Create(
        nameof(EnableMarquee), typeof(bool), typeof(RichLabel), false,
        propertyChanged: (bo, ov, nv) => ((RichLabel)bo).OnMarqueeToggled((bool)nv));

    public static readonly BindableProperty MarqueeSpeedProperty = BindableProperty.Create(
        nameof(MarqueeSpeed), typeof(double), typeof(RichLabel), 40.0); // Pixels per second

    public static readonly BindableProperty LinkCommandProperty = BindableProperty.Create(
        nameof(LinkCommand), typeof(ICommand), typeof(RichLabel), null);

    // ==========================================
    // ACCESSORS
    // ==========================================
    public float LineSpacing { get => (float)GetValue(LineSpacingProperty); set => SetValue(LineSpacingProperty, value); }
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color FontColor { get => (Color)GetValue(FontColorProperty); set => SetValue(FontColorProperty, value); }
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
    public bool FontIsItalic { get => (bool)GetValue(FontIsItalicProperty); set => SetValue(FontIsItalicProperty, value); }
    public Color LinkColor { get => (Color)GetValue(LinkColorProperty); set => SetValue(LinkColorProperty, value); }
    public bool EnableMarquee { get => (bool)GetValue(EnableMarqueeProperty); set => SetValue(EnableMarqueeProperty, value); }
    public double MarqueeSpeed { get => (double)GetValue(MarqueeSpeedProperty); set => SetValue(MarqueeSpeedProperty, value); }
    public ICommand? LinkCommand { get => (ICommand?)GetValue(LinkCommandProperty); set => SetValue(LinkCommandProperty, value); }

    public ObservableCollection<InlineText> Inlines { get; } = new();
    internal List<RichTextRun> FormattedRuns { get; } = new();
    public event EventHandler<string>? LinkClicked;

    internal float MarqueeOffset { get; set; } = 0f;
    internal float TotalMeasuredWidth { get; set; } = 0f;

    private IDispatcherTimer? _marqueeTimer;
    private bool _marqueeForward = true;
    private int _marqueePauseTicks = 0;

    public RichLabel()
    {
        this.SetDynamicResource(StyleProperty, "StandardRichLabelStyle");
        this.Drawable = new RichLabelDrawable(this);
        this.Inlines.CollectionChanged += OnInlinesCollectionChanged;
        this.Clicked += OnLabelClicked;
    }

    private void OnLabelClicked(object? sender, TouchEventArgs e)
    {
#if WINDOWS
        if (!this.IsLeftClick) return;
#endif
        if (e.Touches.Length == 0) return;
        var touch = e.Touches[0];

        // Touch Hit-Testing: Check if the user tapped on a link!
        foreach (var run in this.FormattedRuns)
        {
            if (run.IsLink && run.Bounds.Contains(touch))
            {
                run.LinkSource?.TriggerClick();

                LinkClicked?.Invoke(this, run.LinkUrl);
                if (LinkCommand?.CanExecute(run.LinkUrl) is true)
                    LinkCommand.Execute(run.LinkUrl);
                return;
            }
        }
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var maxWidth = Math.Min(
            widthConstraint,
            this.MaximumWidthRequest != -1 ? this.MaximumWidthRequest : double.PositiveInfinity
        );

        if (double.IsInfinity(maxWidth) || maxWidth <= 0)
            maxWidth = 300;

        var drawable = (RichLabelDrawable)this.Drawable;
        float measuredHeight = drawable.CalculateLayout(new RectF(0, 0, (float)maxWidth, 10000), recordBounds: false);

        var finalWidth = this.WidthRequest != -1 ? this.WidthRequest : (this.EnableMarquee ? maxWidth : Math.Min(maxWidth, this.TotalMeasuredWidth));
        var finalHeight = this.HeightRequest != -1 ? this.HeightRequest : Math.Max(measuredHeight, this.FontSize + 8f);

        this.DesiredSize = new Size(Math.Ceiling(finalWidth), Math.Ceiling(finalHeight));
        return this.DesiredSize;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var inline in this.Inlines)
        {
            SetInheritedBindingContext(inline, this.BindingContext);
        }
    }

    private void OnInlinesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (InlineText item in e.OldItems)
            {
                item.Changed -= OnInlineItemChanged;
                SetInheritedBindingContext(item, null);
            }
        }

        if (e.NewItems != null)
        {
            foreach (InlineText item in e.NewItems)
            {
                item.Changed += OnInlineItemChanged;
                SetInheritedBindingContext(item, this.BindingContext);
            }
        }

        ((IElement)this).InvalidateMeasure();
    }

    private void OnInlineItemChanged(object? sender, EventArgs e)
    {
        ((IElement)this).InvalidateMeasure();
    }

    private void OnMarqueeToggled(bool enable)
    {
        if (enable) StartMarquee();
        else StopMarquee();
    }

    private void StartMarquee()
    {
        _marqueeTimer?.Stop();
        _marqueeTimer = Dispatcher.CreateTimer();
        _marqueeTimer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS
        _marqueeTimer.Tick += (s, e) =>
        {
            if (this.Bounds.Width <= 0) return;

            float overflow = this.TotalMeasuredWidth - (float)this.Bounds.Width + 24f;
            if (overflow <= 0) return;

            if (_marqueePauseTicks > 0)
            {
                _marqueePauseTicks--;
                return;
            }

            float delta = (float)(this.MarqueeSpeed * 0.016);
            if (_marqueeForward)
            {
                this.MarqueeOffset += delta;
                if (this.MarqueeOffset >= overflow)
                {
                    this.MarqueeOffset = overflow;
                    _marqueeForward = false;
                    _marqueePauseTicks = 90; // Pause at right edge
                }
            }
            else
            {
                this.MarqueeOffset -= delta;
                if (this.MarqueeOffset <= 0)
                {
                    this.MarqueeOffset = 0;
                    _marqueeForward = true;
                    _marqueePauseTicks = 90; // Pause at left edge
                }
            }

            this.Invalidate();
        };
        _marqueeTimer.Start();
    }

    private void StopMarquee()
    {
        _marqueeTimer?.Stop();
        _marqueeTimer = null;
        this.MarqueeOffset = 0f;
        this.Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            StopMarquee();
            this.Clicked -= OnLabelClicked;
            foreach (var inline in this.Inlines)
                inline.Changed -= OnInlineItemChanged;
        }
        base.Dispose(disposing);
    }
}

internal class RichTextRun
{
    public string Text { get; set; } = string.Empty;
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    public bool IsUnderlined { get; set; }
    public bool IsStrikethrough { get; set; }
    public bool IsLink { get; set; }
    public string LinkUrl { get; set; } = string.Empty;
    public Color Color { get; set; } = Colors.Black;
    public float FontSize { get; set; }
    public string? FontFamily { get; set; }
    public LinkInlineText? LinkSource { get; set; }
    public RectF Bounds { get; set; }
}