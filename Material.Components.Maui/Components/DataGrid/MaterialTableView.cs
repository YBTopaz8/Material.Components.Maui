using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui.Components;

public class MaterialTableView : View
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(MaterialTableView));

    public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns), typeof(IList<TableColumn>), typeof(MaterialTableView),
            defaultValueCreator: _ => new List<TableColumn>());

    public static readonly BindableProperty AutoGenerateColumnsProperty =
        BindableProperty.Create(nameof(AutoGenerateColumns), typeof(bool), typeof(MaterialTableView), true);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(MaterialTableView),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(nameof(IsReadOnly), typeof(bool), typeof(MaterialTableView), false);

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IList<TableColumn> Columns
    {
        get => (IList<TableColumn>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public bool AutoGenerateColumns
    {
        get => (bool)GetValue(AutoGenerateColumnsProperty);
        set => SetValue(AutoGenerateColumnsProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}

public class TableColumn
{
    public string Header { get; set; } = string.Empty;
    public string BindingPath { get; set; } = string.Empty;
    public double Width { get; set; } = double.NaN;
    public bool IsReadOnly { get; set; }
}