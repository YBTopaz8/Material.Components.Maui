namespace Material.Components.Maui.Primitives;

public sealed class MaterialTableRowEventArgs(object rowItem) : EventArgs
{
    public object RowItem { get; } = rowItem;
}