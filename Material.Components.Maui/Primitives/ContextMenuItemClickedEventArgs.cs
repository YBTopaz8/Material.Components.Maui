namespace Material.Components.Maui.Primitives;

public class ContextMenuItemClickedEventArgs : EventArgs
{
    public object? Item { get; }
    public int ItemIndex { get; } = -1;
    public View? Caller { get; }

    public ContextMenuItemClickedEventArgs(object? item, View? caller, int itemIndex)
    {
        this.Item = item;
        this.Caller = caller;
        this.ItemIndex = itemIndex;
    }
}
