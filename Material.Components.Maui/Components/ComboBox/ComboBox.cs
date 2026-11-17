using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Animations;

namespace Material.Components.Maui;

[ContentProperty(nameof(Items))]
public partial class ComboBox
    : TouchGraphicsView,
        IItemsElement<MenuItem>,
        IItemsSourceElement,
        IOutlineElement,
        IFontElement,
        ILabelTextElement,
        IActiveIndicatorElement,
        IElement,
        IBackgroundElement,
        IICommandElement,
        IVisualTreeElement
{
    protected override void ChangeVisualState()
    {
        this.IsVisualStateChanging = true;
        var state = this.ViewState switch
        {
            ViewState.Disabled => "disabled",
            _ => this.IsDropDown ? "drop_down" : "normal",
        };

        VisualStateManager.GoToState(this, state);
        this.IsVisualStateChanging = false;
    }
    public event EventHandler? DropDownOpened;
    public event EventHandler? DropDownClosed;
    public static readonly BindableProperty MenuBackgroundColorProperty = BindableProperty.Create(
    nameof(MenuBackgroundColor),
    typeof(Color),
    typeof(ComboBox),
    default(Color),
    propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
);

    public Color? MenuBackgroundColor
    {
        get => (Color?)this.GetValue(MenuBackgroundColorProperty);
        set => this.SetValue(MenuBackgroundColorProperty, value);
    }
    
    public static readonly BindableProperty MenuFontColorProperty = BindableProperty.Create(
    nameof(MenuFontColor),
    typeof(Color),
    typeof(ComboBox),
    default(Color),
    propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged()
);

    public Color? MenuFontColor
    {
        get => (Color?)this.GetValue(MenuFontColorProperty);
        set => this.SetValue(MenuFontColorProperty, value);
    }

    public static readonly BindableProperty ShowDropDownArrowProperty = BindableProperty.Create(
    nameof(ShowDropDownArrow), typeof(bool), typeof(ComboBox), true,
    propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public bool ShowDropDownArrow
    {
        get => (bool)GetValue(ShowDropDownArrowProperty);
        set => SetValue(ShowDropDownArrowProperty, value);
    }

    public static readonly BindableProperty ShowFloatingLabelProperty = BindableProperty.Create(
        nameof(ShowFloatingLabel), typeof(bool), typeof(ComboBox), true,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public bool ShowFloatingLabel
    {
        get => (bool)GetValue(ShowFloatingLabelProperty);
        set => SetValue(ShowFloatingLabelProperty, value);
    }
    public static readonly BindableProperty ItemsProperty = IItemsElement<MenuItem>.ItemsProperty;
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
    nameof(Placement), typeof(MenuPlacement), typeof(ComboBox), MenuPlacement.Down);

    public MenuPlacement Placement
    {
        get => (MenuPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    public static readonly BindableProperty ItemsSourceProperty =
        IItemsSourceElement.ItemsSourceProperty;

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(ComboBox),
        -1,
        propertyChanged: (bo, ov, nv) =>
        {
            var comboBox = bo as ComboBox;
            if (comboBox is null) return;
            var index = (int)nv;

            MenuItem? selectedMenuItem = null;

            if (index >= 0 && index < comboBox.Items.Count)
            {
                selectedMenuItem = comboBox.Items[index];
                if (selectedMenuItem is not null)
                {
                    selectedMenuItem.FontColor = comboBox.FontColor;

                    comboBox.SelectedItem = selectedMenuItem?.Value ?? selectedMenuItem;


                }                
            }
            else if (index == -1)
            {
                comboBox.SelectedItem = null;
            }

            if (comboBox.Command?.CanExecute(comboBox.CommandParameter ?? index) is true)
                comboBox.Command?.Execute(comboBox.CommandParameter ?? index);

            comboBox.SelectedChanged?.Invoke(comboBox, new SelectedItemChangedEventArgs(selectedItem: selectedMenuItem, index));
            ((IElement)comboBox).OnPropertyChanged();
        }
    );

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(ComboBox),
        propertyChanged: (bo, ov, nv) =>
        {
            var comboBox = bo as ComboBox;
            if(comboBox is null) return;
            var match = comboBox.Items.FirstOrDefault(m => m == nv || m?.Value == nv);
            if (match != null)
                comboBox.SelectedIndex = comboBox.Items.IndexOf(match);
        }
    );

    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty =
        IFontElement.FontIsItalicProperty;

    public static readonly BindableProperty LabelTextProperty = ILabelTextElement.LabelTextProperty;
    public static readonly BindableProperty LabelFontColorProperty =
         ILabelTextElement.LabelFontColorProperty;

    public static readonly BindableProperty ActiveIndicatorHeightProperty =
        IActiveIndicatorElement.ActiveIndicatorHeightProperty;
    public static readonly BindableProperty ActiveIndicatorColorProperty =
        IActiveIndicatorElement.ActiveIndicatorColorProperty;

    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;

    public ObservableCollection<MenuItem?> Items
    {
        get => (ObservableCollection<MenuItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    public IList ItemsSource
    {
        get => (IList)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    void IItemsElement<MenuItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.OldItems != null)
        {
            foreach (MenuItem? item in e.OldItems)
            {
                this.menu.Items.Remove(item);
            }
        }

        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (MenuItem item in e.NewItems)
            {

                this.menu.MenuFontColor = this.MenuFontColor;
                this.menu.Items.Insert(index, item);
                index++;
            }
        }
        ((IElement)this).InvalidateMeasure();
    }
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
    nameof(ItemTemplate), typeof(DataTemplate), typeof(ComboBox));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(ComboBox));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    void IItemsSourceElement.OnItemsSourceCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
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
                if (index < this.Items.Count)
                    this.Items.RemoveAt(index);
            }
        }
        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (var item in e.NewItems) // <-- 'var' instead of 'string'
            {
                string displayText = string.Empty;

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    {
                        var prop = item.GetType().GetProperty(this.DisplayMemberPath);
                        displayText = prop?.GetValue(item)?.ToString() ?? item.ToString() ?? string.Empty;
                    }
                    else
                    {
                        displayText = item.ToString() ?? string.Empty;
                    }
                }
                var menItem=  new MenuItem {  Text = displayText, Value = item };
                
                // Put the raw object (e.g. AudioDevice) in Value!
                this.Items.Insert(index, menItem);
                index++;
            }
        }
    }

    public int SelectedIndex
    {
        get => (int)this.GetValue(SelectedIndexProperty);
        set => this.SetValue(SelectedIndexProperty, value);
    }

    public object? SelectedItem
    {
        get => this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    public new bool IsEnabled
    {
        get => (bool)this.GetValue(IsEnabledProperty);
        set => this.SetValue(IsEnabledProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)this.GetValue(BackgroundColorProperty);
        set => this.SetValue(BackgroundColorProperty, value);
    }

    public Color FontColor
    {
        get => (Color)this.GetValue(FontColorProperty);
        set => this.SetValue(FontColorProperty, value);
    }
    public float FontSize
    {
        get => (float)this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }
    public string FontFamily
    {
        get => (string)this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }
    public FontWeight FontWeight
    {
        get => (FontWeight)this.GetValue(FontWeightProperty);
        set => this.SetValue(FontWeightProperty, value);
    }
    public bool FontIsItalic
    {
        get => (bool)this.GetValue(FontIsItalicProperty);
        set => this.SetValue(FontIsItalicProperty, value);
    }

    public string LabelText
    {
        get => (string)this.GetValue(LabelTextProperty);
        set => this.SetValue(LabelTextProperty, value);
    }
    public Color? LabelFontColor
    {
        get => (Color?)this.GetValue(LabelFontColorProperty);
        set => this.SetValue(LabelFontColorProperty, value);
    }

    public int ActiveIndicatorHeight
    {
        get => (int)this.GetValue(ActiveIndicatorHeightProperty);
        set => this.SetValue(ActiveIndicatorHeightProperty, value);
    }

    public Color ActiveIndicatorColor
    {
        get => (Color)this.GetValue(ActiveIndicatorColorProperty);
        set => this.SetValue(ActiveIndicatorColorProperty, value);
    }

    public Color? OutlineColor
    {
        get => (Color)this.GetValue(OutlineColorProperty);
        set => this.SetValue(OutlineColorProperty, value);
    }

    public int OutlineWidth
    {
        get => (int)this.GetValue(OutlineWidthProperty);
        set => this.SetValue(OutlineWidthProperty, value);
    }

    public event EventHandler<SelectedItemChangedEventArgs>? SelectedChanged;

    bool isDropDown = false;
    internal bool IsDropDown
    {
        get => this.isDropDown;
        private set
        {
            this.isDropDown = value;
            this.ChangeVisualState();
        }
    }

    internal float LabelAnimationPercent { get; private set; } = 1f;

    readonly ContextMenu menu = new();

    public ComboBox()
    {
        this.Items = new ObservableCollection<MenuItem?>();
        this.SetDynamicResource(StyleProperty, "FieldComboBoxStyle");

        this.Drawable = new ComboBoxDrawable(this);

        this.Clicked += this.OnClicked;

        this.menu.Closed += this.OnMenuClosed;
    }

    protected void OnClicked(object? sender, TouchEventArgs e)
    {
        if (Items.Count <1)
        {
            this.OutlineColor = Colors.Red;
            return;
        }

        this.IsDropDown = true;
        this.menu.Placement = this.Placement;

        var popupBg = this.MenuBackgroundColor ?? this.BackgroundColor;
        this.menu.BackgroundColor = popupBg;

        this.menu.Shape = this.Shape;

        this.UpdateMenuWidth();


       

        this.StartLabelTextAnimation();
        this.menu.Show(this);


        this.DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    private void OnMenuClosed(object? sender, object? e)
    {
        this.IsDropDown = false;
        this.StartLabelTextAnimation();
        if (e is not null)
        {


            if (e is int index)
            { 
                this.SelectedIndex = index; 
            }

            else if (e.GetType() == typeof(MenuItem))
            {
                var menItem = e as MenuItem;
                this.SelectedIndex = this.Items.IndexOf(menItem);
            }
        }
        this.DropDownClosed?.Invoke(this, EventArgs.Empty);
    }

    public void StartLabelTextAnimation()
    {
        if (this.SelectedIndex != -1)
        {
            this.Invalidate();
            return;
        }

        this.animationManager ??= this.Handler?
            .MauiContext
            ?.Services
            .GetRequiredService<IAnimationManager>();
        var start = 0f;
        var end = 1f;

        this.animationManager?.Add(
            new Microsoft.Maui.Animations.Animation(
                callback: (progress) =>
                {
                    this.LabelAnimationPercent = start.Lerp(end, progress);
                    this.Invalidate();
                },
                duration: 0.5f,
                easing: Easing.Default
            )
        );
    }

    protected override float GetRippleSize()
    {
        return 0f;
    }

    protected override void StartRippleEffect()
    {
        return;
    }

    protected override void OnParentChanged()
    {
        base.OnParentChanged();

        this.AddLogicalChild(this.menu);
        SetInheritedBindingContext(this.menu, this.BindingContext);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        SetInheritedBindingContext(this.menu, this.BindingContext);
    }

    Size GetMaxItemTextSize()
    {
        var result = Size.Zero;
        foreach (var item in this.Items)
        {
            
            var size = this.GetStringSize(item?.Text);
            if (size.Width > result.Width)
                result.Width = size.Width;
        }
        return result;
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var maxWidth = Math.Min(
            Math.Min(widthConstraint, this.MaximumWidthRequest),
            this.WidthRequest != -1 ? this.WidthRequest : double.PositiveInfinity
        );
        var maxHeight = Math.Min(
            Math.Min(heightConstraint, this.MaximumHeightRequest),
            this.HeightRequest != -1 ? this.HeightRequest : double.PositiveInfinity
        );

        float baseHeight = this.ShowFloatingLabel ? 64f : 36f;
        var targetHeight = this.HeightRequest != -1 ? (float)this.HeightRequest : Math.Min(baseHeight, (float)maxHeight);
        var scale = targetHeight / baseHeight;

        var itemTextSize = this.GetMaxItemTextSize();

        float paddingWidth = this.ShowDropDownArrow ? 64f : 32f;
        var needWidth = paddingWidth * scale + itemTextSize.Width;
        var needHeight = baseHeight * scale;

        var width = this.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxWidth)
            ? maxWidth
            : this.Margin.HorizontalThickness + Math.Max(
                this.MinimumWidthRequest != -1 ? this.MinimumWidthRequest : 0,
                this.WidthRequest == -1 ? Math.Min((float)maxWidth, needWidth) : (float)this.WidthRequest
            );

        var height = this.VerticalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxHeight)
            ? maxHeight
            : this.Margin.VerticalThickness + Math.Max(
                this.MinimumHeightRequest != -1 ? this.MinimumHeightRequest : 0,
                this.HeightRequest == -1 ? Math.Min((float)maxHeight, needHeight) : (float)this.HeightRequest
            );

        this.DesiredSize = new Size(Math.Ceiling(width), Math.Ceiling(height));
        return this.DesiredSize;
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Items != null ? this.Items! : Array.Empty<IVisualTreeElement>().ToList();

    private void UpdateMenuWidth()
    {
        // 1. Get the pill's actual width on screen
        double pillActualWidth = this.Width > 0 ? this.Width : this.DesiredSize.Width;

        // 2. Get the largest width needed by any text item in the menu
        double maxItemWidth = 0;
        foreach (var item in this.menu.Items)
        {
            if (item != null)
                maxItemWidth = Math.Max(maxItemWidth, item.GetDesiredSize().Width);
        }

        // 3. The popup must ALWAYS be AT LEAST as wide as the pill, 
        //    or wider if an item's text requires it!
        this.menu.WidthRequest = Math.Max(pillActualWidth, maxItemWidth + 32);
    }
}
