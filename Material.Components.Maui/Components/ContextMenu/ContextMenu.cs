using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace Material.Components.Maui;

public enum ContextMenuTrigger
{
    Click,                  // Tapping / Left-clicking (great for Chips & Dropdown pills)
    RightClickOrLongPress,  // Desktop right-click / Mobile long-press (great for Cards)
    Both
}
[ContentProperty(nameof(Items))]
public partial class ContextMenu
    : TemplatedView,
        IItemsElement<MenuItem>,
        IItemsSourceElement,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IElevationElement,
        IVisualTreeElement,
        IDisposable
{
    ViewState IElement.ViewState => ViewState.Normal;

    void IElement.InvalidateMeasure() { }

    void IElement.OnPropertyChanged() { }

    public View? Caller { get; private set; }

    public event EventHandler<ContextMenuOpenedEventArgs>? Opened;
    public event EventHandler<ContextMenuItemClickedEventArgs>? ItemClicked;

    public static readonly BindableProperty OpenedCommandProperty = BindableProperty.Create(
        nameof(OpenedCommand), typeof(ICommand), typeof(ContextMenu), null);

    public ICommand? OpenedCommand
    {
        get => (ICommand?)GetValue(OpenedCommandProperty);
        set => SetValue(OpenedCommandProperty, value);
    }
    public static readonly BindableProperty MenuFontColorProperty = BindableProperty.Create(
       nameof(MenuFontColor), typeof(Color), typeof(ContextMenu), null,
       propertyChanged: (bo, ov, nv) =>
       {
           ((IElement)bo).OnPropertyChanged();
       });

    public static readonly BindableProperty MenuFontSizeProperty = BindableProperty.Create(
        nameof(MenuFontSize), typeof(float), typeof(ContextMenu), 14f);

    public static readonly BindableProperty MenuFontFamilyProperty = BindableProperty.Create(
        nameof(MenuFontFamily), typeof(string), typeof(ContextMenu), null);

    public static readonly BindableProperty MenuFontWeightProperty = BindableProperty.Create(
        nameof(MenuFontWeight), typeof(FontWeight), typeof(ContextMenu), FontWeight.Regular);

    public Color? MenuFontColor { get => (Color?)GetValue(MenuFontColorProperty); set => SetValue(MenuFontColorProperty, value); }
    public float MenuFontSize { get => (float)GetValue(MenuFontSizeProperty); set => SetValue(MenuFontSizeProperty, value); }
    public string? MenuFontFamily { get => (string?)GetValue(MenuFontFamilyProperty); set => SetValue(MenuFontFamilyProperty, value); }
    public FontWeight MenuFontWeight { get => (FontWeight)GetValue(MenuFontWeightProperty); set => SetValue(MenuFontWeightProperty, value); }
    public static readonly BindableProperty ItemsProperty = IItemsElement<MenuItem>.ItemsProperty;

    public static readonly BindableProperty ItemsSourceProperty =
        IItemsSourceElement.ItemsSourceProperty;

    public static new readonly BindableProperty IsEnabledProperty = IElement.IsEnabledProperty;
    public static new readonly BindableProperty BackgroundColorProperty =
        IBackgroundElement.BackgroundColorProperty;
    public static readonly BindableProperty ShapeProperty = IShapeElement.ShapeProperty;
    public static readonly BindableProperty ElevationProperty = IElevationElement.ElevationProperty;

    public ObservableCollection<MenuItem?> Items
    {
        get => (ObservableCollection<MenuItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
    nameof(Placement), typeof(MenuPlacement), typeof(ContextMenu), MenuPlacement.Down);

    public MenuPlacement Placement
    {
        get => (MenuPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    public IList ItemsSource
    {
        get => (IList)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
    }

    [TypeConverter(typeof(ShapeConverter))]
    public Shape Shape
    {
        get => (Shape)this.GetValue(ShapeProperty);
        set => this.SetValue(ShapeProperty, value);
    }

    public Elevation Elevation
    {
        get => (Elevation)this.GetValue(ElevationProperty);
        set => this.SetValue(ElevationProperty, value);
    }

    void IItemsElement<MenuItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.OldItems != null)
        {
            foreach (MenuItem item in e.OldItems)
            {
                item.Clicked -= this.OnMenuItemClicked;
                var visual = this.PART_Stack?.Children.FirstOrDefault(c => c == item || (c is Grid g && g.Children.Contains(item)));
                if (visual != null)
                    this.PART_Stack?.Remove(visual);
            }
        }

        if (e.NewItems != null)
        {
            var defColor = Application.Current?.PlatformAppTheme == AppTheme.Light ? Colors.Black : Colors.White;
            if (defColor is null)
            {
                defColor = Colors.White;
            }
            var index = e.NewStartingIndex;
            foreach (MenuItem item in e.NewItems)
            {
                var visual = this.WrapMenuItem(item);
                item.FontColor = this.MenuFontColor ?? defColor;
                this.PART_Stack?.Insert(index, visual);
                item.Clicked += this.OnMenuItemClicked;
                index++;
            }
        }
    }

    private void OnMenuItemClicked(object? sender, TouchEventArgs e)
    {
        var item = sender as MenuItem;
        if (item == null) return;

        item.ViewState = ViewState.Normal;

        var model = item.Value ?? item;

        this.Close(item);
        var itemIndx = this.Items.IndexOf(item);
        ItemClicked?.Invoke(this, new ContextMenuItemClickedEventArgs(model, this.Caller, itemIndx));


        var param = this.ItemCommandParameter ?? model;
        if (this.ItemCommand?.CanExecute(param) is true)
        {
            this.ItemCommand.Execute(param);
        }
    }

    void IItemsSourceElement.OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            this.Items.Clear();
            return;
        }
        if (e.OldItems != null)
        {
            var index = e.OldStartingIndex;
            for (int i = 0; i < e.OldItems.Count; i++)
            {
                if (index < this.Items.Count) this.Items.RemoveAt(index);
            }
        }

        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (var item in e.NewItems)
            {
                var menuItem = new MenuItem
                {
                    Text = item?.ToString() ?? string.Empty,
                    Value = item,

                    FontColor = this.MenuFontColor ?? Colors.Red,
                    FontSize = this.MenuFontSize,
                    FontFamily = this.MenuFontFamily,
                    FontWeight = this.MenuFontWeight
                };

                this.Items.Insert(index, menuItem);
                index++;
            }
        }
    }

    public object? Result { get; private set; }
    public static readonly BindableProperty ItemCommandProperty = BindableProperty.Create(
        nameof(ItemCommand), typeof(ICommand), typeof(ContextMenu), null);

    public static readonly BindableProperty ItemCommandParameterProperty = BindableProperty.Create(
        nameof(ItemCommandParameter), typeof(object), typeof(ContextMenu), null);

    public ICommand? ItemCommand { get => (ICommand?)GetValue(ItemCommandProperty); set => SetValue(ItemCommandProperty, value); }
    public object? ItemCommandParameter { get => GetValue(ItemCommandParameterProperty); set => SetValue(ItemCommandParameterProperty, value); }

    // This allows <Button Command="{Binding ShowCommand, Source={x:Reference CtxMenu}}" CommandParameter="{x:Reference MyBtn}" />
    public ICommand ShowCommand { get; private set; }

  
    public EventHandler<object?>? Closed;

    private void OnClosed(object? sender, object e)
    {

        this.Closed?.Invoke(sender, this.Result ?? e);
    }

    private Grid? PART_Root;

    private VerticalStackLayout? PART_Stack;

    public ContextMenu() 
    {
        ShowCommand = new Command<View>(anchorView =>
        {
            if (anchorView != null)
                this.Show(anchorView);
        });
    }

    public void Show(View anchor, Point location = default)
    {
        this.Caller = anchor;
        if (this.DesiredSize == Size.Zero)
            this.MeasureOverride(double.PositiveInfinity, double.PositiveInfinity);

        if (location == default)
            this.PlatformShow(anchor);
        else
            this.PlatformShow(anchor, location);

        Opened?.Invoke(this, new ContextMenuOpenedEventArgs(anchor));

        if (this.OpenedCommand?.CanExecute(anchor) is true)
            this.OpenedCommand.Execute(anchor);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.PART_Root = (Grid)this.GetTemplateChild("PART_Root");
        this.PART_Stack = (VerticalStackLayout)this.GetTemplateChild("PART_Stack");
        if (this.PART_Stack != null && this.Items != null)
        {
            this.PART_Stack.Children.Clear();
            foreach (var item in this.Items)
            {
                if (item != null)
                    this.PART_Stack.Children.Add(this.WrapMenuItem(item));
            }
        }
        this.OnChildAdded(this.PART_Root);
        VisualDiagnostics.OnChildAdded(this, this.PART_Root);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (this.PART_Root != null)
        {
            SetInheritedBindingContext(this.PART_Root, this.BindingContext);
        }
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var maxWidth = 0d;
        foreach (var item in this.Items)
        {
            if (item is null) continue;
            var (w, h) = item.GetDesiredSize();
            maxWidth = Math.Max(maxWidth, w);
        }

        // Always take WidthRequest if set, but expand if content is even wider!
        var targetWidth = this.WidthRequest > 0
            ? Math.Max(this.WidthRequest, maxWidth)
            : maxWidth;

        var totalHeight = 0d;
        foreach (var item in this.Items)
        {
            if (item is null) continue;
            var (_, h) = item.GetDesiredSize();
            totalHeight += h;
        }

        this.DesiredSize = new Size(targetWidth, totalHeight + 16d);
        return this.DesiredSize;
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Items != null ? this.Items! : Array.Empty<IVisualTreeElement>().ToList();

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                foreach (var item in this.Items)
                {
                    if (item is null) continue;
                    item.Clicked -= this.OnMenuItemClicked;
                }
            }
            this.disposedValue = true;
        }
    }
    private View WrapMenuItem(MenuItem item)
    {
        if (item.Content == null)
            return item; // Normal custom-drawn item

        var container = new Grid();

        // Layer 0: The MenuItem canvas (handles touches, ripple, hover background)
        container.Children.Add(item);

        // Layer 1: The custom view on top
        // InputTransparent = true ensures touches pass through to the MenuItem's TouchGraphicsView!
        item.Content.InputTransparent = true;
        container.Children.Add(item.Content);

        return container;
    }


        // Internal property used to track and clean up the active gesture recognizer
        private static readonly BindableProperty ActiveTapGestureProperty = BindableProperty.CreateAttached(
            "ActiveTapGesture",
            typeof(TapGestureRecognizer),
            typeof(ContextMenu),
            null
        );

        // 1. Menu Attached Property
        public static readonly BindableProperty MenuProperty = BindableProperty.CreateAttached(
            "Menu",
            typeof(ContextMenu),
            typeof(ContextMenu),
            null,
            propertyChanged: OnMenuChanged
        );

        public static ContextMenu? GetMenu(BindableObject view) => (ContextMenu?)view.GetValue(MenuProperty);
        public static void SetMenu(BindableObject view, ContextMenu? value) => view.SetValue(MenuProperty, value);

        // 2. Trigger Attached Property
        public static readonly BindableProperty TriggerProperty = BindableProperty.CreateAttached(
            "Trigger",
            typeof(ContextMenuTrigger),
            typeof(ContextMenu),
            ContextMenuTrigger.Click
        );

        public static ContextMenuTrigger GetTrigger(BindableObject view) => (ContextMenuTrigger)view.GetValue(TriggerProperty);
        public static void SetTrigger(BindableObject view, ContextMenuTrigger value) => view.SetValue(TriggerProperty, value);

      

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
    

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
