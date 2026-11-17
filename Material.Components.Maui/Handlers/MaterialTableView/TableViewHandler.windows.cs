using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Material.Components.Maui.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using FontWeights = Microsoft.UI.Text.FontWeights;
using GridUnitType = Microsoft.Maui.GridUnitType;
using ListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using WinTableViewCell = WinUI.TableView.TableViewCell;
using TextAlignment = Microsoft.Maui.TextAlignment;
using VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WinBinding = Microsoft.UI.Xaml.Data.Binding;
using WinBindingMode = Microsoft.UI.Xaml.Data.BindingMode;
using WinComboBox = Microsoft.UI.Xaml.Controls.ComboBox;
using WinControl = Microsoft.UI.Xaml.Controls.Control;
using WinCornerRadius = Microsoft.UI.Xaml.CornerRadius;
using WinFontFamily = Microsoft.UI.Xaml.Media.FontFamily;
using WinGridLength = Microsoft.UI.Xaml.GridLength;
using WinGridUnitType = Microsoft.UI.Xaml.GridUnitType;
using WinHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WinSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;
using WinSetter = Microsoft.UI.Xaml.Setter;
using WinSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WinStyle = Microsoft.UI.Xaml.Style;
using WinTableView = WinUI.TableView.TableView;
using WinTableViewBoundColumn = WinUI.TableView.TableViewBoundColumn;
using WinTableViewCheckBoxColumn = WinUI.TableView.TableViewCheckBoxColumn;
using WinTableViewColumn = WinUI.TableView.TableViewColumn;
using WinTableViewColumnHeader = WinUI.TableView.TableViewColumnHeader;
using WinTableViewComboBoxColumn = WinUI.TableView.TableViewComboBoxColumn;
using WinTableViewCornerButtonMode = WinUI.TableView.TableViewCornerButtonMode;
using WinTableViewDateColumn = WinUI.TableView.TableViewDateColumn;
using WinTableViewHyperlinkColumn = WinUI.TableView.TableViewHyperlinkColumn;
using WinTableViewNumberColumn = WinUI.TableView.TableViewNumberColumn;
using WinTableViewSelectionUnit = WinUI.TableView.TableViewSelectionUnit;
using WinTableViewTextColumn = WinUI.TableView.TableViewTextColumn;
using WinTableViewTimeColumn = WinUI.TableView.TableViewTimeColumn;
using WinTableViewToggleSwitchColumn = WinUI.TableView.TableViewToggleSwitchColumn;
using WinTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using WinTextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using WinTextBox = Microsoft.UI.Xaml.Controls.TextBox;
using WinThickness = Microsoft.UI.Xaml.Thickness;
using WinUpdateSourceTrigger = Microsoft.UI.Xaml.Data.UpdateSourceTrigger;
using WinVisibility = Microsoft.UI.Xaml.Visibility;
using MauiApplication = Microsoft.Maui.Controls.Application;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
namespace Material.Components.Maui.Handlers;

public partial class TableViewHandler : ViewHandler<MaterialTableView, PlatformTableView>
{
    private readonly Dictionary<MaterialTableColumn, WinTableViewColumn> _columnMap = new();
    private INotifyCollectionChanged? _subscribedColumns;
    private bool _isUpdatingSelection;

    private WinTableView Table => PlatformView.TableView;

    protected override PlatformTableView CreatePlatformView() => new();

    protected override void ConnectHandler(PlatformTableView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TableView.SelectionChanged += OnPlatformSelectionChanged;

        if (MauiApplication.Current is not null)
            MauiApplication.Current.RequestedThemeChanged += Current_RequestedThemeChanged; ;

        VirtualView?.RaiseNativeViewReady(platformView.TableView);
    }

    private void Current_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        
        if (VirtualView is not null)
            ApplyMaterialDesign3Theme(this, VirtualView);
    }

    protected override void DisconnectHandler(PlatformTableView platformView)
    {
        platformView.TableView.SelectionChanged -= OnPlatformSelectionChanged;

        if (MauiApplication.Current is not null)
            MauiApplication.Current.RequestedThemeChanged -= Current_RequestedThemeChanged;

        UnsubscribeColumns();
        UntrackColumns();
        base.DisconnectHandler(platformView);
    }

    

    private void OnPlatformSelectionChanged(object sender, WinSelectionChangedEventArgs e)
    {
        if (VirtualView is null || _isUpdatingSelection)
            return;

        try
        {
            _isUpdatingSelection = true;

            if (!Equals(VirtualView.SelectedItem, Table.SelectedItem))
                VirtualView.SelectedItem = Table.SelectedItem;

            if (VirtualView.SelectedItems is IList boundList)
            {
                boundList.Clear();
                foreach (var item in Table.SelectedItems)
                    boundList.Add(item);
            }

            var added = e.AddedItems?.Cast<object>().ToList() ?? [];
            var removed = e.RemovedItems?.Cast<object>().ToList() ?? [];
            VirtualView.RaiseSelectionChanged(added, removed);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    private void SubscribeColumns(IList<MaterialTableColumn>? columns)
    {
        if (ReferenceEquals(_subscribedColumns, columns))
            return;

        UnsubscribeColumns();

        if (columns is INotifyCollectionChanged observable)
        {
            _subscribedColumns = observable;
            _subscribedColumns.CollectionChanged += OnColumnsCollectionChanged;
        }
    }

    private void UnsubscribeColumns()
    {
        if (_subscribedColumns is not null)
        {
            _subscribedColumns.CollectionChanged -= OnColumnsCollectionChanged;
            _subscribedColumns = null;
        }
    }

    private void UntrackColumns()
    {
        foreach (var col in _columnMap.Keys)
            col.PropertyChanged -= OnColumnPropertyChanged;
        _columnMap.Clear();
    }

    private void OnColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (VirtualView is not null)
            RebuildColumns(VirtualView.Columns);
    }

    private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not MaterialTableColumn col || !_columnMap.TryGetValue(col, out var winCol))
            return;

        switch (e.PropertyName)
        {
            case nameof(MaterialTableColumn.Header):
                winCol.Header = col.Header;
                break;
            case nameof(MaterialTableColumn.IsReadOnly):
                winCol.IsReadOnly = col.IsReadOnly;
                break;
            case nameof(MaterialTableColumn.IsVisible):
                winCol.Visibility = col.IsVisible ? WinVisibility.Visible : WinVisibility.Collapsed;
                break;
            case nameof(MaterialTableColumn.CanSort):
                winCol.CanSort = col.CanSort;
                break;
            case nameof(MaterialTableColumn.CanFilter):
                winCol.CanFilter = col.CanFilter;
                break;
            case nameof(MaterialTableColumn.ItemsSource) when winCol is WinTableViewComboBoxColumn combo:
                combo.ItemsSource = col.ItemsSource;
                break;
        }
    }

    private void RebuildColumns(IList<MaterialTableColumn>? columns)
    {
        UntrackColumns();
        Table.Columns.Clear();
        if (columns is null || VirtualView is null)
            return;

        foreach (var col in columns)
        {
            var winCol = CreateWinColumn(VirtualView, col);
            _columnMap[col] = winCol;
            col.PropertyChanged += OnColumnPropertyChanged;
            Table.Columns.Add(winCol);
        }
    }

    private static WinTableViewColumn CreateWinColumn(MaterialTableView table, MaterialTableColumn col)
    {
        bool usesMaskOrFormat = !string.IsNullOrEmpty(col.DisplayFormat) || !string.IsNullOrEmpty(col.InputMask);

        WinTableViewBoundColumn winCol = col.ColumnType switch
        {
            // When DisplayFormat/InputMask is set on Text/Number/Date/Time, render via TextColumn + 2-way converter
            _ when usesMaskOrFormat && col.ColumnType is MaterialTableColumnType.Text
                or MaterialTableColumnType.Number
                or MaterialTableColumnType.Date
                or MaterialTableColumnType.Time => new WinTableViewTextColumn(),

            MaterialTableColumnType.Number => new WinTableViewNumberColumn(),
            MaterialTableColumnType.CheckBox => new WinTableViewCheckBoxColumn(),
            MaterialTableColumnType.ToggleSwitch => new WinTableViewToggleSwitchColumn(),
            MaterialTableColumnType.Date => new WinTableViewDateColumn(),
            MaterialTableColumnType.Time => new WinTableViewTimeColumn(),
            MaterialTableColumnType.Hyperlink => new WinTableViewHyperlinkColumn(),
            MaterialTableColumnType.ComboBox => CreateComboBoxColumn(col),
            _ => new WinTableViewTextColumn()
        };

        winCol.Header = col.Header;
        winCol.IsReadOnly = col.IsReadOnly;
        winCol.Visibility = col.IsVisible ? WinVisibility.Visible : WinVisibility.Collapsed;
        winCol.CanSort = col.CanSort;
        winCol.CanFilter = col.CanFilter;

        if (!string.IsNullOrWhiteSpace(col.BindingPath))
        {
            var binding = new WinBinding
            {
                Path = new PropertyPath(col.BindingPath),
                Mode = col.IsReadOnly ? WinBindingMode.OneWay : WinBindingMode.TwoWay,
                UpdateSourceTrigger = WinUpdateSourceTrigger.PropertyChanged
            };

            if (usesMaskOrFormat)
                binding.Converter = new MaterialTableColumnValueConverter(table, col);

            if (winCol is WinTableViewComboBoxColumn combo && !string.IsNullOrWhiteSpace(col.SelectedValuePath))
                combo.SelectedValueBinding = binding;
            else
                winCol.Binding = binding;
        }

        // Alignment for MD3 numeric/text display & editing
        TextAlignment effectiveAlign = col.HorizontalTextAlignment
            ?? (col.ColumnType == MaterialTableColumnType.Number ? TextAlignment.End : TextAlignment.Start);

        ApplyColumnElementStyles(table,winCol, effectiveAlign, col);

        winCol.Width = col.Width.GridUnitType switch
        {
            GridUnitType.Absolute => new WinGridLength(col.Width.Value, WinGridUnitType.Pixel),
            GridUnitType.Star => new WinGridLength(col.Width.Value, WinGridUnitType.Star),
            _ => new WinGridLength(1, WinGridUnitType.Auto)
        };

        if (col.MinWidth > 0)
            winCol.MinWidth = col.MinWidth;

        if (!double.IsInfinity(col.MaxWidth))
            winCol.MaxWidth = col.MaxWidth;

        return winCol;
    }

    private static WinTableViewComboBoxColumn CreateComboBoxColumn(MaterialTableColumn col)
    {
        var combo = new WinTableViewComboBoxColumn
        {
            ItemsSource = col.ItemsSource,
            IsEditable = col.IsEditable
        };

        if (!string.IsNullOrWhiteSpace(col.DisplayMemberPath) || !string.IsNullOrWhiteSpace(col.SelectedValuePath))
        {
            var editStyle = new WinStyle(typeof(WinComboBox));
            editStyle.Setters.Add(new WinSetter(WinControl.HorizontalAlignmentProperty, WinHorizontalAlignment.Stretch));
            if (!string.IsNullOrWhiteSpace(col.DisplayMemberPath))
                editStyle.Setters.Add(new WinSetter(ItemsControl.DisplayMemberPathProperty, col.DisplayMemberPath));
            if (!string.IsNullOrWhiteSpace(col.SelectedValuePath))
                editStyle.Setters.Add(new WinSetter(Microsoft.UI.Xaml.Controls.Primitives.Selector.SelectedValuePathProperty, col.SelectedValuePath));
            combo.EditingElementStyle = editStyle;
        }

        return combo;
    }
    private static void ApplyColumnElementStyles(
    MaterialTableView table,
    WinTableViewBoundColumn winCol,
    TextAlignment align,
    MaterialTableColumn col)
    {
        bool isDark = MauiApplication.Current?.RequestedTheme == AppTheme.Dark;
        var effectiveColor = col.TextColor ?? table.TextColor ?? Color.FromArgb(isDark ? "#E6E0E9" : "#1D1B20");
        var fgBrush = ToWinBrush(effectiveColor);

        double fontSize = !double.IsNaN(col.FontSize) && col.FontSize > 0 ? col.FontSize : table.FontSize;
        string? fontFamilyName = !string.IsNullOrWhiteSpace(col.FontFamily) ? col.FontFamily : table.FontFamily;

        Windows.UI.Text.FontWeight fontWeight = col.FontWeight != MaterialTableFontWeight.Default
            ? new Windows.UI.Text.FontWeight { Weight = (ushort)col.FontWeight }
            : (col.FontAttributes.HasFlag(FontAttributes.Bold) ? FontWeights.Bold : FontWeights.Normal);

        Windows.UI.Text.FontStyle fontStyle = col.FontAttributes.HasFlag(FontAttributes.Italic)
            ? Windows.UI.Text.FontStyle.Italic
            : Windows.UI.Text.FontStyle.Normal;

        // 1. Per-column CellStyle (applies foreground & font to any column type)
        var colCellStyle = new WinStyle(typeof(WinTableViewCell));
        colCellStyle.Setters.Add(new WinSetter(WinControl.ForegroundProperty, fgBrush));
        colCellStyle.Setters.Add(new WinSetter(WinControl.FontSizeProperty, fontSize));
        colCellStyle.Setters.Add(new WinSetter(WinControl.FontWeightProperty, fontWeight));
        colCellStyle.Setters.Add(new WinSetter(WinControl.FontStyleProperty, fontStyle));
        if (!string.IsNullOrWhiteSpace(fontFamilyName))
            colCellStyle.Setters.Add(new WinSetter(WinControl.FontFamilyProperty, new WinFontFamily(fontFamilyName)));
        winCol.CellStyle = colCellStyle;

        // 2. ElementStyle & EditingElementStyle (directly targets inner TextBlock / TextBox)
        WinTextAlignment winTextAlign = align switch
        {
            TextAlignment.End => WinTextAlignment.Right,
            TextAlignment.Center => WinTextAlignment.Center,
            _ => WinTextAlignment.Left
        };

        WinHorizontalAlignment winHorizAlign = align switch
        {
            TextAlignment.End => WinHorizontalAlignment.Right,
            TextAlignment.Center => WinHorizontalAlignment.Center,
            _ => WinHorizontalAlignment.Left
        };

        if (winCol is WinTableViewTextColumn or WinTableViewNumberColumn or WinTableViewDateColumn or WinTableViewTimeColumn)
        {
            var displayStyle = new WinStyle(typeof(WinTextBlock));
            displayStyle.Setters.Add(new WinSetter(WinTextBlock.ForegroundProperty, fgBrush));
            displayStyle.Setters.Add(new WinSetter(WinTextBlock.FontSizeProperty, fontSize));
            displayStyle.Setters.Add(new WinSetter(WinTextBlock.FontWeightProperty, fontWeight));
            displayStyle.Setters.Add(new WinSetter(WinTextBlock.FontStyleProperty, fontStyle));
            displayStyle.Setters.Add(new WinSetter(WinTextBlock.TextAlignmentProperty, winTextAlign));
            displayStyle.Setters.Add(new WinSetter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
            if (!string.IsNullOrWhiteSpace(fontFamilyName))
                displayStyle.Setters.Add(new WinSetter(WinTextBlock.FontFamilyProperty, new WinFontFamily(fontFamilyName)));
            winCol.ElementStyle = displayStyle;
        }

        if (winCol is WinTableViewTextColumn)
        {
            var editStyle = new WinStyle(typeof(WinTextBox));
            editStyle.Setters.Add(new WinSetter(WinTextBox.TextAlignmentProperty, winTextAlign));
            editStyle.Setters.Add(new WinSetter(WinControl.FontSizeProperty, fontSize));
            editStyle.Setters.Add(new WinSetter(WinControl.FontWeightProperty, fontWeight));
            editStyle.Setters.Add(new WinSetter(WinControl.FontStyleProperty, fontStyle));
            editStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty, WinHorizontalAlignment.Stretch));
            if (!string.IsNullOrWhiteSpace(fontFamilyName))
                editStyle.Setters.Add(new WinSetter(WinControl.FontFamilyProperty, new WinFontFamily(fontFamilyName)));
            winCol.EditingElementStyle = editStyle;
        }
        else if (winCol is WinTableViewCheckBoxColumn or WinTableViewToggleSwitchColumn)
        {
            var displayStyle = new WinStyle(typeof(FrameworkElement));
            displayStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty,
                col.HorizontalTextAlignment.HasValue ? winHorizAlign : WinHorizontalAlignment.Center));
            winCol.ElementStyle = displayStyle;
        }
    }
    //private static void ApplyColumnElementStyles(
    //MaterialTableView table, WinTableViewBoundColumn winCol, TextAlignment align, MaterialTableColumn col)
    //{
    //    WinTextAlignment winTextAlign = align switch
    //    {
    //        TextAlignment.End => WinTextAlignment.Right,
    //        TextAlignment.Center => WinTextAlignment.Center,
    //        _ => WinTextAlignment.Left
    //    };

    //    WinHorizontalAlignment winHorizAlign = align switch
    //    {
    //        TextAlignment.End => WinHorizontalAlignment.Right,
    //        TextAlignment.Center => WinHorizontalAlignment.Center,
    //        _ => WinHorizontalAlignment.Left
    //    };

    //    if (winCol is WinTableViewTextColumn)
    //    {
    //        var displayStyle = new WinStyle(typeof(WinTextBlock));
    //        displayStyle.Setters.Add(new WinSetter(WinTextBlock.TextAlignmentProperty, winTextAlign));
    //        displayStyle.Setters.Add(new WinSetter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
    //        winCol.ElementStyle = displayStyle;

    //        var editStyle = new WinStyle(typeof(WinTextBox));
    //        editStyle.Setters.Add(new WinSetter(WinTextBox.TextAlignmentProperty, winTextAlign));
    //        editStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty, WinHorizontalAlignment.Stretch));
    //        winCol.EditingElementStyle = editStyle;
    //    }
    //    else if (winCol is WinTableViewNumberColumn)
    //    {
    //        var displayStyle = new WinStyle(typeof(WinTextBlock));
    //        displayStyle.Setters.Add(new WinSetter(WinTextBlock.TextAlignmentProperty, winTextAlign));
    //        displayStyle.Setters.Add(new WinSetter(FrameworkElement.VerticalAlignmentProperty, Microsoft.UI.Xaml.VerticalAlignment.Center));
    //        winCol.ElementStyle = displayStyle;
    //    }
    //    else if (winCol is WinTableViewCheckBoxColumn or WinTableViewToggleSwitchColumn)
    //    {
    //        var displayStyle = new WinStyle(typeof(FrameworkElement));
    //        displayStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty,
    //            col.HorizontalTextAlignment.HasValue ? winHorizAlign : WinHorizontalAlignment.Center));
    //        winCol.ElementStyle = displayStyle;
    //    }
    //}

    private static void ApplyMaterialDesign3Theme(TableViewHandler handler, MaterialTableView v)
    {
        bool isDark = MauiApplication.Current?.RequestedTheme == AppTheme.Dark;
        var resolvedSurfaceColor =
       v.SurfaceColor
       ?? v.BackgroundColor
       ?? (v.Background as SolidColorBrush)?.Color
       ?? Color.FromArgb(isDark ? "#141218" : "#FFFBFE");

            var resolvedTextColor = v.TextColor ?? Color.FromArgb(isDark ? "#E6E0E9" : "#1D1B20");

        // MD3 Color Palette (falls back cleanly when custom color BindableProperties aren't set)
        var surface = ToWinBrush(resolvedSurfaceColor);
        var headerBg = ToWinBrush(v.HeaderBackgroundColor ?? Color.FromArgb(isDark ? "#211F26" : "#F3EDF7"));
        var headerFg = ToWinBrush(v.HeaderTextColor ?? Color.FromArgb(isDark ? "#E6E0E9" : "#1D1B20"));
        var altRowBg = ToWinBrush(v.AlternateRowColor ?? Color.FromArgb(isDark ? "#1D1B20" : "#F7F2FA"));
        var textFg = ToWinBrush(resolvedTextColor);
        var outline = ToWinBrush(v.OutlineColor ?? Color.FromArgb(isDark ? "#49454F" : "#CAC4D0"));

        // Outer MD3 Card Container
        var container = handler.PlatformView;
        container.CornerRadius = new WinCornerRadius(v.CornerRadius);
        container.BorderThickness = new WinThickness(v.BorderThickness);
        container.BorderBrush = outline;
        container.Background = surface;

        // Inner TableView MD3 Tokens
        var tv = handler.Table;
        tv.CornerRadius = container.CornerRadius;
        tv.Background = surface;
        tv.Foreground = textFg;
        tv.FontSize = v.FontSize;

        if (!string.IsNullOrWhiteSpace(v.FontFamily))
            tv.FontFamily = new WinFontFamily(v.FontFamily);

        tv.RowMinHeight = v.Density switch
        {
            MaterialTableDensity.Compact => 36d,
            MaterialTableDensity.Comfortable => 56d,
            _ => 48d
        };

        tv.AlternateRowBackground = v.UseAlternatingRowColors ? altRowBg : null;
        tv.AlternateRowForeground = v.UseAlternatingRowColors ? textFg : null;

        var cellStyle = new WinStyle(typeof(WinTableViewCell));
        cellStyle.Setters.Add(new WinSetter(WinControl.ForegroundProperty, textFg));
        cellStyle.Setters.Add(new WinSetter(WinControl.FontSizeProperty, v.FontSize));
        if (!string.IsNullOrWhiteSpace(v.FontFamily))
            cellStyle.Setters.Add(new WinSetter(WinControl.FontFamilyProperty, new WinFontFamily(v.FontFamily)));
        tv.CellStyle = cellStyle;

        // MD3 Column Header Style (SurfaceContainer + SemiBold)
        var headerStyle = new WinStyle(typeof(WinTableViewColumnHeader));
        headerStyle.Setters.Add(new WinSetter(WinControl.BackgroundProperty, headerBg));
        headerStyle.Setters.Add(new WinSetter(WinControl.ForegroundProperty, headerFg));
        headerStyle.Setters.Add(new WinSetter(WinControl.FontWeightProperty, FontWeights.SemiBold));
        headerStyle.Setters.Add(new WinSetter(WinControl.FontSizeProperty, Math.Max(12d, v.FontSize - 1)));
        headerStyle.Setters.Add(new WinSetter(WinControl.PaddingProperty, new WinThickness(12, 10, 12, 10)));
        tv.ColumnHeaderStyle = headerStyle;

        // Gridlines (MD3 uses subtle horizontal hairlines by default)
        tv.HorizontalGridLinesStroke = outline;
        tv.VerticalGridLinesStroke = outline;
        tv.HorizontalGridLinesStrokeThickness = v.GridLinesMode is MaterialTableGridLinesMode.Horizontal or MaterialTableGridLinesMode.All ? 1d : 0d;
        tv.VerticalGridLinesStrokeThickness = v.GridLinesMode is MaterialTableGridLinesMode.Vertical or MaterialTableGridLinesMode.All ? 1d : 0d;
    }

    private static WinSolidColorBrush ToWinBrush(Color c)
    {
        var winColor = Windows.UI.Color.FromArgb(
            (byte)Math.Round(c.Alpha * 255),
            (byte)Math.Round(c.Red * 255),
            (byte)Math.Round(c.Green * 255),
            (byte)Math.Round(c.Blue * 255));
        return new WinSolidColorBrush(winColor);
    }

    #region Property & Command Mappers

    static partial void MapItemsSource(TableViewHandler handler, MaterialTableView virtualView)
    {
        handler.Table.ItemsSource = virtualView.ItemsSource switch
        {
            null => null,
            IList list => list,
            _ => virtualView.ItemsSource.Cast<object>().ToList()
        };
    }

    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView)
    {
        handler.SubscribeColumns(virtualView.Columns);
        handler.RebuildColumns(virtualView.Columns);
    }

    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.AutoGenerateColumns = virtualView.AutoGenerateColumns;

    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView)
    {
        if (handler._isUpdatingSelection) return;
        try
        {
            handler._isUpdatingSelection = true;
            if (!Equals(handler.Table.SelectedItem, virtualView.SelectedItem))
                handler.Table.SelectedItem = virtualView.SelectedItem;
        }
        finally { handler._isUpdatingSelection = false; }
    }

    static partial void MapSelectedItems(TableViewHandler handler, MaterialTableView virtualView)
    {
        if (handler._isUpdatingSelection || virtualView.SelectedItems is null) return;
        try
        {
            handler._isUpdatingSelection = true;
            handler.Table.SelectedItems.Clear();
            foreach (var item in virtualView.SelectedItems)
                handler.Table.SelectedItems.Add(item);
        }
        finally { handler._isUpdatingSelection = false; }
    }

    static partial void MapSelectionMode(TableViewHandler handler, MaterialTableView virtualView)
    {
        handler.Table.SelectionMode = virtualView.SelectionMode switch
        {
            MaterialTableSelectionMode.None => ListViewSelectionMode.None,
            MaterialTableSelectionMode.Multiple => ListViewSelectionMode.Multiple,
            MaterialTableSelectionMode.Extended => ListViewSelectionMode.Extended,
            _ => ListViewSelectionMode.Single
        };
    }

    static partial void MapSelectionUnit(TableViewHandler handler, MaterialTableView virtualView)
    {
        handler.Table.SelectionUnit = virtualView.SelectionUnit switch
        {
            MaterialTableSelectionUnit.Cell => WinTableViewSelectionUnit.Cell,
            MaterialTableSelectionUnit.Row => WinTableViewSelectionUnit.Row,
            _ => WinTableViewSelectionUnit.CellOrRow
        };
    }

    static partial void MapCornerButtonMode(TableViewHandler handler, MaterialTableView virtualView)
    {
        handler.Table.CornerButtonMode = virtualView.CornerButtonMode switch
        {
            MaterialTableCornerButtonMode.None => WinTableViewCornerButtonMode.None,
            MaterialTableCornerButtonMode.SelectAll => WinTableViewCornerButtonMode.SelectAll,
            _ => WinTableViewCornerButtonMode.Options
        };
    }

    static partial void MapIsReadOnly(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.IsReadOnly = virtualView.IsReadOnly;

    static partial void MapCanSortColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.CanSortColumns = virtualView.CanSortColumns;

    static partial void MapCanFilterColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.CanFilterColumns = virtualView.CanFilterColumns;

    static partial void MapCanResizeColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.CanResizeColumns = virtualView.CanResizeColumns;

    static partial void MapCanReorderColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.CanReorderColumns = virtualView.CanReorderColumns;

    static partial void MapShowExportOptions(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.ShowExportOptions = virtualView.ShowExportOptions;

    static partial void MapMinColumnWidth(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.MinColumnWidth = virtualView.MinColumnWidth;

    static partial void MapMaxColumnWidth(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.MaxColumnWidth = virtualView.MaxColumnWidth;

    static partial void MapAppearance(TableViewHandler handler, MaterialTableView virtualView)
    {
        ApplyMaterialDesign3Theme(handler, virtualView);

        if (virtualView.Columns is { Count: > 0 })
            handler.RebuildColumns(virtualView.Columns);
    }
    static partial void MapSelectAll(TableViewHandler handler, MaterialTableView virtualView, object? args)
        => handler.Table.SelectAll();

    static partial void MapDeselectAll(TableViewHandler handler, MaterialTableView virtualView, object? args)
        => handler.Table.DeselectAll();

    static partial void MapScrollToItem(TableViewHandler handler, MaterialTableView virtualView, object? args)
    {
        if (args is not null)
            handler.Table.ScrollIntoView(args);
    }

    static partial void MapGetCsvContent(TableViewHandler handler, MaterialTableView virtualView, object? args)
    {
        if (args is MaterialTableView.CsvExportRequest request)
        {
            request.Result = request.SelectedOnly
                ? handler.Table.GetSelectedContent(request.IncludeHeaders, request.Separator)
                : handler.Table.GetAllContent(request.IncludeHeaders, request.Separator);
        }
    }

    #endregion
}