using System.ComponentModel;

namespace Material.Components.Maui;

public partial class CardContainer
    : TouchGraphicsView,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IOutlineElement,
    IStateLayerElement,
        IRippleElement,
        IElevationElement
{
    ViewState IElement.ViewState => ViewState.Normal;

    void IElement.OnPropertyChanged()
    {
        if (this.Handler != null)
            this.Invalidate();
    }
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static new readonly BindableProperty BackgroundColorProperty =
        IBackgroundElement.BackgroundColorProperty;
    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;
    public static readonly BindableProperty ElevationProperty = IElevationElement.ElevationProperty;
  
    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
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

    public CardContainer()
    {
        this.Drawable = new CardContainerDrawable(this);
    }
}
