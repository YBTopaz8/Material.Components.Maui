using Material.Components.Maui.Platform;
using Material.Components.Maui.Platform.Editable;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Material.Components.Maui.Handlers;

public partial class TextFieldHandler : ViewHandler<TextField, PlatformTextField>
{
    EditableHandler? editableHandler;

    protected override PlatformTextField CreatePlatformView()
    {
        this.editableHandler = new EditableHandler(this.VirtualView);
        return new PlatformTextField(this.editableHandler);
    }

    protected override void ConnectHandler(PlatformTextField platformView)
    {
        platformView.Connect(this.VirtualView);
        base.ConnectHandler(platformView);
        platformView.CharacterReceived += OnCharacterReceived;
    }

    private void OnCharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
    {
        if (this.VirtualView.InputType is InputType.Numeric)
        {
            // Block everything that is not a digit (allow backspace/control chars: \b, \r)
            if (!char.IsDigit((char)args.Character) && args.Character != '\b' && args.Character != '\r')
            {
                args.Handled = true; // Swallows the keystroke!
            }
        }
        else if (this.VirtualView.InputType is InputType.DecimalNumber)
        {
            // Allow digits, control chars, and decimal separator (. or ,)
            char ch = (char)args.Character;
            bool isDecimalChar = ch == '.' || ch == ',';
            if (!char.IsDigit(ch) && !isDecimalChar && ch != '\b' && ch != '\r')
            {
                args.Handled = true;
            }
        }
    }

    protected override void DisconnectHandler(PlatformTextField platformView)
    {
        platformView.CharacterReceived -= OnCharacterReceived;
        platformView.Disconnect();
        base.DisconnectHandler(platformView);
    }

    public static void MapDrawable(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.UpdateDrawable(virtualView);
    }

    public static void MapInvalidate(TextFieldHandler handler, TextField virtualView, object? arg)
    {
        try
        {
            if (handler.PlatformView is null)
            {
                return;
            }

            handler.PlatformView.Invalidate();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    static partial void MapText(TextFieldHandler handler, TextField virtualView)
    {
        handler.editableHandler?.UpdateText(virtualView.Text);
    }

    static partial void MapFontAttributes(TextFieldHandler handler, TextField virtualView) { }

    static partial void MapSelectionRange(TextFieldHandler handler, TextField virtualView)
    {
        if (!virtualView.SelectionRange.Equals(handler.editableHandler?.SelectionRange))
            handler.editableHandler?.SelectionRange = virtualView.SelectionRange;
    }

    static partial void MapFontSize(TextFieldHandler handler, TextField virtualView) { }

    static partial void MapTextAlignment(TextFieldHandler handler, TextField virtualView) { }

    static partial void MapEditablePadding(TextFieldHandler handler, TextField virtualView) { }

    static partial void MapInputType(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.UpdateInputType(virtualView.InputType);
    }

    static partial void MapIsReadOnly(TextFieldHandler handler, TextField virtualView) { }
}
