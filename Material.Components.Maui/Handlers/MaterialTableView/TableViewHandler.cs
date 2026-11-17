
namespace Material.Components.Maui.Handlers;

public partial class TableViewHandler : IViewHandler
{
    public static IPropertyMapper<MaterialTableView, TableViewHandler> Mapper { get; set; } =
        new PropertyMapper<MaterialTableView, TableViewHandler>(ViewMapper)
        {
            [nameof(MaterialTableView.ItemsSource)] = MapItemsSource,
            [nameof(MaterialTableView.Columns)] = MapColumns,
            [nameof(MaterialTableView.AutoGenerateColumns)] = MapAutoGenerateColumns,
            [nameof(MaterialTableView.SelectedItem)] = MapSelectedItem,
            [nameof(MaterialTableView.SelectedItems)] = MapSelectedItems,
            [nameof(MaterialTableView.SelectionMode)] = MapSelectionMode,
            [nameof(MaterialTableView.SelectionUnit)] = MapSelectionUnit,
            [nameof(MaterialTableView.CornerButtonMode)] = MapCornerButtonMode,
            [nameof(MaterialTableView.IsReadOnly)] = MapIsReadOnly,
            [nameof(MaterialTableView.CanSortColumns)] = MapCanSortColumns,
            [nameof(MaterialTableView.CanFilterColumns)] = MapCanFilterColumns,
            [nameof(MaterialTableView.CanResizeColumns)] = MapCanResizeColumns,
            [nameof(MaterialTableView.CanReorderColumns)] = MapCanReorderColumns,
            [nameof(MaterialTableView.ShowExportOptions)] = MapShowExportOptions,
            [nameof(MaterialTableView.MinColumnWidth)] = MapMinColumnWidth,
            [nameof(MaterialTableView.MaxColumnWidth)] = MapMaxColumnWidth,
            [nameof(VisualElement.BackgroundColor)] = MapAppearance,
            [nameof(IView.Background)] = MapAppearance,
            // MD3 Theme & Layout
            [nameof(MaterialTableView.Density)] = MapAppearance,
            [nameof(MaterialTableView.GridLinesMode)] = MapAppearance,
            [nameof(MaterialTableView.UseAlternatingRowColors)] = MapAppearance,
            [nameof(MaterialTableView.CornerRadius)] = MapAppearance,
            [nameof(MaterialTableView.BorderThickness)] = MapAppearance,
            [nameof(MaterialTableView.SurfaceColor)] = MapAppearance,
            [nameof(MaterialTableView.HeaderBackgroundColor)] = MapAppearance,
            [nameof(MaterialTableView.HeaderTextColor)] = MapAppearance,
            [nameof(MaterialTableView.AlternateRowColor)] = MapAppearance,
            [nameof(MaterialTableView.TextColor)] = MapAppearance,
            [nameof(MaterialTableView.OutlineColor)] = MapAppearance,
            [nameof(MaterialTableView.FontSize)] = MapAppearance,
            [nameof(MaterialTableView.FontFamily)] = MapAppearance,
        };

    public static CommandMapper<MaterialTableView, TableViewHandler> CommandMapper { get; set; } =
        new(ViewCommandMapper)
        {
            [nameof(MaterialTableView.SelectAll)] = MapSelectAll,
            [nameof(MaterialTableView.DeselectAll)] = MapDeselectAll,
            [nameof(MaterialTableView.ScrollToItem)] = MapScrollToItem,
            [nameof(MaterialTableView.GetCsvContent)] = MapGetCsvContent,
        };

    public TableViewHandler() : base(Mapper, CommandMapper) { }
    public TableViewHandler(IPropertyMapper mapper) : base(mapper ?? Mapper, CommandMapper) { }
    public TableViewHandler(IPropertyMapper mapper, CommandMapper commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

    static partial void MapItemsSource(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapSelectedItems(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapSelectionMode(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapSelectionUnit(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapCornerButtonMode(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapIsReadOnly(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapCanSortColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapCanFilterColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapCanResizeColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapCanReorderColumns(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapShowExportOptions(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapMinColumnWidth(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapMaxColumnWidth(TableViewHandler handler, MaterialTableView virtualView);
    static partial void MapAppearance(TableViewHandler handler, MaterialTableView virtualView);

    static partial void MapSelectAll(TableViewHandler handler, MaterialTableView virtualView, object? args);
    static partial void MapDeselectAll(TableViewHandler handler, MaterialTableView virtualView, object? args);
    static partial void MapScrollToItem(TableViewHandler handler, MaterialTableView virtualView, object? args);
    static partial void MapGetCsvContent(TableViewHandler handler, MaterialTableView virtualView, object? args);
}