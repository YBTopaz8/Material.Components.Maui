namespace Material.Components.Maui.Primitives;

public class ValueChangedEventArgs<T> : EventArgs
{
    public T? OldValue { get; }
    public T? NewValue { get; }

    public ValueChangedEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
public class ValueChangedEventArgs : EventArgs
{
    public double OldValue { get; }
    public double NewValue { get; }

    public ValueChangedEventArgs(double oldValue, double newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}