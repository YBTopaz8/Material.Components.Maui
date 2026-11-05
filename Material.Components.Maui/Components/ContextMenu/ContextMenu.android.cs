using Android.Widget;
using Microsoft.Maui.Platform;

namespace Material.Components.Maui;

public partial class ContextMenu
{
    private PopupWindow? container;
    private Android.Views.View? platformContent;
    private Android.Views.View? platformAnchor;

    private void PlatformShow(View? anchor)
    {
        var context = anchor?.Handler?.MauiContext;
        if (context == null) throw new InvalidOperationException("Anchor view must be attached to a handler.");
        if (anchor == null) throw new ArgumentNullException(nameof(anchor));

        if (this.container == null)
        {
            this.platformContent = this.ToPlatform(context);
            this.platformAnchor = anchor.ToPlatform(context);
            this.container = new PopupWindow(this.platformContent) { OutsideTouchable = true };
            this.container.DismissEvent += this.OnClosed;
            this.container.AnimationStyle = Android.Resource.Style.AnimationDialog;
        }

        var density = this.platformContent!.Context!.Resources!.DisplayMetrics!.Density;
        int menuWidth = (int)(this.DesiredSize.Width * density);
        int menuHeight = (int)(this.DesiredSize.Height * density);
        int anchorWidth = this.platformAnchor!.Width;
        int anchorHeight = this.platformAnchor.Height;

        this.container.Width = menuWidth;
        this.container.Height = menuHeight;

        int xOff = 0;
        int yOff = 0;

        switch (this.Placement)
        {
            case MenuPlacement.Up:
                yOff = -(menuHeight + anchorHeight);
                break;
            case MenuPlacement.Left:
                xOff = -menuWidth;
                yOff = -anchorHeight;
                break;
            case MenuPlacement.Right:
                xOff = anchorWidth;
                yOff = -anchorHeight;
                break;
            case MenuPlacement.Down:
            default:
                xOff = 0;
                yOff = 0;
                break;
        }

        this.container.ShowAsDropDown(this.platformAnchor, xOff, yOff);
    }
    private void PlatformShow(View? anchor, Point point)
    {
        var context = anchor?.Handler?.MauiContext;
        if (context == null)
        {
            throw new InvalidOperationException("Anchor view must be attached to a handler.");
        }
        if (anchor == null)
        {
            throw new ArgumentNullException(nameof(anchor), "Anchor view cannot be null.");
        }
        if (this.container == null)
        {
            this.platformContent = this.ToPlatform(context);
            this.platformAnchor = anchor.ToPlatform(context);
            this.container = new PopupWindow(this.platformContent) { OutsideTouchable = true };
            this.container.DismissEvent += this.OnClosed;

            this.container.AnimationStyle = Android.Resource.Style.AnimationDialog;
        }
        var density = this.platformContent!.Context!.Resources!.DisplayMetrics!.Density;
        int menuWidth = (int)(this.DesiredSize.Width * density);
        int menuHeight = (int)(this.DesiredSize.Height * density);
        int anchorWidth = this.platformAnchor!.Width;
        int anchorHeight = this.platformAnchor.Height;

        this.container.Width = menuWidth;
        this.container.Height = menuHeight;

        var location = new int[2];
        this.platformAnchor!.GetLocationOnScreen(location);

        this.container.ShowAtLocation(
            this.platformAnchor,
            Android.Views.GravityFlags.Left | Android.Views.GravityFlags.Top,
            location[0] + (int)(point.X * density),
            location[1] + (int)(point.Y * density)
        );
    }

    public void Close(object? result = null)
    {
        this.Result = result;
        if(container is not null)
            this.container.Dismiss();
    }

    public bool IsShowing() => this.container?.IsShowing ?? false;
}
