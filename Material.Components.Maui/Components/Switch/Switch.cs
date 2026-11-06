using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Animations;

namespace Material.Components.Maui;

public partial class Switch
    : TouchGraphicsView,
        IIconElement,
        IOutlineElement,IFontElement,
        IElement,
    ITextElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IICommandElement,
        IVisualTreeElement,
        IStyleElement,
        IDisposable
{
    protected new bool IsVisualStateChanging { get; set; }
    ViewState viewState = ViewState.Normal;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new ViewState ViewState
    {
        get => this.viewState;
        set
        {
            this.viewState = value;
            this.ChangeVisualState();
        }
    }

    public void OnPropertyChanged()
    {
        if (this.Handler != null && !this.IsVisualStateChanging)
        {
            this.Invalidate();
        }
    }

    void IElement.InvalidateMeasure()
    {
        this.InvalidateMeasure();
    }

    protected override void ChangeVisualState()
    {
        this.IsVisualStateChanging = true;
        var state = this.ViewState switch
        {
            ViewState.Normal => this.IsSelected ? "selected_normal" : "unselected_normal",
            ViewState.Hovered => this.IsSelected ? "selected_hovered" : "unselected_hovered",
            ViewState.Pressed => this.IsSelected ? "selected_pressed" : "unselected_pressed",
            ViewState.Disabled => this.IsSelected ? "selected_disabled" : "unselected_disabled",
            _ => "unselected_normal",
        };
        VisualStateManager.GoToState(this, state);
        this.IsVisualStateChanging = false;

        this.Invalidate();
    }
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected),
        typeof(bool),
        typeof(Switch),
        false,
        propertyChanged: (bo, ov, nv) =>
        {
            var view = bo as Switch;
            view?.ChangeVisualState();
            view?.SelectedChanged?.Invoke(view, new((bool)nv));

            if (view?.Command?.CanExecute(view.CommandParameter ?? nv) is true)
                view.Command?.Execute(view.CommandParameter ?? nv);
        }
    );

    public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(
        nameof(ThumbColor),
        typeof(Color),
        typeof(Switch),
        propertyChanged: (bo, ov, nv) => ((Switch)bo).OnPropertyChanged()
    );
    public static new readonly BindableProperty IsEnabledProperty = IElement.IsEnabledProperty;
    public static new readonly BindableProperty BackgroundColorProperty =
        IBackgroundElement.BackgroundColorProperty;
    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;
    public new static readonly BindableProperty ShapeProperty = IShapeElement.ShapeProperty;
    public static readonly BindableProperty IconDataProperty = IIconElement.IconDataProperty;
    public static readonly BindableProperty IconColorProperty = IIconElement.IconColorProperty;
    public new static readonly BindableProperty StateLayerColorProperty =
        IStateLayerElement.StateLayerColorProperty;

    public new static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public new static readonly BindableProperty CommandParameterProperty =
        IICommandElement.CommandParameterProperty;

    public new static readonly BindableProperty DynamicStyleProperty =
        IStyleElement.DynamicStyleProperty;

    public new string DynamicStyle
    {
        get => (string)this.GetValue(DynamicStyleProperty);
        set => this.SetValue(DynamicStyleProperty, value);
    }

    public Color ThumbColor
    {
        get => (Color)this.GetValue(ThumbColorProperty);
        set => this.SetValue(ThumbColorProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)this.GetValue(IsSelectedProperty);
        set => this.SetValue(IsSelectedProperty, value);
    }

    public new bool IsEnabled
    {
        get => (bool)this.GetValue(TouchGraphicsView.IsEnabledProperty);
        set => this.SetValue(TouchGraphicsView.IsEnabledProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
    }

    [TypeConverter(typeof(ShapeConverter))]
    public new Shape Shape
    {
        get => (Shape)this.GetValue(ShapeProperty);
        set => this.SetValue(ShapeProperty, value);
    }
    public Color? OutlineColor
    {
        get => (Color)this.GetValue(OutlineColorProperty);
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

    public new Color StateLayerColor
    {
        get => (Color)this.GetValue(StateLayerColorProperty);
        set => this.SetValue(StateLayerColorProperty, value);
    }

    public new ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }

    public new object CommandParameter
    {
        get => this.GetValue(CommandParameterProperty);
        set => this.SetValue(CommandParameterProperty, value);
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
    public event EventHandler<CheckedChangedEventArgs>? SelectedChanged;

    protected new IAnimationManager? animationManager;
    new bool isTouching = false;

    public Switch()
    {
        this.SetDynamicResource(StyleProperty, "DefaultSwitchStyle");

        this.Drawable = new SwitchDrawable(this);
        this.StartInteraction += this.OnStartInteraction;
        this.EndInteraction += this.OnEndInteraction;
        this.CancelInteraction += this.OnCancelInteraction;
        this.StartHoverInteraction += this.OnStartHoverInteraction;
        this.EndHoverInteraction += this.OnEndHoverInteraction;
    }

    private new void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        this.ViewState = ViewState.Pressed;
        this.isTouching = true;
    }

    private new void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        if (this.isTouching)
            this.IsSelected = !this.IsSelected;

#if __MOBILE__
        this.ViewState = ViewState.Normal;
#else
        if (e.IsInsideBounds)
        {
            this.ViewState = e.IsInsideBounds ? ViewState.Hovered : ViewState.Normal;
        }
#endif

        this.isTouching = false;
    }

    private new void OnCancelInteraction(object? sender, EventArgs e)
    {
        this.isTouching = false;
    }

    private new void OnStartHoverInteraction(object? sender, TouchEventArgs e)
    {
        if (!this.IsEnabled)
            return;

        this.ViewState = ViewState.Hovered;
    }

    private new void OnEndHoverInteraction(object? sender, EventArgs e)
    {
        if (!this.IsEnabled)
            return;

        this.ViewState = ViewState.Normal;
    }
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var textSize = this.GetStringSize();
        var width = 52f + (textSize.Width > 0 ? 8f + textSize.Width : 0); // 52px track + 8px gap + text
        var height = Math.Max(32f, textSize.Height); // 32px track height

        return new Size(width, height);
    }
    private new bool disposedValue;

    protected new virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.StartInteraction -= this.OnStartInteraction;
                this.EndInteraction -= this.OnEndInteraction;
                this.CancelInteraction -= this.OnCancelInteraction;
                this.StartHoverInteraction -= this.OnStartHoverInteraction;
                this.EndHoverInteraction -= this.OnEndHoverInteraction;
            }
            this.disposedValue = true;
        }
    }

    public new void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
