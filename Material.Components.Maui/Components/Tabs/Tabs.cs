using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections;
namespace Material.Components.Maui;

[ContentProperty(nameof(Items))]
public partial class Tabs
    : TemplatedView,
        IItemsElement<TabItem>,
        IICommandElement, IItemsSourceElement,
        IVisualTreeElement,
        IStyleElement
{
    public static readonly BindableProperty ItemsProperty = IItemsElement<TabItem>.ItemsProperty;

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(TabItem),
        typeof(Tabs),
        propertyChanged: (bo, ov, nv) =>
        {
            var tabs = bo as Tabs;
            if (tabs == null)
                return;
            var item = (TabItem)nv;

            foreach (var tabItem in tabs.Items)
            {
                if(tabItem == null)
                    continue;   
                var s = item.Equals(tabItem);
                tabItem.IsActived = item.Equals(tabItem);
            }
            if (item != null && tabs.Items.Contains(item))
            {
                int newIndex = tabs.Items.IndexOf(item);
                if (tabs.SelectedIndex != newIndex)
                    tabs.SelectedIndex = newIndex;
            }
            if (tabs.PART_Content != null && item?.Content != null)
            {
                // Cancel any running animations
                item.Content.CancelAnimations();

                // Set opacity to 0, swap the content, then fade it in smoothly!
                item.Content.Opacity = 0;
                tabs.PART_Content.Content = item.Content;
                item.Content.FadeToAsync(1, 250, Easing.CubicOut);
            }
            // ---------------------------------

            tabs.SelectedItemChanged?.Invoke(tabs, new SelectedItemChangedArgs<TabItem>(item));

            if (tabs.Command?.CanExecute(tabs.CommandParameter ?? item) is true)
                tabs.Command?.Execute(tabs.CommandParameter ?? item);
        
        }
    );

    public static readonly BindableProperty ItemsSourceProperty = IItemsSourceElement.ItemsSourceProperty;
    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(Tabs), default(string)); // or typeof(NavigationBar)
    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath), typeof(string), typeof(Tabs), default(string));

    public IList ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
    public string IconMemberPath { get => (string)GetValue(IconMemberPathProperty); set => SetValue(IconMemberPathProperty, value); }



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
                string iconData = string.Empty;

                if (item != null)
                {
                    if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                    {
                        var prop = item.GetType().GetProperty(this.DisplayMemberPath);
                        displayText = prop?.GetValue(item)?.ToString() ?? item.ToString() ?? "";
                    }
                    else displayText = item.ToString() ?? "";

                    if (!string.IsNullOrEmpty(this.IconMemberPath))
                    {
                        var prop = item.GetType().GetProperty(this.IconMemberPath);
                        iconData = prop?.GetValue(item)?.ToString() ?? "";
                    }
                }

                // Create TabItem (or NavigationBarItem) automatically!
                var newItem = new TabItem
                {
                    Text = displayText,
                    IconData = iconData,
                    BindingContext = item
                };

                this.Items.Insert(index, newItem);
                index++;
            }
        }
    }
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
    nameof(SelectedIndex), typeof(int), typeof(Tabs), -1, 
    propertyChanged: (bo, ov, nv) =>
    {
        var view = bo as Tabs; 
        if (view == null) return;

        int index = (int)nv;
        if (index >= 0 && index < view.Items.Count)
        {
            view.SelectedItem = view.Items[index];
        }
    });

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }
    public static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public static readonly BindableProperty CommandParameterProperty =
        IICommandElement.CommandParameterProperty;

    public static readonly BindableProperty ItemStyleProperty = BindableProperty.Create(
        nameof(ItemStyle),
        typeof(ItemStyle),
        typeof(Tabs)
    );

    public static readonly BindableProperty DynamicStyleProperty =
        IStyleElement.DynamicStyleProperty;

    public string DynamicStyle
    {
        get => (string)this.GetValue(DynamicStyleProperty);
        set => this.SetValue(DynamicStyleProperty, value);
    }

    public ObservableCollection<TabItem?> Items
    {
        get => (ObservableCollection<TabItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    void IItemsElement<TabItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
        {
            if (this.Items?[e.NewStartingIndex]?.IsActived == true)
                this.SelectedItem = this.Items[e.NewStartingIndex];

            this.SelectedItem ??= this.Items?[e.NewStartingIndex];
        }
    }

    public TabItem? SelectedItem
    {
        get => (TabItem?)this.GetValue(SelectedItemProperty);
        set => this.SetValue(SelectedItemProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => this.GetValue(CommandParameterProperty);
        set => this.SetValue(CommandParameterProperty, value);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public ItemStyle ItemStyle
    {
        get => (ItemStyle)this.GetValue(ItemStyleProperty);
        set => this.SetValue(ItemStyleProperty, value);
    }

    public event EventHandler<SelectedItemChangedArgs<TabItem>>? SelectedItemChanged;

    private Grid? PART_Root;
    private ContentPresenter? PART_Content;
    public Tabs()
    {
        this.SetDynamicResource(StyleProperty, "PrimaryTabsStyle");
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.PART_Root = (Grid)this.GetTemplateChild("PART_Root");
        this.PART_Content = (ContentPresenter)this.GetTemplateChild("PART_Content");
       
        this.OnChildAdded(this.PART_Root);
        VisualDiagnostics.OnChildAdded(this, this.PART_Root);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        SetInheritedBindingContext(this.PART_Root, this.BindingContext);
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Items != null ? this.Items! : Array.Empty<IVisualTreeElement>().ToList();
}
