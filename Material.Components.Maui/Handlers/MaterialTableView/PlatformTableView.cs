#if WINDOWS
using Microsoft.UI.Xaml;
using WinGrid = Microsoft.UI.Xaml.Controls.Grid;
using WinHorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment;
using WinTableView = WinUI.TableView.TableView;
using WinTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using WinTextBlock = Microsoft.UI.Xaml.Controls.TextBlock;
using WinThickness = Microsoft.UI.Xaml.Thickness;
using WinVerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment;
using WinVisibility = Microsoft.UI.Xaml.Visibility;
namespace Material.Components.Maui.Platform;

public sealed partial class PlatformTableView : WinGrid
{
    public WinTableView TableView { get; }
    public WinTextBlock EmptyOverlay { get; }

    public PlatformTableView()
    {
        TableView = new WinTableView();

        EmptyOverlay = new WinTextBlock
        {
            HorizontalAlignment = WinHorizontalAlignment.Center,
            VerticalAlignment = WinVerticalAlignment.Center,
            TextAlignment = WinTextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new WinThickness(24),
            Opacity = 0.6,
            IsHitTestVisible = false,
            Visibility = WinVisibility.Collapsed
        };

        Children.Add(TableView);
        Children.Add(EmptyOverlay);
    }
}
#endif