namespace Material.Components.Maui;

public partial class FAB
    : TouchGraphicsView,
        IIconElement,
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

    public static readonly BindableProperty IconDataProperty = IIconElement.IconDataProperty;
    public static readonly BindableProperty IconColorProperty = IIconElement.IconColorProperty;
    public static readonly BindableProperty ElevationProperty = IElevationElement.ElevationProperty;

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

    public Elevation Elevation
    {
        get => (Elevation)this.GetValue(ElevationProperty);
        set => this.SetValue(ElevationProperty, value);
    }

    public FAB()
    {
        this.SetDynamicResource(StyleProperty, "SurfaceFABStyle");

        this.Drawable = new FABDrawable(this);
        this.Clicked += this.OnClicked;
    }
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        // Standard Material Design FAB is 56x56
        var scale = (this.HeightRequest != -1 ? this.HeightRequest : 56f) / 56f;
        var size = 56f * scale;

        var width = this.Margin.HorizontalThickness + Math.Max(this.MinimumWidthRequest, this.WidthRequest == -1 ? size : this.WidthRequest);
        var height = this.Margin.VerticalThickness + Math.Max(this.MinimumHeightRequest, this.HeightRequest == -1 ? size : this.HeightRequest);

        this.DesiredSize = new Size(Math.Ceiling(width), Math.Ceiling(height));
        return this.DesiredSize;
    }
    void OnClicked(object? sender, TouchEventArgs e)
    {
#if WINDOWS
        if (!this.IsLeftClick) return;
#endif
        if (this.Command?.CanExecute(this.CommandParameter) is true)
            this.Command?.Execute(this.CommandParameter);
    }

    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            this.Clicked -= this.OnClicked;
            ((IIconElement)this).IconPath?.Dispose();
        }
        base.Dispose(disposing);
    }
}
