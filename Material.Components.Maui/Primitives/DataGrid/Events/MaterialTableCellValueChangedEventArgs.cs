namespace Material.Components.Maui.Primitives;

public sealed class MaterialTableCellValueChangedEventArgs(
    object rowItem,
    MaterialTableColumn? column,
    string bindingPath,
    object? oldValue,
    object? newValue,
    bool isNewRow) : EventArgs
{
    public object RowItem { get; } = rowItem;
    public MaterialTableColumn? Column { get; } = column;
    public string BindingPath { get; } = bindingPath;
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
    public bool IsNewRow { get; } = isNewRow;
}
