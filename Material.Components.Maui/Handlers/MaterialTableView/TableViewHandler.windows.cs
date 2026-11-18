using Material.Components.Maui.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using WinTableViewBeginningEditEventArgs = WinUI.TableView.TableViewBeginningEditEventArgs;
using WinTableViewCellEditEndedEventArgs = WinUI.TableView.TableViewCellEditEndedEventArgs;
using WinTableViewEditAction = WinUI.TableView.TableViewEditAction;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Dispatching;
using WinCheckBox = Microsoft.UI.Xaml.Controls.CheckBox;
using WinToggleSwitch = Microsoft.UI.Xaml.Controls.ToggleSwitch;
using Windows.System;
using FontWeights = Microsoft.UI.Text.FontWeights;
using GridUnitType = Microsoft.Maui.GridUnitType;
using ListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using MauiApplication = Microsoft.Maui.Controls.Application;
using PropertyChangingEventArgs = System.ComponentModel.PropertyChangingEventArgs;
using SolidColorBrush = Microsoft.Maui.Controls.SolidColorBrush;
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
using WinTableViewCell = WinUI.TableView.TableViewCell;
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
namespace Material.Components.Maui.Handlers;

public partial class TableViewHandler : ViewHandler<MaterialTableView, PlatformTableView>
{
    private readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache = new();
    private readonly Dictionary<MaterialTableColumn, WinTableViewColumn> _columnMap = new();
    private readonly Dictionary<WinTableViewColumn, MaterialTableColumn> _winColumnMap = new();

    private (object? Item, WinTableViewColumn? Column, object? OldValue) _activeEdit;
    private bool _isConnecting;

    private INotifyCollectionChanged? _subscribedItemsSource;
    private ObservableCollection<object>? _proxyItems;
    private object? _draftItem;
    private bool _isSyncingProxy;
    private INotifyCollectionChanged? _subscribedColumns;
    private bool _isUpdatingSelection;

    private WinTableView Table => PlatformView.TableView;

    protected override PlatformTableView CreatePlatformView() => new();
    static partial void MapClearFilters(TableViewHandler handler, MaterialTableView virtualView, object? args)
    => handler.Table.FilterDescriptions.Clear();

    static partial void MapClearSorting(TableViewHandler handler, MaterialTableView virtualView, object? args)
        => handler.Table.SortDescriptions.Clear();

    static partial void MapEmptyText(TableViewHandler handler, MaterialTableView virtualView)
        => handler.UpdateEmptyOverlay();

    private void OnTableItemsVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs args)
        => UpdateEmptyOverlay();

    private void UpdateEmptyOverlay()
    {
        if (VirtualView is null || string.IsNullOrWhiteSpace(VirtualView.EmptyText))
        {
            PlatformView.EmptyOverlay.Visibility = WinVisibility.Collapsed;
            return;
        }

        PlatformView.EmptyOverlay.Text = VirtualView.EmptyText;
        PlatformView.EmptyOverlay.Visibility = Table.Items.Count == 0
            ? WinVisibility.Visible
            : WinVisibility.Collapsed;
    }
    protected override void ConnectHandler(PlatformTableView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TableView.SelectionChanged += OnPlatformSelectionChanged;
        platformView.TableView.PreviewKeyDown += OnTablePreviewKeyDown;

        platformView.TableView.BeginningEdit += OnTableBeginningEdit;
        platformView.TableView.CellEditEnded += OnTableCellEditEnded;
        platformView.TableView.Items.VectorChanged += OnTableItemsVectorChanged;
        

        if (MauiApplication.Current is not null)
            MauiApplication.Current.RequestedThemeChanged += Current_RequestedThemeChanged;

        VirtualView?.RaiseNativeViewReady(platformView.TableView);
    }

    protected override void DisconnectHandler(PlatformTableView platformView)
    {
        platformView.TableView.SelectionChanged -= OnPlatformSelectionChanged;
        platformView.TableView.PreviewKeyDown -= OnTablePreviewKeyDown;
        platformView.TableView.BeginningEdit -= OnTableBeginningEdit;
        platformView.TableView.CellEditEnded -= OnTableCellEditEnded;

        if (MauiApplication.Current is not null)
            MauiApplication.Current.RequestedThemeChanged -= Current_RequestedThemeChanged;

        platformView.TableView.Items.VectorChanged -= OnTableItemsVectorChanged;
        UnsubscribeItemsSource();
        
        UnsubscribeColumns();
        UntrackColumns();
        base.DisconnectHandler(platformView);
    }
    private void Current_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        
        if (VirtualView is not null)
            ApplyMaterialDesign3Theme(this, VirtualView);
    }
    private void OnTableBeginningEdit(object? sender, WinTableViewBeginningEditEventArgs e)
    {
        _activeEdit = default;

        if (e.DataItem is null || e.Column is null)
            return;

        if (!_winColumnMap.TryGetValue(e.Column, out var col) || string.IsNullOrWhiteSpace(col.BindingPath))
            return;

        TryGetPropertyValue(e.DataItem, col.BindingPath, out var oldValue);
        _activeEdit = (e.DataItem, e.Column, oldValue);
    }

    private void OnTableCellEditEnded(object? sender, WinTableViewCellEditEndedEventArgs e)
    {
        var pending = _activeEdit;
        _activeEdit = default;

        if (VirtualView is null || e.EditAction != WinTableViewEditAction.Commit || e.DataItem is null || e.Column is null)
            return;

        if (!_winColumnMap.TryGetValue(e.Column, out var col) || string.IsNullOrWhiteSpace(col.BindingPath))
            return;

        if (!TryGetPropertyValue(e.DataItem, col.BindingPath, out var newValue))
            return;

        object? oldValue = ReferenceEquals(pending.Item, e.DataItem) && ReferenceEquals(pending.Column, e.Column)
            ? pending.OldValue
            : null;

        if (Equals(oldValue, newValue))
            return;

        bool isNewRow = ReferenceEquals(e.DataItem, _draftItem);
        if (isNewRow)
            PromoteDraftRow(e.DataItem);

        VirtualView.RaiseCellValueChanged(e.DataItem, col, col.BindingPath, oldValue, newValue, isNewRow);
    }

    private void OnPlatformSelectionChanged(object sender, WinSelectionChangedEventArgs e)
    {
        if (VirtualView is null || _isUpdatingSelection)
            return;

        try
        {
            _isUpdatingSelection = true;

            var selected = ReferenceEquals(Table.SelectedItem, _draftItem) ? null : Table.SelectedItem;
            if (!Equals(VirtualView.SelectedItem, selected))
                VirtualView.SelectedItem = selected;

            if (VirtualView.SelectedItems is IList boundList)
            {
                boundList.Clear();
                foreach (var item in Table.SelectedItems)
                {
                    if (!ReferenceEquals(item, _draftItem))
                        boundList.Add(item);
                }
            }

            var added = e.AddedItems?.Where(x => !ReferenceEquals(x, _draftItem)).ToList() ?? [];
            var removed = e.RemovedItems?.Where(x => !ReferenceEquals(x, _draftItem)).ToList() ?? [];
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
        _winColumnMap.Clear();
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
            _winColumnMap[winCol] = col;
            col.PropertyChanged += OnColumnPropertyChanged;
            Table.Columns.Add(winCol);
        }
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
            case nameof(MaterialTableColumn.TextColor):
            case nameof(MaterialTableColumn.FontSize):
            case nameof(MaterialTableColumn.FontFamily):
            case nameof(MaterialTableColumn.FontAttributes):
            case nameof(MaterialTableColumn.FontWeight):
                if (VirtualView is not null)
                    RebuildColumns(VirtualView.Columns);
                break;
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
                UpdateSourceTrigger = col.ColumnType is MaterialTableColumnType.Text or MaterialTableColumnType.Number
                            ? WinUpdateSourceTrigger.Default   // Commits once on Enter/Tab/LostFocus
                            : WinUpdateSourceTrigger.PropertyChanged // Commits immediately for CheckBox/ToggleSwitch/ComboBox/Date/Time
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

        if (winCol is WinTableViewTextColumn or WinTableViewNumberColumn or WinTableViewDateColumn or WinTableViewTimeColumn or WinTableViewComboBoxColumn)
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
        else if (winCol is WinTableViewCheckBoxColumn)
        {
            var displayStyle = new WinStyle(typeof(WinCheckBox));
            displayStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty,
                col.HorizontalTextAlignment.HasValue ? winHorizAlign : WinHorizontalAlignment.Center));
            winCol.ElementStyle = displayStyle;
        }
        else if (winCol is WinTableViewToggleSwitchColumn)
        {
            var displayStyle = new WinStyle(typeof(WinToggleSwitch));
            displayStyle.Setters.Add(new WinSetter(FrameworkElement.HorizontalAlignmentProperty,
                col.HorizontalTextAlignment.HasValue ? winHorizAlign : WinHorizontalAlignment.Center));
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
        

    }

    public override void SetVirtualView(IView view)
    {
        _isConnecting = true;
        try
        {
            base.SetVirtualView(view);
        }
        finally
        {
            _isConnecting = false;
        }

        if (VirtualView is not null)
        {
            ApplyMaterialDesign3Theme(this, VirtualView);
            SubscribeColumns(VirtualView.Columns);
            RebuildColumns(VirtualView.Columns);
            BindItemsSource();
            MapSelectedItem(this, VirtualView);
            MapSelectedItems(this, VirtualView);
        }
    }
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
        var headerFg = ToWinBrush(v.HeaderTextColor ?? resolvedTextColor);
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
        handler.PlatformView.EmptyOverlay.Foreground = textFg;
        handler.PlatformView.EmptyOverlay.FontSize = v.FontSize;
        if (!string.IsNullOrWhiteSpace(v.FontFamily))
            handler.PlatformView.EmptyOverlay.FontFamily = new WinFontFamily(v.FontFamily);
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
        if (handler._isConnecting) return;
        handler.BindItemsSource();
    }

    static partial void MapAllowUserToAddRows(TableViewHandler handler, MaterialTableView virtualView)
    {
        if (handler._isConnecting) return;
        handler.BindItemsSource();
    }

    static partial void MapAllowUserToDeleteRows(TableViewHandler handler, MaterialTableView virtualView) { }

    private void BindItemsSource()
    {
        UnsubscribeItemsSource();
        _proxyItems = null;
        _draftItem = null;

        if (VirtualView?.ItemsSource is null)
        {
            Table.ItemsSource = null;
            UpdateEmptyOverlay(); // <-- see #2 below
            return;
        }

        if (VirtualView.ItemsSource is INotifyCollectionChanged observable)
        {
            _subscribedItemsSource = observable;
            _subscribedItemsSource.CollectionChanged += OnSourceCollectionChanged;
        }

        if (VirtualView.AllowUserToAddRows && !VirtualView.IsReadOnly && VirtualView.ItemsSource is IList sourceList)
        {
            _proxyItems = new ObservableCollection<object>();
            foreach (var item in sourceList)
                _proxyItems.Add(item);

            AppendFreshDraftRow();
            Table.ItemsSource = _proxyItems;
        }
        else
        {
            var items = VirtualView.ItemsSource switch
            {
                IList list => list,
                _ => VirtualView.ItemsSource.Cast<object>().ToList()
            };

            Table.ItemsSource = items;
        }

        UpdateEmptyOverlay();
    }

    private void UnsubscribeItemsSource()
    {
        if (_subscribedItemsSource is not null)
        {
            _subscribedItemsSource.CollectionChanged -= OnSourceCollectionChanged;
            _subscribedItemsSource = null;
        }
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isSyncingProxy || _proxyItems is null || VirtualView?.ItemsSource is not IList sourceList)
            return;

        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
        {
            int insertAt = _draftItem is not null ? Math.Max(0, _proxyItems.Count - 1) : _proxyItems.Count;
            foreach (var item in e.NewItems)
                _proxyItems.Insert(insertAt++, item);
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
                _proxyItems.Remove(item);
        }
        else
        {
            _proxyItems.Clear();
            foreach (var item in sourceList)
                _proxyItems.Add(item);

            if (_draftItem is not null)
                _proxyItems.Add(_draftItem);
        }
    }
    private void AppendFreshDraftRow()
    {
        if (_proxyItems is null || VirtualView is null) return;

        object? draft = CreateNewItemInstance();
        if (draft is null) return;

        _draftItem = draft;
        
        _proxyItems.Add(draft);
    }

    private object? CreateNewItemInstance()
    {
        if (VirtualView?.NewItemFactory is { } factory)
            return factory();

        if (VirtualView?.ItemsSource is null)
            return null;

        // Infer T from IEnumerable<T> / ObservableCollection<T>
        var enumerableInterface = VirtualView.ItemsSource.GetType()
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        Type? itemType = enumerableInterface?.GetGenericArguments()[0];
        if (itemType is null || itemType == typeof(object))
            itemType = _proxyItems?.FirstOrDefault(x => x is not null && !ReferenceEquals(x, _draftItem))?.GetType();

        if (itemType is null)
            return null;

        try
        {
            return Activator.CreateInstance(itemType);
        }
        catch
        {
            return null;
        }
    }

    private Dictionary<string, PropertyInfo> GetCachedProperties(Type type)
    {
        if (_propertyCache.TryGetValue(type, out var cached))
            return cached;

        cached = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                     .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

        _propertyCache[type] = cached;
        return cached;
    }




    
    private bool TryGetPropertyValue(object root, string bindingPath, out object? value)
    {
        if (!bindingPath.Contains('.'))
        {
            if (GetCachedProperties(root.GetType()).TryGetValue(bindingPath, out var prop))
            {
                value = prop.GetValue(root);
                return true;
            }
            value = null;
            return false;
        }

        object? current = root;
        foreach (var segment in bindingPath.Split('.'))
        {
            if (current is null)
            {
                value = null;
                return true; // Intermediate object is null -> valid null display value
            }

            if (!GetCachedProperties(current.GetType()).TryGetValue(segment, out var prop))
            {
                value = null;
                return false;
            }

            current = prop.GetValue(current);
        }

        value = current;
        return true;
    }





    private void PromoteDraftRow(object promotedItem)
    {
        if (VirtualView?.ItemsSource is not IList sourceList)
            return;

        try
        {
            _isSyncingProxy = true;
            _draftItem = null;

            // Add to real ItemsSource; promotedItem stays in place inside _proxyItems so Tab navigation keeps working!
            sourceList.Add(promotedItem);
            VirtualView.RaiseRowAdded(promotedItem);

            AppendFreshDraftRow();
        }
        finally
        {
            _isSyncingProxy = false;
        }
    }

    private void OnTablePreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Delete || VirtualView is null || !VirtualView.AllowUserToDeleteRows || VirtualView.IsReadOnly)
            return;

        // Never delete the whole row if the user is currently typing inside an editor!
        var focused = FocusManager.GetFocusedElement(PlatformView.XamlRoot);
        if (focused is TextBox or PasswordBox or RichEditBox or NumberBox or WinComboBox)
            return;

        if (VirtualView.ItemsSource is not IList sourceList || sourceList.IsFixedSize)
            return;

        var toDelete = Table.SelectedItems
            .Cast<object>()
            .Where(x => !ReferenceEquals(x, _draftItem))
            .ToList();

        if (toDelete.Count == 0 && Table.SelectedItem is not null && !ReferenceEquals(Table.SelectedItem, _draftItem))
            toDelete.Add(Table.SelectedItem);

        if (toDelete.Count == 0)
            return;

        e.Handled = true;
        foreach (var item in toDelete)
        {
            sourceList.Remove(item);
            VirtualView.RaiseRowDeleted(item);
        }
    }

    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView)
    {
        if (handler._isConnecting) return;
        handler.SubscribeColumns(virtualView.Columns);
        handler.RebuildColumns(virtualView.Columns);
    }

    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView)
        => handler.Table.AutoGenerateColumns = virtualView.AutoGenerateColumns;

    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView)
    {
        if (handler._isConnecting || handler._isUpdatingSelection) return;
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
        if (handler._isConnecting || handler._isUpdatingSelection || virtualView.SelectedItems is null) return;

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
    {
        handler.Table.IsReadOnly = virtualView.IsReadOnly;
        if (!handler._isConnecting && virtualView.AllowUserToAddRows)
            handler.BindItemsSource();
    }
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
        if (handler._isConnecting) return;
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