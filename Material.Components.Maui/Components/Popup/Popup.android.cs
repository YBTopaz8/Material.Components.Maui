#if ANDROID
using Android.App;
using Android.Graphics.Drawables;
using Android.Views;
using Google.Android.Material.Dialog; // <--- Google's Official M3 Dialog Engine
using Microsoft.Maui.Platform;
using Activity = Android.App.Activity;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace Material.Components.Maui;

public partial class Popup
{
    private AndroidX.AppCompat.App.AlertDialog? container; // <--- AndroidX / Material AlertDialog
    private Android.Views.View? platformContent;

    public void PlatformShow(VisualElement? anchor)
    {
        // 1. Resolve Page & Context safely
        var page = anchor as Page ?? FindParentPage(anchor) ?? MauiApplication.Current?.Windows.FirstOrDefault()?.Page;
        var context = anchor?.Handler?.MauiContext ?? page?.Handler?.MauiContext ?? MauiApplication.Current?.Handler?.MauiContext;
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity ?? (context?.Context as Activity);

        if (activity == null || context == null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity != null)
                    PlatformShow(anchor);
            });
            return;
        }

        if (this.container == null)
        {
            this.platformContent = this.Content.ToPlatform(context);

            // ==========================================================
            // PURE MATERIAL 3 DIALOG BUILDER (com.google.android.material)
            // ==========================================================
            var builder = new MaterialAlertDialogBuilder(activity);

            // Set your custom MAUI view inside the Material dialog
            builder.SetView(this.platformContent);

            this.container = builder.Create();
            this.container.ShowEvent += (s, e) => this.Opened?.Invoke(this, EventArgs.Empty);
            this.container.DismissEvent += (s, e) => this.Close();
            this.container.SetCanceledOnTouchOutside(this.DismissOnOutside);

            var window = this.container.Window;
            if (window != null)
            {
                // Material 3 Standard 32% Scrim
                window.AddFlags(WindowManagerFlags.DimBehind);
                window.SetDimAmount(0.32f);

                // If this is a Side Drawer / Sheet (docked to Left/Right/Top/Bottom),
                // remove default dialog insets so it docks flush against the screen edge!
                if (this.HorizontalOptions != LayoutAlignment.Center || this.VerticalOptions != LayoutAlignment.Center)
                {
                    window.SetBackgroundDrawable(new ColorDrawable(Android.Graphics.Color.Transparent));
                    window.DecorView.SetPadding(0, 0, 0, 0);
                    window.SetWindowAnimations(Android.Resource.Style.AnimationTranslucent);
                }
                else
                {
                    // Centered M3 Modal: Use springy Material scale & fade animations
                    window.SetWindowAnimations(Android.Resource.Style.AnimationDialog);
                }
            }
        }

        var dialogWindow = this.container.Window;
        if (dialogWindow != null)
        {
            var density = activity.Resources?.DisplayMetrics?.Density ?? 1f;

            // Gravity alignment (Supports Left-Docked Drawers or Centered Modals)
            var gravity = this.HorizontalOptions switch
            {
                LayoutAlignment.Start => GravityFlags.Left,
                LayoutAlignment.End => GravityFlags.Right,
                _ => GravityFlags.CenterHorizontal
            };

            gravity |= this.VerticalOptions switch
            {
                LayoutAlignment.Start => GravityFlags.Top,
                LayoutAlignment.End => GravityFlags.Bottom,
                _ => GravityFlags.CenterVertical
            };

            dialogWindow.SetGravity(gravity);

            int width = this.Content.WidthRequest > 0
                ? (int)(this.Content.WidthRequest * density)
                : ViewGroup.LayoutParams.WrapContent;

            int height = (this.VerticalOptions == LayoutAlignment.Fill || this.Content.HeightRequest > 0)
                ? (int)((this.Content.HeightRequest > 0 ? this.Content.HeightRequest : activity.Resources?.DisplayMetrics?.HeightPixels ?? 0) * (this.Content.HeightRequest > 0 ? density : 1))
                : ViewGroup.LayoutParams.WrapContent;

            dialogWindow.SetLayout(width, height);
            dialogWindow.Attributes!.X = (int)(this.OffsetX * density);
            dialogWindow.Attributes.Y = (int)(this.OffsetY * density);
        }

        this.container.Show();
    }

    public void Close(object? result = default)
    {
        if (this.container is null) return;
        if (this.container.IsShowing)
        {
            this.container.Dismiss();
        }
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
                if (this.container != null)
                {
                    this.container.DismissEvent -= (s, e) => this.Close();
                    this.container.ShowEvent -= (s, e) => this.Opened?.Invoke(this, EventArgs.Empty);
                    this.container.Dispose();
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