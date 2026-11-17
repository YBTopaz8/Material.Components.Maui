using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui.Handlers;

public partial class TableViewHandler : ViewHandler<MaterialTableView, RecyclerView>
{
    protected override RecyclerView CreatePlatformView() => new(this.Context);

    static partial void MapItemsSource(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapSelectedItems(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapSelectionMode(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapSelectionUnit(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapCornerButtonMode(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapIsReadOnly(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapCanSortColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapCanFilterColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapCanResizeColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapCanReorderColumns(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapShowExportOptions(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapMinColumnWidth(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapMaxColumnWidth(TableViewHandler handler, MaterialTableView virtualView) { }
    static partial void MapAppearance(TableViewHandler handler, MaterialTableView virtualView) { }

    static partial void MapSelectAll(TableViewHandler handler, MaterialTableView virtualView, object? args) { }
    static partial void MapDeselectAll(TableViewHandler handler, MaterialTableView virtualView, object? args) { }
    static partial void MapScrollToItem(TableViewHandler handler, MaterialTableView virtualView, object? args) { }
    static partial void MapGetCsvContent(TableViewHandler handler, MaterialTableView virtualView, object? args) { }
}