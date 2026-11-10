using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Material.Components.Maui;

[ContentProperty(nameof(Items))]
public partial class SegmentedButton
    : TouchGraphicsView,
        IItemsElement<SegmentedItem>,
        IItemsSourceElement,
        IFontElement,
        IOutlineElement,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IRippleElement,
        IICommandElement,
        IVisualTreeElement,
        IDisposable
{
    protected override void ChangeVisualState()
    {
        if (this.ViewState is ViewState.Hovered or ViewState.Pressed && this.Items.Count > 0)
        {
            var itemWidth = this.Bounds.Width / this.Items.Count;
            var index = (int)(this.LastTouchPoint.X / itemWidth);
            index = Math.Clamp(index, 0, this.Items.Count - 1);

            for (var i = 0; i < this.Items.Count; i++)
            {
                var item = this.Items[i];
                if (item != null) item.ViewState = i == index ? this.ViewState : ViewState.Normal;
            }
        }
        else
        {
            foreach (var item in this.Items)
            {
                if (item != null) item.ViewState = this.ViewState;
            }
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (this.Items != null)
        {
            foreach (var item in this.Items)
            {
                SetInheritedBindingContext(item, this.BindingContext);
            }
        }
    }
    public static readonly BindableProperty CornerRadiusProperty = IOutlineElement.CornerRadiusProperty;
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    public static readonly BindableProperty ItemsProperty =
        IItemsElement<SegmentedItem>.ItemsProperty;

    public static readonly BindableProperty ItemsSourceProperty =
        IItemsSourceElement.ItemsSourceProperty;

    public static readonly BindableProperty MultiSelectModeProperty = BindableProperty.Create(
        nameof(MultiSelectMode),
        typeof(bool),
        typeof(SegmentedButton),
        false
    );

    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty =
        IFontElement.FontIsItalicProperty;

    public static readonly BindableProperty OutlineWidthProperty =
        IOutlineElement.OutlineWidthProperty;
    public static readonly BindableProperty OutlineColorProperty =
        IOutlineElement.OutlineColorProperty;


    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(SegmentedButton), -1,
        propertyChanged: OnSelectedIndexPropertyChanged);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(SegmentedButton), null,
        propertyChanged: OnSelectedItemPropertyChanged);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(SegmentedButton), default(string));
    public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(
       nameof(SelectedBackgroundColor), typeof(Color), typeof(SegmentedButton), null,
       propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());

    public static readonly BindableProperty SelectedFontColorProperty = BindableProperty.Create(
        nameof(SelectedFontColor), typeof(Color), typeof(SegmentedButton), null,
        propertyChanged: (bo, ov, nv) => ((IElement)bo).OnPropertyChanged());


    public ObservableCollection<SegmentedItem?> Items
    {
        get => (ObservableCollection<SegmentedItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    public IList ItemsSource
    {
        get => (IList)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }
    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
    nameof(SelectedItems),
    typeof(IList),
    typeof(SegmentedButton),
    null,
    BindingMode.TwoWay, 
    propertyChanged: OnSelectedItemsPropertyChanged);

    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    private static void OnSelectedItemsPropertyChanged(BindableObject bo, object ov, object nv)
    {
        var view = (SegmentedButton)bo;
        if (!view.MultiSelectMode || nv is not IList newList) return;

        // When the ViewModel updates the list, visually check/uncheck the UI items!
        foreach (var item in view.Items)
        {
            if (item != null)
            {
                var model = item.Value ?? item;
                item.IsSelected = newList.Contains(model);
            }
        }
    }

    public event EventHandler<ValueChangedEventArgs<object>>? SelectedItemChanged;
    public event EventHandler<ValueChangedEventArgs<IEnumerable<object>>>? SelectedItemsChanged;
    void IItemsElement<SegmentedItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.OldItems != null)
        {
            var index = e.OldStartingIndex;
            foreach (SegmentedItem item in e.OldItems)
            {
                this.OnChildRemoved(item, index);
                VisualDiagnostics.OnChildRemoved(this, item, index);
                SetInheritedBindingContext(item, null);
                index++;
            }
        }

        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (SegmentedItem item in e.NewItems)
            {
                this.AddLogicalChild(item);
                if (this.BindingContext != null)
                {
                    SetInheritedBindingContext(item, this.BindingContext);
                }
            }
        }
        ((IElement)this).InvalidateMeasure();
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
            foreach (var item in e.NewItems)
            {
                string displayText = string.Empty;

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    {
                        var prop = item.GetType().GetProperty(this.DisplayMemberPath);
                        displayText = prop?.GetValue(item)?.ToString() ?? item.ToString() ?? "";
                    }
                    else
                    {
                        displayText = item.ToString() ?? "";
                    }
                }

                this.Items.Insert(index, new SegmentedItem { Text = displayText, Value = item });
                index++;
            }
        }
    }
    public Color? SelectedBackgroundColor { get => (Color?)GetValue(SelectedBackgroundColorProperty); set => SetValue(SelectedBackgroundColorProperty, value); }
    public Color? SelectedFontColor { get => (Color?)GetValue(SelectedFontColorProperty); set => SetValue(SelectedFontColorProperty, value); }
    public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public bool MultiSelectMode
    {
        get => (bool)this.GetValue(MultiSelectModeProperty);
        set => this.SetValue(MultiSelectModeProperty, value);
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

    public Color? OutlineColor
    {
        get => (Color?)this.GetValue(OutlineColorProperty);
        set => this.SetValue(OutlineColorProperty, value);
    }

    public int OutlineWidth
    {
        get => (int)this.GetValue(OutlineWidthProperty);
        set => this.SetValue(OutlineWidthProperty, value);
    }


    public SegmentedButton()
    {
        this.Clicked += this.OnClicked;
        this.MoveHoverInteraction += this.OnMoveHoverInteraction;
        this.Drawable = new SegmentedButtonDrawable(this);
    }

    private void OnClicked(object? sender, TouchEventArgs e)
    {

#if WINDOWS
        if (!this.IsLeftClick) return; // Prevent right-click toggles!
#endif
        if (this.Items.Count == 0) return;

        var itemWidth = this.Bounds.Width / this.Items.Count;
        var index = (int)(this.LastTouchPoint.X / itemWidth);
        index = Math.Clamp(index, 0, this.Items.Count - 1);

        var clickedItem = this.Items[index];
        if (clickedItem == null) return;

        if (this.MultiSelectMode)
        {
            var oldSelection = this.Items.Where(i => i?.IsSelected == true).Select(i => i!.Value ?? i).ToList();

            // Toggle freely in Multi-Select
            clickedItem.IsSelected = !clickedItem.IsSelected;
            var model = clickedItem.Value ?? clickedItem;

            // 1. Maintain the bound IList if it exists
            if (this.SelectedItems != null)
            {
                if (clickedItem.IsSelected && !this.SelectedItems.Contains(model))
                    this.SelectedItems.Add(model);
                else if (!clickedItem.IsSelected && this.SelectedItems.Contains(model))
                    this.SelectedItems.Remove(model);
            }

            // 2. Fire the traditional event for Code-Behind users
            var currentSelection = this.Items.Where(i => i?.IsSelected == true).Select(i => i!.Value ?? i).ToList();
            SelectedItemsChanged?.Invoke(this, new ValueChangedEventArgs<IEnumerable<object>>(oldSelection, currentSelection));

            if (this.Command?.CanExecute(this.CommandParameter ?? currentSelection) is true)
                this.Command?.Execute(this.CommandParameter ?? currentSelection);
        }
        else
        {
            // Radio Group Mode: Enforce Single Selection
            if (!clickedItem.IsSelected)
            {
                this.SelectedIndex = index; // Triggers OnSelectedIndexChanged which handles the rest!
            }
        }
    }

    private static void OnSelectedIndexPropertyChanged(BindableObject bo, object ov, object nv)
    {
        var view = (SegmentedButton)bo;
        if (view.MultiSelectMode) return;

        int index = (int)nv;
        object? oldVal = view.SelectedItem ?? view.Items.FirstOrDefault(i => i != null && i.IsSelected)?.Value;
        object? newVal = null;

        for (int i = 0; i < view.Items.Count; i++)
        {
            var item = view.Items[i];
            if (item != null)
            {
                bool shouldSelect = (i == index);
                item.IsSelected = shouldSelect;
                if (shouldSelect) newVal = item.Value ?? item;
            }
        }

        view.SelectedItem = newVal;
        view.SelectedItemChanged?.Invoke(view, new ValueChangedEventArgs<object>(oldVal, newVal));

        if (view.Command?.CanExecute(view.CommandParameter ?? newVal) is true)
            view.Command?.Execute(view.CommandParameter ?? newVal);
    }

    private static void OnSelectedItemPropertyChanged(BindableObject bo, object ov, object nv)
    {
        var view = (SegmentedButton)bo;
        if (view.MultiSelectMode) return;

        var match = view.Items.FirstOrDefault(m => m == nv || m?.Value == nv);
        if (match != null)
            view.SelectedIndex = view.Items.IndexOf(match);
    }

    internal void OnSegmentedItemSelected(SegmentedItem item)
    {
        if (item.IsSelected && !this.MultiSelectMode)
        {
            foreach (var _item in this.Items)
            {
                if (_item is null) continue;
                if (!_item.Equals(item))
                {
                    _item.IsSelected = false;
                }
            }
        }
    }
    protected override void OnMoveHoverInteraction(object? sender, TouchEventArgs e)
    {
        this.LastTouchPoint = e.Touches[0];
        this.ChangeVisualState();
    }
    




    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        var maxWidth = Math.Min(widthConstraint, this.MaximumWidthRequest);
        var maxHeight = Math.Min(heightConstraint, this.MaximumHeightRequest);

        var scale = (this.HeightRequest != -1 ? this.HeightRequest : Math.Min(40f, maxHeight)) / 40f;
        var needHeight = 40f * scale;

        // Automatically divide width equally if constrained, else shrink to fit content
        var width = this.HorizontalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxWidth)
            ? maxWidth
            : this.Margin.HorizontalThickness + (this.WidthRequest != -1 ? this.WidthRequest : 200); // fallback 200

        var height = this.VerticalOptions.Alignment == LayoutAlignment.Fill && !double.IsInfinity(maxHeight)
            ? maxHeight
            : this.Margin.VerticalThickness + (this.HeightRequest == -1 ? needHeight : this.HeightRequest);

        this.DesiredSize = new Size(Math.Ceiling(width), Math.Ceiling(height));
        return this.DesiredSize;
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() => this.Items.ToList()!;

    protected override void Dispose(bool disposing)
    {
        if (!this.disposedValue && disposing)
        {
            this.Clicked -= this.OnClicked;
            this.MoveHoverInteraction -= this.OnMoveHoverInteraction;
            this.Items.CollectionChanged -= (
                (IItemsElement<SegmentedItem>)this
            ).OnItemsCollectionChanged;
            if (this.ItemsSource is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged -= ((IItemsSourceElement)this).OnItemsSourceCollectionChanged;
            }
            ((IIconElement)this).IconPath?.Dispose();
        }
        base.Dispose(disposing);
    }
}
