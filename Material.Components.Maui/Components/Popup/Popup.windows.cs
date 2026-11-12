#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Grid = Microsoft.Maui.Controls.Grid;
using Page = Microsoft.Maui.Controls.Page;
using WPopup = Microsoft.UI.Xaml.Controls.Primitives.Popup;
using MauiApplication = Microsoft.Maui.Controls.Application;
using SolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;


namespace Material.Components.Maui;

public partial class Popup
{
    private WPopup? container;
    private ContentPanel? platformAnchor;
    private FrameworkElement? platformOutside;

    public void PlatformShow(VisualElement? anchor)
    {
        var page = anchor as Page ?? FindParentPage(anchor) ?? MauiApplication.Current?.Windows.FirstOrDefault()?.Page;
        var context = anchor?.Handler?.MauiContext ?? page?.Handler?.MauiContext ?? MauiApplication.Current?.Handler?.MauiContext;

        if (context == null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                if (MauiApplication.Current?.Windows.FirstOrDefault()?.Page?.Handler != null)
                    PlatformShow(anchor);
            });
            return;
        }

        // 1. Resolve XamlRoot safely from the Anchor, Page, or Native Window
        var xamlRoot = (anchor?.Handler?.PlatformView as FrameworkElement)?.XamlRoot
            ?? (page?.Handler?.PlatformView as FrameworkElement)?.XamlRoot;

        if (xamlRoot == null)
        {
            var winUIWindow = MauiApplication.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            xamlRoot = (winUIWindow?.Content as FrameworkElement)?.XamlRoot;
        }

        var platformContent = this.Content.ToPlatform(context);

        if (this.container == null)
        {
            var anch = (anchor ?? page)?.ToPlatform(context) as ContentPanel;
            if (anch != null)
                this.platformAnchor = anch;

            var windowWidth = xamlRoot?.Size.Width ?? 1200;
            var windowHeight = xamlRoot?.Size.Height ?? 800;

            // Fluent Smoke Backdrop
            var outside = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                WidthRequest = windowWidth,
                HeightRequest = windowHeight
            };

            var root = new AbsoluteLayout
            {
                Parent = this,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                WidthRequest = windowWidth,
                HeightRequest = windowHeight,
                Children = { outside, this.Content },
            };

            this.platformOutside = outside.ToPlatform(context);
            var platformRoot = root.ToPlatform(context);

            // Native Windows 11 Smoke Brush
            if (Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue("ContentDialogSmokeFill", out var smokeResource)
                && smokeResource is Microsoft.UI.Xaml.Media.Brush smokeBrush)
            {
                ((Panel)platformRoot).Background = smokeBrush;
            }
            else
            {
                ((Panel)platformRoot).Background = new SolidColorBrush(Windows.UI.Color.FromArgb(85, 0, 0, 0));
            }

            this.platformOutside.Tapped += this.OnOutsideTapped;

            // Physical ThemeShadow elevation
            platformContent.Translation = new System.Numerics.Vector3(0, 0, 32);
            platformContent.Shadow = new ThemeShadow();

            this.container = new WPopup
            {
                Child = platformRoot,
                IsLightDismissEnabled = false,
                LightDismissOverlayMode = LightDismissOverlayMode.Off,
            };

            // Fluent Motion Transitions
            var transitions = new TransitionCollection();
            if (this.HorizontalOptions == LayoutAlignment.Start)
            {
                transitions.Add(new EdgeUIThemeTransition
                {
                    Edge = Microsoft.UI.Xaml.Controls.Primitives.EdgeTransitionLocation.Left
                });
            }
            else
            {
                transitions.Add(new PopupThemeTransition());
            }
            this.container.ChildTransitions = transitions;

            this.container.Opened += this.OnContainerOpened;
            this.container.Closed += this.OnContainerClosed;
        }

        // ==============================================================
        // CRITICAL FIX: EXPLICITLY SET XAML ROOT ON THE UNPARENTED POPUP
        // ==============================================================
        if (xamlRoot != null)
        {
            this.container.XamlRoot = xamlRoot;
        }

        platformContent.Measure(new Windows.Foundation.Size(double.PositiveInfinity, double.PositiveInfinity));
        var size = platformContent.DesiredSize;

        var actualWidth = xamlRoot?.Size.Width ?? this.platformAnchor?.ActualWidth ?? 1200;
        var actualHeight = xamlRoot?.Size.Height ?? this.platformAnchor?.ActualHeight ?? 800;

        var x = this.OffsetX + this.HorizontalOptions switch
        {
            LayoutAlignment.Start => 0,
            LayoutAlignment.End => actualWidth - size.Width,
            _ => (actualWidth - size.Width) / 2
        };
        var y = this.OffsetY + this.VerticalOptions switch
        {
            LayoutAlignment.Start => 0,
            LayoutAlignment.End => actualHeight - size.Height,
            _ => (actualHeight - size.Height) / 2
        };

        this.Content.SetValue(AbsoluteLayout.LayoutBoundsProperty, new Rect(x, y, size.Width, size.Height));

        // This will now open cleanly without the COMException!
        this.container.IsOpen = true;
    }

    private void OnOutsideTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (this.DismissOnOutside)
            this.Close();
    }

    private void OnContainerClosed(object? sender, object e) => this.Close();

    private void OnContainerOpened(object? sender, object e) =>
        this.Opened?.Invoke(this, EventArgs.Empty);

    public void Close(object? result = null)
    {
        if (this.container is null) return;
        if (this.container.IsOpen)
            this.container.IsOpen = false;

        if (!this.taskCompletionSource.Task.IsCompleted)
        {
            this.taskCompletionSource.TrySetResult(result);
            this.Closed?.Invoke(this, result);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.Close();
                if (this.platformOutside != null) this.platformOutside.Tapped -= this.OnOutsideTapped;
                if (this.container != null)
                {
                    this.container.Opened -= this.OnContainerOpened;
                    this.container.Closed -= this.OnContainerClosed;
                }
            }
            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
#endif