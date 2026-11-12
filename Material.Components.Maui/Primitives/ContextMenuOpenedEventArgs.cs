namespace Material.Components.Maui.Primitives;

// Payload for Opened event
public class ContextMenuOpenedEventArgs : EventArgs
{
    public View Caller { get; }

    public ContextMenuOpenedEventArgs(View caller)
    {
        this.Caller = caller;
    }
}