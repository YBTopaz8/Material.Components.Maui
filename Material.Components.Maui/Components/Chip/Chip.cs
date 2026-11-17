using System.ComponentModel;
using System.Xml.Linq;

namespace Material.Components.Maui;

public partial class Chip
    : TouchGraphicsView,
        IIconElement,
        ITextElement,
        IFontElement,
        IOutlineElement,
        IElevationElement,
        IElement,
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
    public ImageSource IconSource
    {
        get => (ImageSource)this.GetValue(IconSourceProperty);
        set => this.SetValue(IconSourceProperty, value);
    }
    public static readonly BindableProperty IconSourceProperty = IIconElement.IconSourceProperty;

    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly BindableProperty ChipTypeProperty = BindableProperty.Create(
        nameof(ChipType), typeof(ChipType), typeof(Chip), ChipType.Filter,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected),
        typeof(bool),
        typeof(Chip),
        false,
        propertyChanged: (bo, ov, nv) =>
        {
            var view = bo as Chip;
            view?.ChangeVisualState();
            view?.SelectedChanged?.Invoke(view, new((bool)nv));

            if (view?.Command?.CanExecute(view.CommandParameter ?? nv) is true)
                view.Command?.Execute(view.CommandParameter ?? nv);
        }
    );
    public static readonly BindableProperty IsTriStateProperty = BindableProperty.Create(
       nameof(IsTriState), typeof(bool), typeof(Chip), false);
    public static readonly BindableProperty IndeterminateColorProperty = BindableProperty.Create(
           nameof(IndeterminateColor), typeof(Color), typeof(Chip), Colors.DarkSlateGray,
           propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
       nameof(IsChecked), typeof(bool?), typeof(Chip), false,
       propertyChanged: (bo, ov, nv) =>
       {
           var view = (Chip)bo;
           view.IsSelected = (nv as bool?) == true;
           view.ChangeVisualState();
           view.CheckedChanged?.Invoke(view, new CheckedChangedEventArgs((bool?)nv == true));
           if (view.Command?.CanExecute(view.CommandParameter ?? nv) is true)
               view.Command?.Execute(view.CommandParameter ?? nv);
       });
    public static readonly BindableProperty HasCloseButtonProperty = BindableProperty.Create(
        nameof(HasCloseButton),
        typeof(bool),
        typeof(Chip),
        false,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).InvalidateMeasure()
    );

    public static readonly BindableProperty TextProperty = ITextElement.TextProperty;
    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty =
        IFontElement.FontIsItalicProperty;

    public static readonly BindableProperty IconDataProperty = IIconElement.IconDataProperty;
    public static readonly BindableProperty IconColorProperty = IIconElement.IconColorProperty;

    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;
    public static readonly BindableProperty ElevationProperty = IElevationElement.ElevationProperty;

    public bool IsSelected
    {
        get => (bool)this.GetValue(IsSelectedProperty);
        set => this.SetValue(IsSelectedProperty, value);
    }


    public ChipType ChipType { get => (ChipType)GetValue(ChipTypeProperty); set => SetValue(ChipTypeProperty, value); }
    public bool? IsChecked { get => (bool?)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }
    public bool IsTriState { get => (bool)GetValue(IsTriStateProperty); set => SetValue(IsTriStateProperty, value); }
    public Color IndeterminateColor { get => (Color)GetValue(IndeterminateColorProperty); set => SetValue(IndeterminateColorProperty, value); }
    public bool HasCloseButton
    {
        get => (bool)this.GetValue(HasCloseButtonProperty);
        set => this.SetValue(HasCloseButtonProperty, value);
    }

    public string Text
    {
        get => (string)this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }
    public Color FontColor
    {
        get => (Color)this.GetValue(FontColorProperty);
        set => this.SetValue(FontColorProperty, value);
    }
    public float FontSize
    {
        get => (float)this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }
    public string FontFamily
    {
        get => (string)this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }
    public FontWeight FontWeight
    {
        get => (FontWeight)this.GetValue(FontWeightProperty);
        set => this.SetValue(FontWeightProperty, value);
    }
    public bool FontIsItalic
    {
        get => (bool)this.GetValue(FontIsItalicProperty);
        set => this.SetValue(FontIsItalicProperty, value);
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
    public Elevation Elevation
    {
        get => (Elevation)this.GetValue(ElevationProperty);
        set => this.SetValue(ElevationProperty, value);
    }

    public event EventHandler<EventArgs>? Closed;
    public event EventHandler<CheckedChangedEventArgs>? SelectedChanged;

    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;
    protected IFontManager? fontManager;

    public Chip()
    {
        this.SetDynamicResource(StyleProperty, "FilterChipStyle");

        this.Drawable = new ChipDrawable(this);
        this.Clicked += this.OnClicked;
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var maxWidth = Math.Min(
            Math.Min(widthConstraint, this.MaximumWidthRequest),
            this.WidthRequest != -1 ? this.WidthRequest : double.PositiveInfinity
        );
        var maxHeight = Math.Min(
            Math.Min(heightConstraint, this.MaximumHeightRequest),
            this.HeightRequest != -1 ? this.HeightRequest : double.PositiveInfinity
        );

        var scale =
            (this.HeightRequest != -1 ? this.HeightRequest : Math.Min(32f, maxHeight)) / 32f;
        var iconSize = (!string.IsNullOrEmpty(this.IconData) ? 18f : 0f) * scale;
        var closeSize = this.HasCloseButton ? 18f * scale : 0f;
        var textSize = this.GetStringSize();
        //16 + iconSize + textSize.Width + closeBtnSize +  16
        var needWidth = 32f * scale + iconSize + textSize.Width + closeSize;
        var needHeight = 32f * scale;

        var width =
            this.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxWidth)
                ? maxWidth
                : this.Margin.HorizontalThickness
                    + Math.Max(
                        this.MinimumWidthRequest,
                        this.WidthRequest == -1 ? Math.Min(maxWidth, needWidth) : this.WidthRequest
                    );
        var height =
            this.VerticalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxHeight)
                ? maxHeight
                : this.Margin.VerticalThickness
                    + Math.Max(
                        this.MinimumHeightRequest,
                        this.HeightRequest == -1
                            ? Math.Min(maxHeight, needHeight)
                            : this.HeightRequest
                    );

        this.DesiredSize = new Size(Math.Ceiling(width), Math.Ceiling(height));
        return this.DesiredSize;
    }

    private void OnClicked(object? sender, TouchEventArgs e)
    {
#if WINDOWS
        if (!this.IsLeftClick) return;
#endif

        if (this.HasCloseButton || this.ChipType == ChipType.Input)
        {
            var scale = (float)this.Bounds.Height / 32f;
            var closeRect = new RectF((float)(this.Bounds.Width - 24f * scale), 8f * scale, 18f * scale, 18f * scale);
            if (closeRect.Contains(e.Touches[0]))
            {
                this.Closed?.Invoke(this, e);
                return;
            }
        }

        // Action Chip: Simply triggers click/command, never toggles checks!
        if (this.ChipType == ChipType.Action)
        {
            return;
        }

        // Filter Chip: Toggles state
        if (this.IsTriState)
        {
            // Cycle: false -> true -> null (indeterminate) -> false
            this.IsChecked = this.IsChecked switch
            {
                false => true,
                true => null,
                _ => false
            };
        }
        else
        {
            this.IsChecked = !(this.IsChecked == true);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            this.Clicked -= this.OnClicked;
            ((IIconElement)this).IconPath?.Dispose();
            ((ChipDrawable)this.Drawable)?.Dispose();
        }
        base.Dispose(disposing);
    }
}
public enum ChipType
{
    Action,   // Simple touch button, only ripples & fires Clicked/Command
    Filter,   // Toggleable with Checkmark or Indeterminate Dot •
    Input     // Removable with close button (✕)
}