using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;

using System.Text;
using System.Windows.Input;

namespace Material.Components.Maui;

[ContentProperty(nameof(Items))]
public partial class ChipGroup
    : WrapLayout,
        IItemsElement<Chip>,
        IItemsSourceElement,
        IICommandElement,
        IVisualTreeElement
{
    // ==========================================
    // BINDABLE PROPERTIES
    // ==========================================
    public static readonly BindableProperty ItemsProperty = IItemsElement<Chip>.ItemsProperty;
    public static readonly BindableProperty ItemsSourceProperty = IItemsSourceElement.ItemsSourceProperty;

    public static readonly BindableProperty MultiSelectModeProperty = BindableProperty.Create(
        nameof(MultiSelectMode), typeof(bool), typeof(ChipGroup), false);

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(ChipGroup), -1,
        propertyChanged: OnSelectedIndexPropertyChanged);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(ChipGroup), null,
        propertyChanged: OnSelectedItemPropertyChanged);

    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems), typeof(IList), typeof(ChipGroup), null,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(ChipGroup), default(string));

    public static readonly BindableProperty CommandProperty = IICommandElement.CommandProperty;
    public static readonly BindableProperty CommandParameterProperty = IICommandElement.CommandParameterProperty;

    // ==========================================
    // ACCESSORS
    // ==========================================
    public bool MultiSelectMode { get => (bool)GetValue(MultiSelectModeProperty); set => SetValue(MultiSelectModeProperty, value); }
    public int SelectedIndex { get => (int)GetValue(SelectedIndexProperty); set => SetValue(SelectedIndexProperty, value); }
    public object? SelectedItem { get => GetValue(SelectedItemProperty); set => SetValue(SelectedItemProperty, value); }
    public IList? SelectedItems { get => (IList?)GetValue(SelectedItemsProperty); set => SetValue(SelectedItemsProperty, value); }
    public string DisplayMemberPath { get => (string)GetValue(DisplayMemberPathProperty); set => SetValue(DisplayMemberPathProperty, value); }
    public ObservableCollection<Chip?> Items { get => (ObservableCollection<Chip?>)GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
    public IList ItemsSource { get => (IList)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public ICommand Command { get => (ICommand)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    // ==========================================
    // EVENTS
    // ==========================================
    public event EventHandler<ValueChangedEventArgs<object>>? SelectedItemChanged;
    public event EventHandler<ValueChangedEventArgs<IEnumerable<object>>>? SelectedItemsChanged;

    private bool _isSyncing = false;

    public ChipGroup()
    {
        this.ChildAdded += OnChildAdded;
        this.ChildRemoved += OnChildRemoved;
    }

    // ==========================================
    // CHILD & SELECTION SYNCHRONIZATION
    // ==========================================
    void IItemsElement<Chip>.OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isSyncing) return;

        try
        {
            _isSyncing = true;

            if (e.OldItems != null)
            {
                foreach (Chip item in e.OldItems)
                {
                    item.SelectedChanged -= OnChipSelectedChanged;
                    if (this.Children.Contains(item))
                        this.Children.Remove(item);
                }
            }

            if (e.NewItems != null)
            {
                var index = e.NewStartingIndex;
                foreach (Chip item in e.NewItems)
                {
                    item.ChipType = ChipType.Filter; // Force selection capability inside group
                    item.SelectedChanged += OnChipSelectedChanged;

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
            _isSyncing = false;
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

                var chip = new Chip
                {
                    Text = displayText,
                    ChipType = ChipType.Filter,
                    BindingContext = item
                };

                this.Items.Insert(index, chip);
                index++;
            }
        }
    }

    private void OnChipSelectedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (_isSyncing) return;
        var clickedChip = sender as Chip;
        if (clickedChip == null) return;

        if (this.MultiSelectMode)
        {
            var model = clickedChip.BindingContext ?? clickedChip;

            // Maintain SelectedItems bound collection
            if (this.SelectedItems != null)
            {
                if (clickedChip.IsSelected && !this.SelectedItems.Contains(model))
                    this.SelectedItems.Add(model);
                else if (!clickedChip.IsSelected && this.SelectedItems.Contains(model))
                    this.SelectedItems.Remove(model);
            }

            var currentSelection = this.Items
                .Where(i => i?.IsSelected == true)
                .Select(i => i!.BindingContext ?? i)
                .ToList();

            SelectedItemsChanged?.Invoke(this, new ValueChangedEventArgs<IEnumerable<object>>(null, currentSelection));

            if (this.Command?.CanExecute(this.CommandParameter ?? currentSelection) is true)
                this.Command?.Execute(this.CommandParameter ?? currentSelection);
        }
        else
        {
            // Radio Mode: If checked, uncheck all others
            if (clickedChip.IsSelected)
            {
                int index = this.Items.IndexOf(clickedChip);
                if (this.SelectedIndex != index)
                {
                    this.SelectedIndex = index;
                }
            }
            else
            {
                // If user unclicked the active chip, reset selection
                if (this.Items.IndexOf(clickedChip) == this.SelectedIndex)
                {
                    this.SelectedIndex = -1;
                }
            }
        }
    }

    private static void OnSelectedIndexPropertyChanged(BindableObject bo, object ov, object nv)
    {
        var group = (ChipGroup)bo;
        if (group.MultiSelectMode || group._isSyncing) return;

        int index = (int)nv;
        object? oldVal = group.SelectedItem;
        object? newVal = null;

        try
        {
            group._isSyncing = true;

            for (int i = 0; i < group.Items.Count; i++)
            {
                var chip = group.Items[i];
                if (chip != null)
                {
                    bool shouldSelect = (i == index);
                    chip.IsSelected = shouldSelect;
                    if (shouldSelect)
                        newVal = chip.BindingContext ?? chip;
                }
            }
        }
        finally
        {
            group._isSyncing = false;
        }

        group.SelectedItem = newVal;
        group.SelectedItemChanged?.Invoke(group, new ValueChangedEventArgs<object>(oldVal, newVal));

        if (group.Command?.CanExecute(group.CommandParameter ?? newVal) is true)
            group.Command?.Execute(group.CommandParameter ?? newVal);
    }

    private static void OnSelectedItemPropertyChanged(BindableObject bo, object ov, object nv)
    {
        var group = (ChipGroup)bo;
        if (group.MultiSelectMode) return;

        var match = group.Items.FirstOrDefault(c => c == nv || (c?.BindingContext != null && c.BindingContext.Equals(nv)));
        if (match != null)
            group.SelectedIndex = group.Items.IndexOf(match);
        else if (nv == null)
            group.SelectedIndex = -1;
    }

    private void OnChildAdded(object? sender, ElementEventArgs e)
    {
        if (_isSyncing) return;
        if (e.Element is Chip item && !this.Items.Contains(item))
        {
            try
            {
                _isSyncing = true;
                item.ChipType = ChipType.Filter;
                item.SelectedChanged += OnChipSelectedChanged;
                this.Items.Add(item);
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    private void OnChildRemoved(object? sender, ElementEventArgs e)
    {
        if (_isSyncing) return;
        if (e.Element is Chip item && this.Items.Contains(item))
        {
            try
            {
                _isSyncing = true;
                item.SelectedChanged -= OnChipSelectedChanged;
                this.Items.Remove(item);
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }

    public IReadOnlyList<IVisualTreeElement> GetVisualChildren() =>
        this.Items != null ? this.Items.Where(i => i != null).ToList()! : Array.Empty<IVisualTreeElement>().ToList();
}