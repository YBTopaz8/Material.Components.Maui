using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Material.Components.Maui;

public partial class ContextMenu
{
    private static readonly Microsoft.UI.Xaml.Style flyoutStyle;
    private Flyout? container;
    private FrameworkElement? platformAnchor;

    static ContextMenu()
    {
        flyoutStyle = new Microsoft.UI.Xaml.Style(typeof(FlyoutPresenter));
        flyoutStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Control.MarginProperty, 0));
        flyoutStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Control.PaddingProperty, 0));
        flyoutStyle.Setters.Add(new Microsoft.UI.Xaml.Setter(Control.BorderThicknessProperty, 0));
        flyoutStyle.Setters.Add(
            new Microsoft.UI.Xaml.Setter(
                Control.BackgroundProperty,
                Colors.Transparent.ToWindowsColor()
            )
        );
        flyoutStyle.Setters.Add(
            new Microsoft.UI.Xaml.Setter(FlyoutPresenter.IsDefaultShadowEnabledProperty, false)
        );
    }

    private void PlatformShow(View anchor)
    {
        var context = anchor.Handler?.MauiContext;
        if (context is null) return;

        if (this.container == null)
        {
            var platformContent = this.ToPlatform(context);
            this.platformAnchor = anchor.ToPlatform(context);
            platformContent.Width = this.DesiredSize.Width;
            platformContent.Height = this.DesiredSize.Height;

            this.container = new Flyout
            {
                FlyoutPresenterStyle = flyoutStyle,
                LightDismissOverlayMode = LightDismissOverlayMode.Off,
                Content = platformContent,
            };
            this.container.Closed += this.OnClosed;
        }
        var winPlacement = this.Placement switch
        {
            MenuPlacement.Up => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.TopEdgeAlignedLeft,
            MenuPlacement.Left => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.LeftEdgeAlignedTop,
            MenuPlacement.Right => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.RightEdgeAlignedTop,
            _ => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
        };

        this.container.ShowAt(this.platformAnchor, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions
        {
            Placement = winPlacement
        });
    }

    private void PlatformShow(View anchor, Point point)
    {
        var context = anchor.Handler?.MauiContext;
        if(context is null) return;
        if (this.container == null)
        {
            var platformContent = this.ToPlatform(context);
            this.platformAnchor = anchor.ToPlatform(context);
            platformContent.Width = this.DesiredSize.Width;
            platformContent.Height = this.DesiredSize.Height;

            this.container = new Flyout
            {
                FlyoutPresenterStyle = flyoutStyle,
                LightDismissOverlayMode = LightDismissOverlayMode.Off,
                Content = platformContent,
            };

            this.container.Closed += this.OnClosed;
        }


        var winPlacement = this.Placement switch
        {
            MenuPlacement.Up => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.TopEdgeAlignedLeft,
            MenuPlacement.Left => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.LeftEdgeAlignedTop,
            MenuPlacement.Right => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.RightEdgeAlignedTop,
            _ => Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
        };

        this.container.ShowAt(this.platformAnchor, new Microsoft.UI.Xaml.Controls.Primitives.FlyoutShowOptions
        {
            Placement = winPlacement
        ,
                Position = new Windows.Foundation.Point(point.X, point.Y)
            }
        );
    }

    public void Close(object? result = null)
    {
        this.Result = result;
        if(container is not null)
            this.container.Hide();
    }

    public bool IsShowing() => this.container?.IsOpen ?? false;
}
