using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Material.Components.Maui;

public partial class RadioButton
    : WrapLayout,
        IItemsElement<RadioItem>,
        IItemsSourceElement,
        IICommandElement,
        IVisualTreeElement
{
    public static readonly BindableProperty ItemsProperty = IItemsElement<RadioItem>.ItemsProperty;

    public static readonly BindableProperty ItemsSourceProperty =
        IItemsSourceElement.ItemsSourceProperty;

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(RadioButton),
        -1,
        propertyChanged: (bo, ov, nv) =>
        {
            var view = bo as RadioButton;
            if (view is null) return;
            var index = (int)nv;
            if (index >= 0 && index < view.Items.Count)
                view.SelectedItem = view.Items[index];

            for (var i = 0; i < view.Items.Count; i++)
                view.Items[i]?.IsSelected = i == index;

            if (view.Command?.CanExecute(view.CommandParameter ?? index) is true)
                view.Command?.Execute(view.CommandParameter ?? index);

            view.SelectedChanged?.Invoke(view, new(view.Items[index], index));
        }
    );

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(RadioItem),
        typeof(RadioButton),
        propertyChanged: (bo, ov, nv) =>
        {
            var view = bo as RadioButton;
            if (view is null) return;
            var item = (RadioItem)nv;
            if (view.Items.Contains(item))
                view.SelectedIndex = view.Items.IndexOf(item);

            foreach (var ri in view.Items)
            {
                if(ri is null) continue;
                ri.IsSelected = item != null && item.Equals(ri);
            }
        }
    );

    public static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public static readonly BindableProperty CommandParameterProperty =
        IICommandElement.CommandParameterProperty;

    public ObservableCollection<RadioItem?> Items
    {
        get => (ObservableCollection<RadioItem?>)this.GetValue(ItemsProperty);
        set => this.SetValue(ItemsProperty, value);
    }

    public IList ItemsSource
    {
        get => (IList)this.GetValue(ItemsSourceProperty);
        set => this.SetValue(ItemsSourceProperty, value);
    }

    private bool isSyncing = false;

    void IItemsElement<RadioItem>.OnItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (this.isSyncing) return;

        try
        {
            this.isSyncing = true;

            if (e.OldItems != null)
            {
                foreach (RadioItem item in e.OldItems)
                {
                    if (this.Children.Contains(item))
                        this.Children.Remove(item);
                }
            }

            if (e.NewItems != null)
            {
                var index = e.NewStartingIndex;
                foreach (RadioItem item in e.NewItems)
                {
                    if (!this.Children.Contains(item))
                    {
                        var safeIndex = Math.Clamp(index, 0, this.Children.Count);
                        this.Children.Insert(safeIndex, item);
                    }
                    index++;
                }
            }
        }
        finally
        {
            this.isSyncing = false;
        }
    }

    void IItemsSourceElement.OnItemsSourceCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.OldItems != null)
        {
            var index = e.OldStartingIndex;
            foreach (string item in e.OldItems)
            {
                this.Items.RemoveAt(index);
                index++;
            }
        }

        if (e.NewItems != null)
        {
            var index = e.NewStartingIndex;
            foreach (string item in e.NewItems)
            {
                this.Items.Insert(index, new RadioItem { Text = item });
                index++;
            }
        }
    }

    public int SelectedIndex
    {
        get => (int)this.GetValue(SelectedIndexProperty);
        set => this.SetValue(SelectedIndexProperty, value);
    }

    public RadioItem? SelectedItem
    {
        get => (RadioItem?)this.GetValue(SelectedItemProperty);
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

    public event EventHandler<SelectedItemChangedEventArgs>? SelectedChanged;

    public RadioButton()
    {
        this.ChildAdded += this.OnChildAdded;
        this.ChildRemoved += this.OnChildRemoved;
    }

    private void OnChildAdded(object? sender, ElementEventArgs e)
    {
        if (this.isSyncing) return;

        if (e.Element is RadioItem item && !this.Items.Contains(item))
        {
            try
            {
                this.isSyncing = true;
                this.Items.Add(item);
            }
            finally
            {
                this.isSyncing = false;
            }
        }
    }

    private void OnChildRemoved(object? sender, ElementEventArgs e)
    {
        if (this.isSyncing) return;

        if (e.Element is RadioItem item && this.Items.Contains(item))
        {
            try
            {
                this.isSyncing = true;
                this.Items.Remove(item);
            }
            finally
            {
                this.isSyncing = false;
            }
        }
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Items != null ? this.Items! : Array.Empty<IVisualTreeElement>().ToList();
}
