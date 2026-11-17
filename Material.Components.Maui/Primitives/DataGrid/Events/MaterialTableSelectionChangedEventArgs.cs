namespace Material.Components.Maui.Primitives;

public sealed class MaterialTableSelectionChangedEventArgs(
    IReadOnlyList<object> addedItems,
    IReadOnlyList<object> removedItems) : EventArgs
{
    public IReadOnlyList<object> AddedItems { get; } = addedItems;
    public IReadOnlyList<object> RemovedItems { get; } = removedItems;
}
