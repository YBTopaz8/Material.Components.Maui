namespace Material.Components.Maui.Primitives;

public class SliderFormatLabelEventArgs : EventArgs
{
    public double Value { get; }
    public string FormattedLabel { get; set; }

    public SliderFormatLabelEventArgs(double value, string defaultFormat)
    {
        Value = value;
        FormattedLabel = defaultFormat;
    }
}