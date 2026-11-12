using System.ComponentModel;

namespace Material.Components.Maui;

public partial class SegmentedItem
    : VisualElement,
        IIconElement,
        ITextElement,
        IFontElement,
        IBackgroundElement,
        IStateLayerElement,
        IVisualTreeElement,
        IDisposable
{
    public object? Value { get; set; }
    protected bool IsVisualStateChanging { get; set; }
    ViewState viewState = ViewState.Normal;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ViewState ViewState
    {
        get => this.viewState;
        set
        {
            this.viewState = value;
            this.ChangeVisualState();
        }
    }

    void IElement.OnPropertyChanged()
    {
        var parent = this.GetParentElement<SegmentedButton>();
        (parent as IElement)?.OnPropertyChanged();
    }

    void IElement.InvalidateMeasure()
    {
    }
    internal float CheckAnimationPercent { get; private set; } = 0f;
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
        // --- ANIMATE THE CHECKMARK FOR MULTI-SELECT MODE! ---
        var parent = this.GetParentElement<SegmentedButton>();
        if (parent != null && parent.MultiSelectMode)
        {
            float start = this.CheckAnimationPercent;
            float end = this.IsSelected ? 1f : 0f;

            this.AbortAnimation("CheckAnim");
            var anim = new Microsoft.Maui.Controls.Animation(v =>
            {
                this.CheckAnimationPercent = (float)v;
                ((IElement)this).OnPropertyChanged();
            }, start, end);

            anim.Commit(this, "CheckAnim", length: 200, easing: Easing.CubicOut);
        }
        else
        {
            this.CheckAnimationPercent = this.IsSelected ? 1f : 0f;
        }

        ((IElement)this).OnPropertyChanged();
    }

    public static readonly new BindableProperty IsEnabledProperty = IElement.IsEnabledProperty;
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected),
        typeof(bool),
        typeof(SegmentedItem),
        false,
        propertyChanged: (bo, ov, nv) =>
        {
            var item = (SegmentedItem)bo;
            item.ChangeVisualState();
            var parent = item.GetParentElement<SegmentedButton>();
            parent?.OnSegmentedItemSelected(item);
        }
    );

    public static new readonly BindableProperty BackgroundColorProperty =
        IBackgroundElement.BackgroundColorProperty;
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(SegmentedItem),
        default,
        propertyChanged: (bo, ov, nv) =>
            ((IElement?)((SegmentedItem)bo)?.Parent)?.InvalidateMeasure()
    );

    public static readonly BindableProperty IconDataProperty = IIconElement.IconDataProperty;
    public static readonly BindableProperty IconColorProperty = IIconElement.IconColorProperty;

    public static readonly BindableProperty StateLayerColorProperty =
        IStateLayerElement.StateLayerColorProperty;

    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty = IFontElement.FontIsItalicProperty;
    public Color FontColor { get => (Color)GetValue(FontColorProperty); set => SetValue(FontColorProperty, value); }
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
    public bool FontIsItalic { get => (bool)GetValue(FontIsItalicProperty); set => SetValue(FontIsItalicProperty, value); }
    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
    }

    public new bool IsEnabled
    {
        get => (bool)this.GetValue(IsEnabledProperty);
        set => this.SetValue(IsEnabledProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)this.GetValue(IsSelectedProperty);
        set => this.SetValue(IsSelectedProperty, value);
    }

    public string Text
    {
        get => (string)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
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

    Shape IShapeElement.Shape
    {
        get => Shape.None;
        set { }
    }
    public Color StateLayerColor
    {
        get => (Color)this.GetValue(StateLayerColorProperty);
        set => this.SetValue(StateLayerColorProperty, value);
    }

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                ((IIconElement)this).IconPath?.Dispose();
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
