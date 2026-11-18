using System.Collections;


namespace Material.Components.Maui;



public partial class MaterialTableColumn : BindableObject
{
    public event EventHandler<MaterialTableCellValueChangedEventArgs>? ValueChanged;

    internal void RaiseValueChanged(MaterialTableCellValueChangedEventArgs args)
        => ValueChanged?.Invoke(this, args);
    public static readonly BindableProperty HeaderProperty =
        BindableProperty.Create(nameof(Header), typeof(string), typeof(MaterialTableColumn), string.Empty);

    public static readonly BindableProperty BindingPathProperty =
        BindableProperty.Create(nameof(BindingPath), typeof(string), typeof(MaterialTableColumn), string.Empty);

    public static readonly BindableProperty ColumnTypeProperty =
        BindableProperty.Create(nameof(ColumnType), typeof(MaterialTableColumnType), typeof(MaterialTableColumn), MaterialTableColumnType.Text);

    public static readonly BindableProperty WidthProperty =
        BindableProperty.Create(nameof(Width), typeof(GridLength), typeof(MaterialTableColumn), GridLength.Auto);

    public static readonly BindableProperty MinWidthProperty =
        BindableProperty.Create(nameof(MinWidth), typeof(double), typeof(MaterialTableColumn), 64d);

    public static readonly BindableProperty MaxWidthProperty =
        BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(MaterialTableColumn), double.PositiveInfinity);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(MaterialTableColumn), false);

    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(MaterialTableColumn), true);

    public static readonly BindableProperty CanSortProperty =
        BindableProperty.Create(nameof(CanSort), typeof(bool), typeof(MaterialTableColumn), true);

    public static readonly BindableProperty CanFilterProperty =
        BindableProperty.Create(nameof(CanFilter), typeof(bool), typeof(MaterialTableColumn), true);

    public static readonly BindableProperty HorizontalTextAlignmentProperty =
        BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment?), typeof(MaterialTableColumn), null);

    // Formatting & Display Masks
    public static readonly BindableProperty DisplayFormatProperty =
        BindableProperty.Create(nameof(DisplayFormat), typeof(string), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty InputMaskProperty =
        BindableProperty.Create(nameof(InputMask), typeof(string), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty StripMaskOnSaveProperty =
        BindableProperty.Create(nameof(StripMaskOnSave), typeof(bool), typeof(MaterialTableColumn), false);

    public static readonly BindableProperty NullDisplayTextProperty =
        BindableProperty.Create(nameof(NullDisplayText), typeof(string), typeof(MaterialTableColumn), string.Empty);

    // ComboBox Lookup Properties
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty DisplayMemberPathProperty =
        BindableProperty.Create(nameof(DisplayMemberPath), typeof(string), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty SelectedValuePathProperty =
        BindableProperty.Create(nameof(SelectedValuePath), typeof(string), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty IsEditableProperty =
        BindableProperty.Create(nameof(IsEditable), typeof(bool), typeof(MaterialTableColumn), false);
    public static readonly BindableProperty TextColorProperty =
    BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(MaterialTableColumn), double.NaN);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(MaterialTableColumn), null);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(MaterialTableColumn), FontAttributes.None);

    public static readonly BindableProperty FontWeightProperty =
        BindableProperty.Create(nameof(FontWeight), typeof(MaterialTableFontWeight), typeof(MaterialTableColumn), MaterialTableFontWeight.Default);

    public Color? TextColor
    {
        get => (Color?)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    /// <summary>
    /// Leaves NaN by default to inherit MaterialTableView.FontSize.
    /// </summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public string? FontFamily
    {
        get => (string?)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    public MaterialTableFontWeight FontWeight
    {
        get => (MaterialTableFontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string BindingPath
    {
        get => (string)GetValue(BindingPathProperty);
        set => SetValue(BindingPathProperty, value);
    }

    public MaterialTableColumnType ColumnType
    {
        get => (MaterialTableColumnType)GetValue(ColumnTypeProperty);
        set => SetValue(ColumnTypeProperty, value);
    }

    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public double MinWidth
    {
        get => (double)GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool IsVisible
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public bool CanSort
    {
        get => (bool)GetValue(CanSortProperty);
        set => SetValue(CanSortProperty, value);
    }

    public bool CanFilter
    {
        get => (bool)GetValue(CanFilterProperty);
        set => SetValue(CanFilterProperty, value);
    }

    /// <summary>
    /// Overrides cell horizontal alignment (Number defaults to End if null).
    /// </summary>
    public TextAlignment? HorizontalTextAlignment
    {
        get => (TextAlignment?)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    /// <summary>
    /// Standard .NET format string, e.g. "N2", "C2", "P0", "dd/MM/yyyy", "{0:N0} pcs".
    /// </summary>
    public string? DisplayFormat
    {
        get => (string?)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    /// <summary>
    /// Character mask: '0' = digit, 'A' = letter/digit, '#' = any char. E.g. "(000) 000-0000" or "SKU-0000".
    /// </summary>
    public string? InputMask
    {
        get => (string?)GetValue(InputMaskProperty);
        set => SetValue(InputMaskProperty, value);
    }

    public bool StripMaskOnSave
    {
        get => (bool)GetValue(StripMaskOnSaveProperty);
        set => SetValue(StripMaskOnSaveProperty, value);
    }

    public string NullDisplayText
    {
        get => (string)GetValue(NullDisplayTextProperty);
        set => SetValue(NullDisplayTextProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string? DisplayMemberPath
    {
        get => (string?)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string? SelectedValuePath
    {
        get => (string?)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }
}