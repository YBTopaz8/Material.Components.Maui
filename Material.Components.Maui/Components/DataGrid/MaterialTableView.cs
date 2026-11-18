
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Material.Components.Maui;

[ContentProperty(nameof(Columns))]
public partial class MaterialTableView : View
{
    public event EventHandler<MaterialTableCellValueChangedEventArgs>? CellValueChanged;
    public event EventHandler<MaterialTableRowEventArgs>? RowAdded;
    public event EventHandler<MaterialTableRowEventArgs>? RowDeleted;
    public static readonly BindableProperty EmptyTextProperty =
    BindableProperty.Create(nameof(EmptyText), typeof(string), typeof(MaterialTableView), null);

    public string? EmptyText
    {
        get => (string?)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public void ClearFilters() => Handler?.Invoke(nameof(ClearFilters));
    public void ClearSorting() => Handler?.Invoke(nameof(ClearSorting));
    public static readonly BindableProperty AllowUserToAddRowsProperty =
        BindableProperty.Create(nameof(AllowUserToAddRows), typeof(bool), typeof(MaterialTableView), false);

    public static readonly BindableProperty AllowUserToDeleteRowsProperty =
        BindableProperty.Create(nameof(AllowUserToDeleteRows), typeof(bool), typeof(MaterialTableView), false);

    public static readonly BindableProperty NewItemFactoryProperty =
        BindableProperty.Create(nameof(NewItemFactory), typeof(Func<object>), typeof(MaterialTableView), null);

    public bool AllowUserToAddRows
    {
        get => (bool)GetValue(AllowUserToAddRowsProperty);
        set => SetValue(AllowUserToAddRowsProperty, value);
    }

    public bool AllowUserToDeleteRows
    {
        get => (bool)GetValue(AllowUserToDeleteRowsProperty);
        set => SetValue(AllowUserToDeleteRowsProperty, value);
    }


    public Func<object>? NewItemFactory
    {
        get => (Func<object>?)GetValue(NewItemFactoryProperty);
        set => SetValue(NewItemFactoryProperty, value);
    }

    internal void RaiseCellValueChanged(
    object rowItem,
    MaterialTableColumn? column,
    string bindingPath,
    object? oldValue,
    object? newValue,
    bool isNewRow)
    {
        var args = new MaterialTableCellValueChangedEventArgs(
            rowItem, column, bindingPath, oldValue, newValue, isNewRow);

        CellValueChanged?.Invoke(this, args);
        column?.RaiseValueChanged(args);
    }
    internal void RaiseRowAdded(object rowItem)
        => RowAdded?.Invoke(this, new MaterialTableRowEventArgs(rowItem));

    internal void RaiseRowDeleted(object rowItem)
        => RowDeleted?.Invoke(this, new MaterialTableRowEventArgs(rowItem));
    public event EventHandler<MaterialTableSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<MaterialTableCellValidatingEventArgs>? CellValidating;
    public event EventHandler<MaterialTableCellEditedEventArgs>? CellEdited;

    /// <summary>
    /// Optional escape hatch exposing the underlying WinUI.TableView.TableView on Windows.
    /// </summary>
    public event EventHandler<object>? NativeViewReady;

    public MaterialTableView()
    {
        if (Columns is INotifyCollectionChanged observable)
            observable.CollectionChanged += OnColumnsCollectionChanged;
    }

    #region Data & Selection BindableProperties

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MaterialTableView), null);

    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(
            nameof(Columns),
            typeof(IList<MaterialTableColumn>),
            typeof(MaterialTableView),
            defaultValueCreator: _ => new ObservableCollection<MaterialTableColumn>(),
            propertyChanged: OnColumnsPropertyChanged);

    public static readonly BindableProperty AutoGenerateColumnsProperty =
        BindableProperty.Create(nameof(AutoGenerateColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(
            nameof(SelectedItem),
            typeof(object),
            typeof(MaterialTableView),
            null,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(
            nameof(SelectedItems),
            typeof(IList),
            typeof(MaterialTableView),
            null);

    public static readonly BindableProperty SelectionModeProperty =
        BindableProperty.Create(nameof(SelectionMode), typeof(MaterialTableSelectionMode), typeof(MaterialTableView), MaterialTableSelectionMode.Single);

    public static readonly BindableProperty SelectionUnitProperty =
        BindableProperty.Create(nameof(SelectionUnit), typeof(MaterialTableSelectionUnit), typeof(MaterialTableView), MaterialTableSelectionUnit.CellOrRow);

    public static readonly BindableProperty CornerButtonModeProperty =
        BindableProperty.Create(nameof(CornerButtonMode), typeof(MaterialTableCornerButtonMode), typeof(MaterialTableView), MaterialTableCornerButtonMode.Options);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(MaterialTableView), false);

    public static readonly BindableProperty CanSortColumnsProperty =
        BindableProperty.Create(nameof(CanSortColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty CanFilterColumnsProperty =
        BindableProperty.Create(nameof(CanFilterColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty CanResizeColumnsProperty =
        BindableProperty.Create(nameof(CanResizeColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty CanReorderColumnsProperty =
        BindableProperty.Create(nameof(CanReorderColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty ShowExportOptionsProperty =
        BindableProperty.Create(nameof(ShowExportOptions), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty MinColumnWidthProperty =
        BindableProperty.Create(nameof(MinColumnWidth), typeof(double), typeof(MaterialTableView), 64d);

    public static readonly BindableProperty MaxColumnWidthProperty =
        BindableProperty.Create(nameof(MaxColumnWidth), typeof(double), typeof(MaterialTableView), double.PositiveInfinity);

    #endregion

    #region MD3 Styling BindableProperties

    public static readonly BindableProperty DensityProperty =
        BindableProperty.Create(nameof(Density), typeof(MaterialTableDensity), typeof(MaterialTableView), MaterialTableDensity.Standard);

    public static readonly BindableProperty GridLinesModeProperty =
        BindableProperty.Create(nameof(GridLinesMode), typeof(MaterialTableGridLinesMode), typeof(MaterialTableView), MaterialTableGridLinesMode.Horizontal);

    public static readonly BindableProperty UseAlternatingRowColorsProperty =
        BindableProperty.Create(nameof(UseAlternatingRowColors), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(double), typeof(MaterialTableView), 12d);

    public static readonly BindableProperty BorderThicknessProperty =
        BindableProperty.Create(nameof(BorderThickness), typeof(double), typeof(MaterialTableView), 1d);

    public static readonly BindableProperty SurfaceColorProperty =
        BindableProperty.Create(nameof(SurfaceColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty HeaderBackgroundColorProperty =
        BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty HeaderTextColorProperty =
        BindableProperty.Create(nameof(HeaderTextColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty AlternateRowColorProperty =
        BindableProperty.Create(nameof(AlternateRowColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty OutlineColorProperty =
        BindableProperty.Create(nameof(OutlineColor), typeof(Color), typeof(MaterialTableView), null);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(MaterialTableView), 14d);

    public static readonly BindableProperty FontFamilyProperty =
        BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(MaterialTableView), null);

    #endregion

    #region CLR Properties

    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public IList<MaterialTableColumn> Columns { get => (IList<MaterialTableColumn>)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public bool AutoGenerateColumns { get => (bool)GetValue(AutoGenerateColumnsProperty); set => SetValue(AutoGenerateColumnsProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public IList? SelectedItems { get => (IList?)GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }
    public MaterialTableSelectionMode SelectionMode { get => (MaterialTableSelectionMode)GetValue(SelectionModeProperty); set => SetValue(SelectionModeProperty, value); }
    public MaterialTableSelectionUnit SelectionUnit { get => (MaterialTableSelectionUnit)GetValue(SelectionUnitProperty); set => SetValue(SelectionUnitProperty, value); }
    public MaterialTableCornerButtonMode CornerButtonMode { get => (MaterialTableCornerButtonMode)GetValue(CornerButtonModeProperty); set => SetValue(CornerButtonModeProperty, value); }
    public bool IsReadOnly { get => (bool)GetValue(IsReadOnlyProperty); set => SetValue(IsReadOnlyProperty, value); }
    public bool CanSortColumns { get => (bool)GetValue(CanSortColumnsProperty); set => SetValue(CanSortColumnsProperty, value); }
    public bool CanFilterColumns { get => (bool)GetValue(CanFilterColumnsProperty); set => SetValue(CanFilterColumnsProperty, value); }
    public bool CanResizeColumns { get => (bool)GetValue(CanResizeColumnsProperty); set => SetValue(CanResizeColumnsProperty, value); }
    public bool CanReorderColumns { get => (bool)GetValue(CanReorderColumnsProperty); set => SetValue(CanReorderColumnsProperty, value); }
    public bool ShowExportOptions { get => (bool)GetValue(ShowExportOptionsProperty); set => SetValue(ShowExportOptionsProperty, value); }
    public double MinColumnWidth { get => (double)GetValue(MinColumnWidthProperty); set => SetValue(MinColumnWidthProperty, value); }
    public double MaxColumnWidth { get => (double)GetValue(MaxColumnWidthProperty); set => SetValue(MaxColumnWidthProperty, value); }

    public MaterialTableDensity Density { get => (MaterialTableDensity)GetValue(DensityProperty); set => SetValue(DensityProperty, value); }
    public MaterialTableGridLinesMode GridLinesMode { get => (MaterialTableGridLinesMode)GetValue(GridLinesModeProperty); set => SetValue(GridLinesModeProperty, value); }
    public bool UseAlternatingRowColors { get => (bool)GetValue(UseAlternatingRowColorsProperty); set => SetValue(UseAlternatingRowColorsProperty, value); }
    public double CornerRadius { get => (double)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
    public double BorderThickness { get => (double)GetValue(BorderThicknessProperty); set => SetValue(BorderThicknessProperty, value); }
    public Color? SurfaceColor { get => (Color?)GetValue(SurfaceColorProperty); set => SetValue(SurfaceColorProperty, value); }
    public Color? HeaderBackgroundColor { get => (Color?)GetValue(HeaderBackgroundColorProperty); set => SetValue(HeaderBackgroundColorProperty, value); }
    public Color? HeaderTextColor { get => (Color?)GetValue(HeaderTextColorProperty); set => SetValue(HeaderTextColorProperty, value); }
    public Color? AlternateRowColor { get => (Color?)GetValue(AlternateRowColorProperty); set => SetValue(AlternateRowColorProperty, value); }
    public Color? TextColor { get => (Color?)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public Color? OutlineColor { get => (Color?)GetValue(OutlineColorProperty); set => SetValue(OutlineColorProperty, value); }
    public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string? FontFamily { get => (string?)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }

    #endregion

    #region Commands & BindingContext Propagation

    public void SelectAll() => Handler?.Invoke(nameof(SelectAll));
    public void DeselectAll() => Handler?.Invoke(nameof(DeselectAll));
    public void ScrollToItem(object item) => Handler?.Invoke(nameof(ScrollToItem), item);

    public string GetCsvContent(bool includeHeaders = true, char separator = ',', bool selectedOnly = false)
    {
        var request = new CsvExportRequest(includeHeaders, separator, selectedOnly);
        Handler?.Invoke(nameof(GetCsvContent), request);
        return request.Result;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        PropagateBindingContext();
    }

    private void PropagateBindingContext()
    {
        if (Columns is null) return;
        foreach (var col in Columns)
            SetInheritedBindingContext(col, BindingContext);
    }

    private static void OnColumnsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not MaterialTableView table) return;

        if (oldValue is INotifyCollectionChanged oldObs)
            oldObs.CollectionChanged -= table.OnColumnsCollectionChanged;

        if (newValue is INotifyCollectionChanged newObs)
            newObs.CollectionChanged += table.OnColumnsCollectionChanged;

        table.PropagateBindingContext();
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null) return;
        foreach (MaterialTableColumn col in e.NewItems)
            SetInheritedBindingContext(col, BindingContext);
    }

    internal void RaiseSelectionChanged(IReadOnlyList<object> added, IReadOnlyList<object> removed)
        => SelectionChanged?.Invoke(this, new MaterialTableSelectionChangedEventArgs(added, removed));

    internal MaterialTableCellValidatingEventArgs RaiseCellValidating(MaterialTableColumn column, string rawText, object? parsedValue)
    {
        var args = new MaterialTableCellValidatingEventArgs(column, rawText, parsedValue);
        CellValidating?.Invoke(this, args);
        return args;
    }

    internal void RaiseCellEdited(MaterialTableColumn column, object? newValue)
        => CellEdited?.Invoke(this, new MaterialTableCellEditedEventArgs(column, newValue));

    internal void RaiseNativeViewReady(object nativeTableView)
        => NativeViewReady?.Invoke(this, nativeTableView);

    internal sealed class CsvExportRequest(bool includeHeaders, char separator, bool selectedOnly)
    {
        public bool IncludeHeaders { get; } = includeHeaders;
        public char Separator { get; } = separator;
        public bool SelectedOnly { get; } = selectedOnly;
        public string Result { get; set; } = string.Empty;
    }

    #endregion
}