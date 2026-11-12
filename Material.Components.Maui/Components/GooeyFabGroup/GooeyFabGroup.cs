using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Input;

namespace Material.Components.Maui;


[ContentProperty(nameof(SubFabs))]
public partial class GooeyFabGroup : ContentView, IItemsSourceElement
{
    private readonly Grid _container;
    private readonly Grid _subFabContainer;
    private readonly ContentView _mainFabContainer;
    public event EventHandler? Opened;
    public event EventHandler? Closed;
    public event EventHandler? Opening;
    public event EventHandler? Closing; 
    public event EventHandler<GooeyItemClickedEventArgs>? ItemClicked;
    public static readonly BindableProperty ArcAngleProperty = BindableProperty.Create(
    nameof(ArcAngle), typeof(double), typeof(GooeyFabGroup), 90.0,
    propertyChanged: (bo, ov, nv) => { var g = (GooeyFabGroup)bo; if (g.IsOpen) g.AnimateSubFabsAsync(true); });

    public static readonly BindableProperty ItemSpacingProperty = BindableProperty.Create(
        nameof(ItemSpacing), typeof(double), typeof(GooeyFabGroup), 16.0,
        propertyChanged: (bo, ov, nv) => { var g = (GooeyFabGroup)bo; if (g.IsOpen) g.AnimateSubFabsAsync(true); });

    public double ArcAngle { get => (double)GetValue(ArcAngleProperty); set => SetValue(ArcAngleProperty, value); }
    public double ItemSpacing { get => (double)GetValue(ItemSpacingProperty); set => SetValue(ItemSpacingProperty, value); }
    public static readonly BindableProperty MainFabProperty = BindableProperty.Create(
        nameof(MainFab), typeof(View), typeof(GooeyFabGroup), null,
        propertyChanged: (bo, ov, nv) => ((GooeyFabGroup)bo).OnMainFabChanged((View)nv));

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(GooeyFabGroup), false,
        propertyChanged: (bo, ov, nv) => ((GooeyFabGroup)bo).AnimateSubFabsAsync((bool)nv));

    public static readonly BindableProperty DistanceProperty = BindableProperty.Create(
        nameof(Distance), typeof(double), typeof(GooeyFabGroup), 80.0,
        propertyChanged: (bo, ov, nv) =>
        {
            var gooey = (GooeyFabGroup)bo;
            if (gooey.IsOpen) gooey.AnimateSubFabsAsync(true); // Re-animate if distance changes while open
        });

    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(GooeyPlacement), typeof(GooeyFabGroup), GooeyPlacement.TopLeft,
        propertyChanged: (bo, ov, nv) =>
        {
            var gooey = (GooeyFabGroup)bo;
            if (gooey.IsOpen) gooey.AnimateSubFabsAsync(true);
        });
    public static readonly BindableProperty RotateMainFabOnOpenProperty = BindableProperty.Create(
        nameof(RotateMainFabOnOpen), typeof(bool), typeof(GooeyFabGroup), true);
    public static readonly BindableProperty ItemsSourceProperty = IItemsSourceElement.ItemsSourceProperty;
    public static readonly BindableProperty AutoCloseOnItemClickProperty = BindableProperty.Create(
        nameof(AutoCloseOnItemClick), typeof(bool), typeof(GooeyFabGroup), true);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(GooeyFabGroup), default(string));

    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath), typeof(string), typeof(GooeyFabGroup), default(string));

    public static readonly BindableProperty ItemCommandProperty = BindableProperty.Create(
        nameof(ItemCommand), typeof(ICommand), typeof(GooeyFabGroup), null);
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(GooeyFabGroup), null);
    public View MainFab
    {
        get => (View)GetValue(MainFabProperty);
        set => SetValue(MainFabProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public double Distance
    {
        get => (double)GetValue(DistanceProperty);
        set => SetValue(DistanceProperty, value);
    }

    public GooeyPlacement Placement
    {
        get => (GooeyPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool RotateMainFabOnOpen
    {
        get => (bool)GetValue(RotateMainFabOnOpenProperty);
        set => SetValue(RotateMainFabOnOpenProperty, value);
    }

    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }
    public bool AutoCloseOnItemClick
    {
        get => (bool)GetValue(AutoCloseOnItemClickProperty);
        set => SetValue(AutoCloseOnItemClickProperty, value);
    }

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public string IconMemberPath
    {
        get => (string)GetValue(IconMemberPathProperty);
        set => SetValue(IconMemberPathProperty, value);
    }

    public ICommand? ItemCommand
    {
        get => (ICommand?)GetValue(ItemCommandProperty);
        set => SetValue(ItemCommandProperty, value);
    }

    public ObservableCollection<View> SubFabs { get; } = new();

    public GooeyFabGroup()
    {
        this.SetDynamicResource(StyleProperty, "BottomRightGooeyFabStyle");

        _container = new Grid();

        // Sub-fabs go in the back
        _subFabContainer = new Grid { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

        // Main fab goes in the front
        _mainFabContainer = new ContentView { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

        _container.Children.Add(_subFabContainer);
        _container.Children.Add(_mainFabContainer);
        this.Content = _container;

        SubFabs.CollectionChanged += OnSubFabsChanged;

        // Auto-toggle when clicking the main FAB area
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => { this.IsOpen = !this.IsOpen; };
        _mainFabContainer.GestureRecognizers.Add(tapGesture);
    }

    private void OnMainFabChanged(View newMainFab)
    {
        _mainFabContainer.Content = newMainFab;
        if (newMainFab != null)
        {
            newMainFab.InputTransparent = true; // Let the container handle the click to toggle!
        }
    }

    private void OnSubFabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (View item in e.OldItems)
                _subFabContainer.Children.Remove(item);
        }

        if (e.NewItems != null)
        {
            foreach (View item in e.NewItems)
            {
                item.Opacity = 0; // Hide initially
                item.InputTransparent = true;
                item.HorizontalOptions = LayoutOptions.Center;
                item.VerticalOptions = LayoutOptions.Center;
                _subFabContainer.Children.Add(item);
            }
        }
    }
    void IItemsSourceElement.OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.SubFabs.Clear();
            return;
        }

        if (e.OldItems != null)
        {
            for (int i = 0; i < e.OldItems.Count; i++)
            {
                if (e.OldStartingIndex < this.SubFabs.Count)
                    this.SubFabs.RemoveAt(e.OldStartingIndex);
            }
        }

        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (var item in e.NewItems)
            {
                View? visual = null;

                // 1. If user provided an ItemTemplate, use it!
                if (this.ItemTemplate != null)
                {
                    visual = this.ItemTemplate.CreateContent() as View;
                    if (visual != null)
                        visual.BindingContext = item;
                }
                // 2. If it's already a View
                else if (item is View v)
                {
                    visual = v;
                }
                // 3. Fallback: Automatically create a Button/FAB for strings or objects!
                else if (item != null)
                {
                    string text = string.Empty;
                    string icon = string.Empty;

                    if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    {
                        var prop = item.GetType().GetProperty(this.DisplayMemberPath);
                        text = prop?.GetValue(item)?.ToString() ?? item.ToString() ?? "";
                    }
                    else
                    {
                        text = item.ToString() ?? "";
                    }

                    if (!string.IsNullOrEmpty(this.IconMemberPath))
                    {
                        var iconProp = item.GetType().GetProperty(this.IconMemberPath);
                        icon = iconProp?.GetValue(item)?.ToString() ?? "";
                    }

                    // Pre-made mini button
                    var autoBtn = new Button
                    {
                        Text = text,
                        IconData = icon,
                        HeightRequest = 40,
                        CornerRadius = 20,
                        BindingContext = item
                    };

                    visual = autoBtn;
                }

                if (visual != null)
                {
                    this.WireUpSubItemClick(visual, item);
                    this.SubFabs.Insert(index, visual);
                    index++;
                }
            }
        }
    }
    private void WireUpSubItemClick(View visual, object? rawItem)
    {
        // If it's a TouchGraphicsView (Button, FAB, Chip, Card)
        if (visual is TouchGraphicsView tgv)
        {
            tgv.Clicked += (s, e) => HandleItemExecution(rawItem, visual);
        }
        // If it's a standard MAUI Button
        else if (visual is Microsoft.Maui.Controls.Button btn)
        {
            btn.Clicked += (s, e) => HandleItemExecution(rawItem, visual);
        }
        else
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) => HandleItemExecution(rawItem, visual);
            visual.GestureRecognizers.Add(tap);
        }
    }
    private void HandleItemExecution(object? rawItem, View visual)
    {
        ItemClicked?.Invoke(this, new GooeyItemClickedEventArgs(rawItem, visual));

        if (ItemCommand?.CanExecute(rawItem) is true)
            ItemCommand.Execute(rawItem);

        if (AutoCloseOnItemClick)
            this.IsOpen = false;
    }

    private async void AnimateSubFabsAsync(bool isOpen)
    {
        int count = SubFabs.Count;
        if (count == 0) return;

        if (isOpen) Opening?.Invoke(this, EventArgs.Empty);
        else Closing?.Invoke(this, EventArgs.Empty);

        if (this.RotateMainFabOnOpen && this.MainFab != null)
        {
            _ = this.MainFab.RotateToAsync(isOpen ? 45 : 0, 250, Easing.CubicOut);
        }

        // Convert ArcAngle (e.g. 90 or 120 deg) to Radians
        double totalArcRadians = Math.PI * (this.ArcAngle / 180.0);
        double angleStep = count > 1 ? totalArcRadians / (count - 1) : 0;

        var tasks = new List<Task>();

        for (int i = 0; i < count; i++)
        {
            var child = SubFabs[i];
            child.CancelAnimations();

            if (isOpen)
            {
                child.InputTransparent = false;

                double targetX = 0;
                double targetY = 0;

                // 1. Linear Directions (Using Distance + ItemSpacing!)
                double linearOffset = (i + 1) * (this.Distance + this.ItemSpacing);

                switch (Placement)
                {
                    case GooeyPlacement.Up:
                        targetX = 0; targetY = -linearOffset; break;
                    case GooeyPlacement.Down:
                        targetX = 0; targetY = linearOffset; break;
                    case GooeyPlacement.Left:
                        targetX = -linearOffset; targetY = 0; break;
                    case GooeyPlacement.Right:
                        targetX = linearOffset; targetY = 0; break;

                    // 2. Arc / Fan Placement
                    default:
                        double angle = count > 1 ? i * angleStep : (totalArcRadians / 2.0);
                        double rawX = this.Distance * Math.Cos(angle);
                        double rawY = this.Distance * Math.Sin(angle);

                        switch (Placement)
                        {
                            case GooeyPlacement.TopLeft:
                                targetX = -rawX; targetY = -rawY; break;
                            case GooeyPlacement.TopRight:
                                targetX = rawX; targetY = -rawY; break;
                            case GooeyPlacement.BottomLeft:
                                targetX = -rawX; targetY = rawY; break;
                            case GooeyPlacement.BottomRight:
                                targetX = rawX; targetY = rawY; break;
                        }
                        break;
                }

                int delay = i * 25;
                tasks.Add(Task.Run(async () =>
                {
                    if (delay > 0) await Task.Delay(delay);
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        _ = child.FadeToAsync(1, 180);
                        await child.TranslateToAsync(targetX, targetY, 450, Easing.SpringOut);
                    });
                }));
            }
            else
            {
                child.InputTransparent = true;
                tasks.Add(child.FadeToAsync(0, 150));
                tasks.Add(child.TranslateToAsync(0, 0, 200, Easing.CubicInOut));
            }
        }

        await Task.WhenAll(tasks);

        if (isOpen) Opened?.Invoke(this, EventArgs.Empty);
        else Closed?.Invoke(this, EventArgs.Empty);
    }
}
