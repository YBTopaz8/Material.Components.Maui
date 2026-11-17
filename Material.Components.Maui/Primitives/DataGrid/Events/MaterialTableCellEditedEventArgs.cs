using System;
using System.Collections.Generic;
using System.Text;

namespace Material.Components.Maui.Primitives;

public sealed class MaterialTableCellEditedEventArgs(
    MaterialTableColumn column,
    object? newValue) : EventArgs
{
    public MaterialTableColumn Column { get; } = column;
    public object? NewValue { get; } = newValue;
}
