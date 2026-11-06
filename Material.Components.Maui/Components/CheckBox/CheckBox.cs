namespace Material.Components.Maui;

public partial class CheckBox
    : TouchGraphicsView,
        IIconElement, IFontElement,
        IOutlineElement,
        IElement, ITextElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IRippleElement,
        IICommandElement,
        IVisualTreeElement,
        IDisposable
{
    protected override void ChangeVisualState()
    {
        this.IsVisualStateChanging = true;
        var state = this.ViewState switch
        {
            ViewState.Normal => "normal",
            ViewState.Hovered => "hovered",
            ViewState.Pressed => "pressed",
            ViewState.Disabled => "disabled",
            _ => "normal",
        };
        VisualStateManager.GoToState(this, state);
        this.IsVisualStateChanging = false;

        this.Invalidate();
    }
    public static readonly BindableProperty TextProperty = ITextElement.TextProperty;
    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty = IFontElement.FontIsItalicProperty;

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color FontColor { get => (Color)GetValue(FontColorProperty); set => SetValue(FontColorProperty, value); }
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
    public bool FontIsItalic { get => (bool)GetValue(FontIsItalicProperty); set => SetValue(FontIsItalicProperty, value); }
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly BindableProperty IconDataProperty = IIconElement.IconDataProperty;
    public static readonly BindableProperty IconColorProperty = IIconElement.IconColorProperty;
    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked),
        typeof(bool),
        typeof(CheckBox),
        false,
        propertyChanged: (bo, ov, nv) =>
        {
            var cb = (CheckBox)bo;
            cb.CheckedChanged?.Invoke(cb, new CheckedChangedEventArgs(cb.IsChecked));

            if (cb.Command?.CanExecute(cb.CommandParameter ?? cb.IsChecked) is true)
                cb.Command?.Execute(cb.CommandParameter ?? cb.IsChecked);
        }
    );

    public Color? OutlineColor
    {
        get => (Color?)this.GetValue(OutlineColorProperty);
        set => this.SetValue(OutlineColorProperty, value);
    }

    public int OutlineWidth
    {
        get => (int)this.GetValue(OutlineWidthProperty);
        set => this.SetValue(OutlineWidthProperty, value);
    }

    public string IconData
    {
        get => (string)this.GetValue(IconDataProperty);
        set => this.SetValue(IconDataProperty, value);
    }
    PathF? IIconElement.IconPath { get; set; }

    public Color IconColor
    {
        get => (Color)this.GetValue(IconColorProperty);
        set => this.SetValue(IconColorProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)this.GetValue(IsCheckedProperty);
        set => this.SetValue(IsCheckedProperty, value);
    }

    event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    public CheckBox()
    {
        this.Drawable = new CheckBoxDrawable(this);
        this.EndInteraction += this.OnCheckChanged;
    }

    private void OnCheckChanged(object? sender, TouchEventArgs e)
    {
        this.IsChecked = !this.IsChecked;
    }
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var textSize = this.GetStringSize();
        var width = 40f + (textSize.Width > 0 ? 8f + textSize.Width : 0); 
        var height = Math.Max(32f, textSize.Height); // 32px track height

        return new Size(width, height);
    }
    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            this.EndInteraction -= this.OnCheckChanged;
            ((IIconElement)this).IconPath?.Dispose();
        }
        base.Dispose(disposing);
    }
}
