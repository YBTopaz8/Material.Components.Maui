namespace Material.Components.Maui.Primitives;

public class GooeyItemClickedEventArgs : EventArgs
{
    public object? Item { get; }
    public View View { get; }

    public GooeyItemClickedEventArgs(object? item, View view)
    {
        this.Item = item;
        this.View = view;
    }
}