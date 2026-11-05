using Material.Components.Maui.Platform;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Handlers;

namespace Material.Components.Maui.Handlers;

public partial class TextFieldHandler : ViewHandler<TextField, PlatformTextField>
{
    protected override PlatformTextField CreatePlatformView()
    {
        return new(this.Context);
    }

    public static void MapDrawable(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.UpdateDrawable(virtualView.Drawable);
    }

    public static void MapInvalidate(TextFieldHandler handler, TextField virtualView, object? arg)
    {
        handler.PlatformView?.Invalidate();
    }

    protected override void ConnectHandler(PlatformTextField platformView)
    {
        platformView.Connect(this.VirtualView);
        platformView.TextChanged += this.OnTextChanged;
        platformView.SelectionChanged += this.OnSelectionChanged;

        var doubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
        doubleTapGesture.Tapped += OnDoubleTapped;
        this.VirtualView.GestureRecognizers.Add(doubleTapGesture);
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformTextField platformView)
    {
        platformView.Disconnect();
        platformView.TextChanged -= this.OnTextChanged;
        platformView.SelectionChanged -= this.OnSelectionChanged;
        var doubleTap = this.VirtualView.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault(g => g.NumberOfTapsRequired == 2);
        if (doubleTap != null)
        {
            doubleTap.Tapped -= OnDoubleTapped;
            this.VirtualView.GestureRecognizers.Remove(doubleTap);
        }

        base.DisconnectHandler(platformView);
    }
    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        //// Select All Text
        //if (!string.IsNullOrEmpty(this.VirtualView.Text))
        //{
        //    this.VirtualView.UpdateSelectionRange(new TextRange(0, this.VirtualView.Text.Length));
        //    // You might need to force a redraw here depending on your PlatformTextField logic
        //    this.PlatformView?.Invalidate();
        //}
    }
    private void OnTextChanged(object? sender, EditTextChangedEventArgs e)
    {
        var rawText = e.Text ?? string.Empty;

        if (this.VirtualView.InputType is InputType.Numeric)
        {
            // Keep digits only
            rawText = new string(rawText.Where(char.IsDigit).ToArray());
        }
        else if (this.VirtualView.InputType is InputType.DecimalNumber)
        {
            // Keep digits and at most one decimal separator
            // (Allows digits and . or ,)
            rawText = System.Text.RegularExpressions.Regex.Replace(rawText, @"[^0-9\.]", "");
            var parts = rawText.Split('.');
            if (parts.Length > 2)
                rawText = parts[0] + "." + string.Join("", parts.Skip(1));
        }

        this.VirtualView.Text = rawText;
        this.VirtualView.UpdateSelectionRange(e.SelectionRange);
    }

    private void OnSelectionChanged(object? sender, SelectionChangedArgs e)
    {
        this.VirtualView.UpdateSelectionRange(e.SelectionRange);
    }

    static partial void MapText(TextFieldHandler handler, TextField virtualView)
    {
        if (handler.PlatformView.Text != virtualView.Text)
            handler.PlatformView?.UpdateText(virtualView.Text);
    }

    static partial void MapSelectionRange(TextFieldHandler handler, TextField virtualView)
    {
        var pv = handler.PlatformView;
        var start = virtualView.SelectionRange.Start;
        var end = virtualView.SelectionRange.End;

        if (start != pv.SelectionStart || end != pv.SelectionEnd)
            pv?.UpdateSelection(start, end);
    }

    static partial void MapFontSize(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.SetTextSize(Android.Util.ComplexUnitType.Dip, virtualView.FontSize);
    }

    static partial void MapFontAttributes(TextFieldHandler handler, TextField virtualView)
    {
        var weight = (int)virtualView.FontWeight;
        var style = virtualView.FontIsItalic ? FontStyleType.Italic : FontStyleType.Normal;
        var font = new Microsoft.Maui.Graphics.Font(virtualView.FontFamily, weight, style);
        var typeface = font.ToTypeface();
        handler.PlatformView?.UpdateTypeface(typeface);
    }

    static partial void MapTextAlignment(TextFieldHandler handler, TextField virtualView)
    {
        var alignment =
            virtualView.TextAlignment == TextAlignment.End
                ? Android.Views.TextAlignment.TextEnd
                : virtualView.TextAlignment == TextAlignment.Center
                    ? Android.Views.TextAlignment.Center
                    : Android.Views.TextAlignment.TextStart;

        handler.PlatformView?.UpdateTextAlignment(alignment);
    }

    static partial void MapEditablePadding(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.UpdatePadding(virtualView.EditablePadding);
    }

    static partial void MapInputType(TextFieldHandler handler, TextField virtualView)
    {
        if (handler.PlatformView is PlatformTextField pv)
        {
            switch (virtualView.InputType)
            {
                case InputType.Numeric:
                    pv.InputType = Android.Text.InputTypes.ClassNumber;
                    break;
                case InputType.DecimalNumber:
                    pv.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
                    break;
                case InputType.Password:
                    pv.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
                    break;
                default:
                    pv.InputType = Android.Text.InputTypes.ClassText;
                    break;
            }
        }
        MapFontAttributes(handler, virtualView);
        MapFontSize(handler, virtualView);
    }

    static partial void MapIsReadOnly(TextFieldHandler handler, TextField virtualView)
    {
        handler.PlatformView?.UpdateIsReadOnly(virtualView.IsReadOnly);
    }
}
