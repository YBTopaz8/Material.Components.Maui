#if WINDOWS
using WinGrid = Microsoft.UI.Xaml.Controls.Grid;
using WinTableView = WinUI.TableView.TableView;

namespace Material.Components.Maui.Platform;

public sealed partial class PlatformTableView : WinGrid
{
    public WinTableView TableView { get; }

    public PlatformTableView()
    {
        TableView = new WinTableView();
        Children.Add(TableView);
    }
}
#endif