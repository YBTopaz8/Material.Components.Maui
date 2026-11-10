using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Collections;
namespace Material.Components.Maui;

[ContentProperty(nameof(Items))]
public partial class NavigationBar
    : TemplatedView,
        IItemsElement<NavigationBarItem>,
        IICommandElement,IItemsSourceElement,
        IVisualTreeElement
{
    public static readonly BindableProperty ItemsProperty =
        IItemsElement<NavigationBarItem>.ItemsProperty;

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(NavigationBarItem),
        typeof(NavigationBar),
        propertyChanged: (bo, ov, nv) =>
        {
            var navBar = bo as NavigationBar;
            if (navBar is null) return;
            var item = (NavigationBarItem)nv;

            foreach (var navItem in navBar.Items)
            {
                if(navItem is null) continue;
                navItem.IsActived = item.Equals(navItem);
            }
            if (item != null && navBar.Items.Contains(item))
            {
                int newIndex = navBar.Items.IndexOf(item);
                if (navBar.SelectedIndex != newIndex)
                    navBar.SelectedIndex = newIndex;
            }

            navBar.SelectedItemChanged?.Invoke(navBar, new(item));

            if (navBar.Command?.CanExecute(navBar.CommandParameter ?? item) is true)
                navBar.Command?.Execute(navBar.CommandParameter ?? item);
        }
    );
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
    nameof(SelectedIndex), typeof(int), typeof(NavigationBar), -1, 
    propertyChanged: (bo, ov, nv) =>
    {
        var view = bo as NavigationBar; 
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

    public ObservableCollection<NavigationBarItem?> Items
    {
        get => (ObservableCollection<NavigationBarItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    void IItemsElement<NavigationBarItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
        {
            if (this.Items[e.NewStartingIndex]?.IsActived is true)
                this.SelectedItem = this.Items[e.NewStartingIndex];

            this.SelectedItem ??= this.Items[e.NewStartingIndex];
        }
    }

    public NavigationBarItem? SelectedItem
    {
        get => (NavigationBarItem?)this.GetValue(SelectedItemProperty);
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
    public static readonly BindableProperty ItemsSourceProperty = IItemsSourceElement.ItemsSourceProperty;
    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(NavigationBar), default(string)); // or typeof(NavigationBar)
    public static readonly BindableProperty IconMemberPathProperty = BindableProperty.Create(
        nameof(IconMemberPath), typeof(string), typeof(NavigationBar), default(string));

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


                var newItem = new NavigationBarItem
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


    public event EventHandler<SelectedItemChangedArgs<NavigationBarItem>>? SelectedItemChanged;

    private Grid? PART_Root;

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.PART_Root = (Grid)this.GetTemplateChild("PART_Root");

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
