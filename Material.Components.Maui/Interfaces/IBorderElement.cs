namespace Material.Components.Maui.Interfaces;

public interface IBorderElement : IShapeElement
{
    int BorderWidth { get; set; }
    Color? BorderColor { get; set; }

    public static readonly BindableProperty BorderWidthProperty = BindableProperty.Create(
        nameof(BorderWidth),
        typeof(int),
        typeof(IBorderElement),
        0,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
        nameof(BorderColor),
        typeof(Color),
        typeof(IBorderElement),
        default,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );
}
