namespace Material.Components.Maui.Primitives;

public enum SliderFormatType
{
    Custom,     // Uses ValueBubbleFormat string or FormatValueBubble event
    Time,       // Automatic mm:ss or h:mm:ss based on seconds
    Percentage, // Formats as 0% - 100%
    Integer,    // Formats as whole number (e.g. 10)
    Decimal     // Formats with decimal precision (e.g. 10.5)
}
