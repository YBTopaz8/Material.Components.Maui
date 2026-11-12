
using Microsoft.Maui.Handlers;

#if ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#else
using PlatformView = System.Object;
#endif

namespace Material.Components.Maui;




public class PopupHandler : ViewHandler<Popup, PlatformView>
{
    public static IPropertyMapper<Popup, PopupHandler> Mapper = new PropertyMapper<Popup, PopupHandler>(ViewMapper);

    public PopupHandler() : base(Mapper) { }

    protected override PlatformView CreatePlatformView()
    {
#if ANDROID
        var view = new Android.Views.View(Context);
        view.Visibility = Android.Views.ViewStates.Gone; // Invisible 0x0 view
        return view;
#elif WINDOWS
        var view = new Microsoft.UI.Xaml.Controls.Grid 
        { 
            Visibility = Microsoft.UI.Xaml.Visibility.Collapsed, 
            Width = 0, 
            Height = 0 
        };
        return view;
#else
        return new PlatformView();
#endif
    }
}