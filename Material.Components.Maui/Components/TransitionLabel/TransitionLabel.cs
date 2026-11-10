using Microsoft.Maui.Animations;

namespace Material.Components.Maui;

public enum TextTransitionType
{
    Fade,
    SlideUp,
    SlideDown
}

public partial class TransitionLabel
    : TouchGraphicsView,
        ITextElement,
        IFontElement,
        IElement,
        IBackgroundElement,
        IShapeElement,
        IStateLayerElement,
        IRippleElement
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(TransitionLabel), string.Empty,
        propertyChanged: OnTextChanged);

    public static readonly BindableProperty FontColorProperty = IFontElement.FontColorProperty;
    public static readonly BindableProperty FontSizeProperty = IFontElement.FontSizeProperty;
    public static readonly BindableProperty FontFamilyProperty = IFontElement.FontFamilyProperty;
    public static readonly BindableProperty FontWeightProperty = IFontElement.FontWeightProperty;
    public static readonly BindableProperty FontIsItalicProperty = IFontElement.FontIsItalicProperty;

    public static readonly BindableProperty TransitionTypeProperty = BindableProperty.Create(
        nameof(TransitionType), typeof(TextTransitionType), typeof(TransitionLabel), TextTransitionType.SlideUp);

    public static readonly BindableProperty TransitionDurationProperty = BindableProperty.Create(
        nameof(TransitionDuration), typeof(uint), typeof(TransitionLabel), 300u);

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public Color FontColor { get => (Color)GetValue(FontColorProperty); set => SetValue(FontColorProperty, value); }
    public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public string FontFamily { get => (string)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
    public bool FontIsItalic { get => (bool)GetValue(FontIsItalicProperty); set => SetValue(FontIsItalicProperty, value); }
    public TextTransitionType TransitionType { get => (TextTransitionType)GetValue(TransitionTypeProperty); set => SetValue(TransitionTypeProperty, value); }
    public uint TransitionDuration { get => (uint)GetValue(TransitionDurationProperty); set => SetValue(TransitionDurationProperty, value); }

    internal string OldText { get; private set; } = string.Empty;
    internal string CurrentText { get; private set; } = string.Empty;
    internal float TransitionProgress { get; set; } = 1f;

    private IAnimationManager? _animManager;

    public TransitionLabel()
    {
        this.SetDynamicResource(StyleProperty, "StandardTransitionLabelStyle");
        this.Drawable = new TransitionLabelDrawable(this);
    }

    private static void OnTextChanged(BindableObject bo, object ov, object nv)
    {
        var view = (TransitionLabel)bo;
        string oldStr = (string)ov ?? string.Empty;
        string newStr = (string)nv ?? string.Empty;

        if (view.Handler == null || view.Width <= 0)
        {
            view.CurrentText = newStr;
            view.OldText = string.Empty;
            view.TransitionProgress = 1f;
            view.Invalidate();
            return;
        }

        view.OldText = oldStr;
        view.CurrentText = newStr;
        view.AnimateTransition();
    }

    private void AnimateTransition()
    {
        _animManager ??= this.Handler?.MauiContext?.Services.GetRequiredService<IAnimationManager>();

        this.TransitionProgress = 0f;
        var anim = new Microsoft.Maui.Animations.Animation(
            callback: p =>
            {
                this.TransitionProgress = (float)p;
                this.Invalidate();
            },
            duration: this.TransitionDuration / 1000f,
            easing: Easing.CubicOut
        );

        _animManager?.Add(anim);
    }
}