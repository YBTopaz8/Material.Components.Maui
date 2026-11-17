//using Material.Components.Maui.Components;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Material.Components.Maui.Handlers;

//public partial class TableViewHandler : IViewHandler
//{
//    public static IPropertyMapper<MaterialTableView, TableViewHandler> Mapper { get; set; } =
//        new PropertyMapper<MaterialTableView, TableViewHandler>(ViewMapper)
//        {
//            [nameof(MaterialTableView.ItemsSource)] = MapItemsSource,
//            [nameof(MaterialTableView.Columns)] = MapColumns,
//            [nameof(MaterialTableView.AutoGenerateColumns)] = MapAutoGenerateColumns,
//            [nameof(MaterialTableView.SelectedItem)] = MapSelectedItem,
//            [nameof(MaterialTableView.IsReadOnly)] = MapIsReadOnly,
//        };

//    public static CommandMapper<MaterialTableView, TableViewHandler> CommandMapper { get; set; } =
//        new(ViewCommandMapper);

//    public TableViewHandler() : base(Mapper, CommandMapper) { }

//    public TableViewHandler(IPropertyMapper mapper) : base(mapper ?? Mapper, CommandMapper) { }

//    public TableViewHandler(IPropertyMapper mapper, CommandMapper commandMapper)
//        : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

//    static partial void MapItemsSource(TableViewHandler handler, MaterialTableView virtualView);
//    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView);
//    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView);
//    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView);
//    static partial void MapIsReadOnly(TableViewHandler handler, MaterialTableView virtualView);
//}