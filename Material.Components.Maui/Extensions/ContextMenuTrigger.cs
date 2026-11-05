using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui;

using Microsoft.Maui.Controls;



public static class ContextMenuService
{
    // Internal property used to track and clean up the active gesture recognizer
    private static readonly BindableProperty ActiveTapGestureProperty = BindableProperty.CreateAttached(
        "ActiveTapGesture",
        typeof(TapGestureRecognizer),
        typeof(ContextMenuService),
        null
    );

    // 1. Menu Attached Property
    public static readonly BindableProperty MenuProperty = BindableProperty.CreateAttached(
        "Menu",
        typeof(ContextMenu),
        typeof(ContextMenuService),
        null,
        propertyChanged: OnMenuChanged
    );

    public static ContextMenu? GetMenu(BindableObject view) => (ContextMenu?)view.GetValue(MenuProperty);
    public static void SetMenu(BindableObject view, ContextMenu? value) => view.SetValue(MenuProperty, value);

    // 2. Trigger Attached Property
    public static readonly BindableProperty TriggerProperty = BindableProperty.CreateAttached(
        "Trigger",
        typeof(ContextMenuTrigger),
        typeof(ContextMenuService),
        ContextMenuTrigger.Click
    );

    public static ContextMenuTrigger GetTrigger(BindableObject view) => (ContextMenuTrigger)view.GetValue(TriggerProperty);
    public static void SetTrigger(BindableObject view, ContextMenuTrigger value) => view.SetValue(TriggerProperty, value);

    // 3. Placement Attached Property
    public static readonly BindableProperty PlacementProperty = BindableProperty.CreateAttached(
        "Placement",
        typeof(MenuPlacement),
        typeof(ContextMenuService),
        MenuPlacement.Down
    );

    public static MenuPlacement GetPlacement(BindableObject view) => (MenuPlacement)view.GetValue(PlacementProperty);
    public static void SetPlacement(BindableObject view, MenuPlacement value) => view.SetValue(PlacementProperty, value);

    private static void OnMenuChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not View targetView)
            return;

        // 1. Clean up existing gesture recognizer if present
        if (targetView.GetValue(ActiveTapGestureProperty) is TapGestureRecognizer existingGesture)
        {
            targetView.GestureRecognizers.Remove(existingGesture);
            targetView.ClearValue(ActiveTapGestureProperty);
        }

        if (newValue is ContextMenu newMenu)
        {
            // Inherit BindingContext so MVVM commands inside MenuItems continue to work
            newMenu.BindingContext = targetView.BindingContext;
            targetView.BindingContextChanged += (s, e) => newMenu.BindingContext = targetView.BindingContext;

            // 2. Create standard TapGestureRecognizer instance (not a subclass)
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                var trigger = GetTrigger(targetView);
                newMenu.Placement = GetPlacement(targetView);

                // Normal Click / Tap
                if (trigger is ContextMenuTrigger.Click or ContextMenuTrigger.Both)
                {
                    newMenu.Show(targetView);
                }
                // Secondary Click (Mac Catalyst / Mobile)
                else if (e.Buttons == ButtonsMask.Secondary && trigger is ContextMenuTrigger.RightClickOrLongPress or ContextMenuTrigger.Both)
                {
                    var pos = e.GetPosition(targetView);
                    newMenu.Show(targetView, pos.HasValue ? (Point)pos.Value : default);
                }
            };

            targetView.GestureRecognizers.Add(tapGesture);
            targetView.SetValue(ActiveTapGestureProperty, tapGesture);

#if WINDOWS
            // Handle native Desktop Windows Right-Click
            targetView.HandlerChanged += (s, e) =>
            {
                if (targetView.Handler?.PlatformView is Microsoft.UI.Xaml.UIElement winElement)
                {
                    winElement.RightTapped += (sender, args) =>
                    {
                        var trigger = GetTrigger(targetView);
                        if (trigger is ContextMenuTrigger.RightClickOrLongPress or ContextMenuTrigger.Both)
                        {
                            var pos = args.GetPosition(winElement);
                            newMenu.Placement = GetPlacement(targetView);
                            newMenu.Show(targetView, new Point(pos.X, pos.Y));
                        }
                    };
                }
            };
#endif
        }
    }
}