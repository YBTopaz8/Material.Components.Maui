namespace Material.Components.Maui.Interfaces;

public interface IEditableElement : IElement
{
    string? Text { get; set; }
    TextRange SelectionRange { get; set; }
    Color CaretColor { get; set; }
    TextAlignment TextAlignment { get; set; }
    bool IsReadOnly { get; set; }
    Thickness EditablePadding { get; set; }
    InputType InputType { get; set; }

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(IEditableElement),
        default,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).InvalidateMeasure(),
         coerceValue: (bo, val) =>
         {
             if (val is string str && bo is IEditableElement editable)
             {
                 if (editable.InputType is InputType.Numeric)
                 {
                     return new string(str.Where(char.IsDigit).ToArray());
                 }
                 if (editable.InputType is InputType.DecimalNumber)
                 {
                     return System.Text.RegularExpressions.Regex.Replace(str, @"[^0-9\.]", "");
                 }
             }
             return val;
         }
    );

    public static readonly BindableProperty SelectionRangeProperty = BindableProperty.Create(
        nameof(SelectionRange),
        typeof(TextRange),
        typeof(IEditableElement),
        default,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty CaretColorProperty = BindableProperty.Create(
        nameof(CaretColor),
        typeof(Color),
        typeof(IEditableElement),
        default,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty TextAlignmentProperty = BindableProperty.Create(
        nameof(TextAlignment),
        typeof(TextAlignment),
        typeof(IEditableElement),
        TextAlignment.Start,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(IEditableElement),
        default,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty EditablePaddingProperty = BindableProperty.Create(
        nameof(EditablePadding),
        typeof(Thickness),
        typeof(IEditableElement),
        Thickness.Zero,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
    );

    public static readonly BindableProperty InputTypeProperty = BindableProperty.Create(
        nameof(InputType),
        typeof(InputType),
        typeof(IEditableElement),
        InputType.None,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).InvalidateMeasure()
    );
}
