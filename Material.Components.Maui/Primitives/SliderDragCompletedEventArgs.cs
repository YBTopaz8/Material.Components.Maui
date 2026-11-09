namespace Material.Components.Maui.Primitives;

public class SliderDragCompletedEventArgs : EventArgs
{
    public double Value { get; }
    public SliderDragCompletedEventArgs(double value) => Value = value;
}
