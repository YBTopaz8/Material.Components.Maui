//using Material.Components.Maui.Components;
//using Microsoft.Maui.Handlers;
//using Microsoft.UI.Xaml.Data;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using BindingMode = Microsoft.UI.Xaml.Data.BindingMode;
//using WinTableView = WinUI.TableView.TableView;
//using WinTableViewTextColumn = WinUI.TableView.TableViewTextColumn;

//namespace Material.Components.Maui.Handlers;

//public partial class TableViewHandler : ViewHandler<MaterialTableView, WinTableView>
//{
//    protected override WinTableView CreatePlatformView()
//    {
//        return new WinTableView();
//    }

//    protected override void ConnectHandler(WinTableView platformView)
//    {
//        base.ConnectHandler(platformView);
//        platformView.SelectionChanged += OnPlatformSelectionChanged;
//    }

//    protected override void DisconnectHandler(WinTableView platformView)
//    {
//        platformView.SelectionChanged -= OnPlatformSelectionChanged;
//        base.DisconnectHandler(platformView);
//    }

//    private void OnPlatformSelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
//    {
//        if (this.VirtualView is not null && this.VirtualView.SelectedItem != this.PlatformView.SelectedItem)
//        {
//            this.VirtualView.SelectedItem = this.PlatformView.SelectedItem;
//        }
//    }

//    static partial void MapItemsSource(TableViewHandler handler, MaterialTableView virtualView)
//    {
//        handler.PlatformView.ItemsSource = virtualView.ItemsSource;
//    }

//    static partial void MapAutoGenerateColumns(TableViewHandler handler, MaterialTableView virtualView)
//    {
//        handler.PlatformView.AutoGenerateColumns = virtualView.AutoGenerateColumns;
//    }

//    static partial void MapColumns(TableViewHandler handler, MaterialTableView virtualView)
//    {
//        handler.PlatformView.Columns.Clear();

//        foreach (var col in virtualView.Columns)
//        {
//            var winCol = new WinTableViewTextColumn
//            {
//                Header = col.Header,
//                Binding = new Microsoft.UI.Xaml.Data.Binding
//                {
//                    Path = new Microsoft.UI.Xaml.PropertyPath(col.BindingPath),
//                    Mode = col.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay
//                },
//                IsReadOnly = col.IsReadOnly
//            };

//            if (!double.IsNaN(col.Width))
//                winCol.Width = new Microsoft.UI.Xaml.GridLength(col.Width);

//            handler.PlatformView.Columns.Add(winCol);
//        }
//    }

//    static partial void MapSelectedItem(TableViewHandler handler, MaterialTableView virtualView)
//    {
//        if (handler.PlatformView.SelectedItem != virtualView.SelectedItem)
//            handler.PlatformView.SelectedItem = virtualView.SelectedItem;
//    }

//    static partial void MapIsReadOnly(TableViewHandler handler, MaterialTableView virtualView)
//    {
//        handler.PlatformView.IsReadOnly = virtualView.IsReadOnly;
//    }
//}