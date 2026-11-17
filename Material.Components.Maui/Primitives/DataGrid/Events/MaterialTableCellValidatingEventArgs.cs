
namespace Material.Components.Maui.Primitives;

public sealed class MaterialTableCellValidatingEventArgs(
    MaterialTableColumn column,
    string rawText,
    object? parsedValue) : EventArgs
{
    public MaterialTableColumn Column { get; } = column;
    public string RawText { get; } = rawText;
    public object? ParsedValue { get; set; } = parsedValue;
    public bool Cancel { get; set; }
}
