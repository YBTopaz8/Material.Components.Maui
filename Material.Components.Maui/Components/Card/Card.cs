using System.ComponentModel;
using System.Windows.Input;
using IPaddingElement = Material.Components.Maui.Interfaces.IPaddingElement;

namespace Material.Components.Maui;

[ContentProperty(nameof(Content))]
public partial class Card
    : TemplatedView,
        IContentElement,
        IElement,
        IBackgroundElement,
        IPaddingElement,
        IShapeElement,
        IOutlineElement,
        IElevationElement,IICommandElement,
        IVisualTreeElement,
        IStyleElement
{
    ViewState IElement.ViewState => ViewState.Normal;

    void IElement.OnPropertyChanged() { }

    public static readonly BindableProperty ContentProperty = BindableProperty.Create(
        nameof(Content),
        typeof(View),
        typeof(Card)
    );
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static new readonly BindableProperty BackgroundColorProperty =
        IBackgroundElement.BackgroundColorProperty;
    public static new readonly BindableProperty PaddingProperty = IPaddingElement.PaddingProperty;
    public static readonly BindableProperty ShapeProperty = IShapeElement.ShapeProperty;
    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;
    public static readonly BindableProperty ElevationProperty = IElevationElement.ElevationProperty;

    public static readonly BindableProperty DynamicStyleProperty =
        IStyleElement.DynamicStyleProperty;

    public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(
       nameof(IsClickable),
       typeof(bool),
       typeof(Card),
       false,
       propertyChanged: (bo, ov, nv) => ((Card)bo).UpdateInteractivity((bool)nv)
   );

    private void UpdateInteractivity(bool clickable)
    {
        if (this.PART_ContentPresenter != null)
        {
            // If card is clickable, pass touches through content to trigger the ripple!
            this.PART_ContentPresenter.InputTransparent = clickable;
        }
    }

    public bool IsClickable
    {
        get => (bool)this.GetValue(IsClickableProperty);
        set => this.SetValue(IsClickableProperty, value);
    }

    public static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty = IICommandElement.CommandParameterProperty;
    public object CommandParameter
    {
        get => this.GetValue(CommandParameterProperty);
        set => this.SetValue(CommandParameterProperty, value);
    }

    public event EventHandler<TouchEventArgs>? Clicked;

    public string DynamicStyle
    {
        get => (string)this.GetValue(DynamicStyleProperty);
        set => this.SetValue(DynamicStyleProperty, value);
    }

    public View Content
    {
        get => (View)this.GetValue(ContentProperty);
        set => this.SetValue(ContentProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
    }

    public new Thickness Padding
    {
        get => (Thickness)this.GetValue(PaddingProperty);
        set => this.SetValue(PaddingProperty, value);
    }

    [TypeConverter(typeof(ShapeConverter))]
    public Shape Shape
    {
        get => (Shape)this.GetValue(ShapeProperty);
        set => this.SetValue(ShapeProperty, value);
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

    Grid? PART_Root;
    CardContainer? PART_Container;
    ContentPresenter? PART_ContentPresenter;

    public Card()
    {
        this.SetDynamicResource(StyleProperty, "FilledCardStyle");
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.PART_Root = (Grid)this.GetTemplateChild("PART_Root");

        if (this.PART_Root != null)
        {
            this.PART_Container = this.PART_Root.Children.OfType<CardContainer>().FirstOrDefault();
            this.PART_ContentPresenter = this.PART_Root.Children.OfType<ContentPresenter>().FirstOrDefault();

            if (this.PART_Container != null)
            {
                this.PART_Container.Clicked += this.OnContainerClicked;
            }

            this.UpdateInteractivity(this.IsClickable);
        }

        this.OnChildAdded(this.PART_Root);
        VisualDiagnostics.OnChildAdded(this, this.PART_Root);
    }

    private void OnContainerClicked(object? sender, TouchEventArgs e)
    {
        if (!this.IsClickable)
            return;

#if WINDOWS
        // Enforce Left-Click only on Windows!
        if (sender is TouchGraphicsView tgv && !tgv.IsLeftClick)
            return;
#endif

        this.Clicked?.Invoke(this, e);

        if (this.Command?.CanExecute(this.CommandParameter) is true)
            this.Command?.Execute(this.CommandParameter);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        SetInheritedBindingContext(this.PART_Root, this.BindingContext);
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.PART_Root != null ? [this.PART_Root] : Array.Empty<IVisualTreeElement>().ToList();
}
