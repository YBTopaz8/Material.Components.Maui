using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Material.Components.Maui;

[ContentProperty(nameof(Text))]
public partial class InlineText : Element
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(InlineText), string.Empty,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
        nameof(TextColor), typeof(Color), typeof(InlineText), null,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
        nameof(FontSize), typeof(float?), typeof(InlineText), null,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
        nameof(FontFamily), typeof(string), typeof(InlineText), null,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public static readonly BindableProperty FontAttributesProperty = BindableProperty.Create(
        nameof(FontAttributes), typeof(FontAttributes), typeof(InlineText), FontAttributes.None,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color? TextColor { get => (Color?)GetValue(TextColorProperty); set => SetValue(TextColorProperty, value); }
    public float? FontSize { get => (float?)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string? FontFamily { get => (string?)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public FontAttributes FontAttributes { get => (FontAttributes)GetValue(FontAttributesProperty); set => SetValue(FontAttributesProperty, value); }

    internal event EventHandler? Changed;
    internal void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}

// 1. Bold Inline
public partial class BoldInlineText : InlineText
{
    public BoldInlineText() => FontAttributes = FontAttributes.Bold;
}

// 2. Italic Inline
public partial class ItalicInlineText : InlineText
{
    public ItalicInlineText() => FontAttributes = FontAttributes.Italic;
}

// 3. Bold & Italic Inline
public partial class BoldItalicInlineText : InlineText
{
    public BoldItalicInlineText() => FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
}

// 4. Underlined Inline
public partial class UnderlinedInlineText : InlineText
{
    public static readonly BindableProperty UnderlineColorProperty = BindableProperty.Create(
        nameof(UnderlineColor), typeof(Color), typeof(UnderlinedInlineText), null,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public static readonly BindableProperty UnderlineThicknessProperty = BindableProperty.Create(
        nameof(UnderlineThickness), typeof(float), typeof(UnderlinedInlineText), 1f,
        propertyChanged: (b, o, n) => ((InlineText)b).NotifyChanged());

    public Color? UnderlineColor { get => (Color?)GetValue(UnderlineColorProperty); set => SetValue(UnderlineColorProperty, value); }
    public float UnderlineThickness { get => (float)GetValue(UnderlineThicknessProperty); set => SetValue(UnderlineThicknessProperty, value); }
}

// 5. Clickable Link Inline
public partial class LinkInlineText : UnderlinedInlineText
{
    public static readonly BindableProperty UrlProperty = BindableProperty.Create(
        nameof(Url), typeof(string), typeof(LinkInlineText), null);

    public static readonly BindableProperty ClickedColorProperty = BindableProperty.Create(
        nameof(ClickedColor), typeof(Color), typeof(LinkInlineText), null);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(LinkInlineText), null);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(LinkInlineText), null);

    public string? Url { get => (string?)GetValue(UrlProperty); set => SetValue(UrlProperty, value); }
    public Color? ClickedColor { get => (Color?)GetValue(ClickedColorProperty); set => SetValue(ClickedColorProperty, value); }
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }
    public object? CommandParameter { get => GetValue(CommandParameterProperty); set => SetValue(CommandParameterProperty, value); }

    public event EventHandler<string?>? LinkClicked;

    internal void TriggerClick()
    {
        LinkClicked?.Invoke(this, this.Url);

        var param = this.CommandParameter ?? this.Url;
        if (this.Command?.CanExecute(param) is true)
            this.Command.Execute(param);
        else if (Uri.TryCreate(this.Url, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }
}