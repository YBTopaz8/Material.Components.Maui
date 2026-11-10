using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Match = System.Text.RegularExpressions.Match;

namespace Material.Components.Maui;

internal class RichLabelDrawable(RichLabel view) : IDrawable
{
    private static readonly Regex TokenRegex = new(
        @"(\*\*(.*?)\*\*)|(__(.*?)__)|(~~(.*?)~~)|(\*(.*?)\*)|(?:\[(.*?)\]\((.*?)\))",
        RegexOptions.Compiled);

    public void Draw(ICanvas canvas, RectF rect)
    {
        canvas.SaveState();
        canvas.Antialias = true;

        canvas.ClipPath(view.GetClipPath(rect));
        canvas.DrawBackground(view, rect);

        if (view.RipplePercent is not 0f and not 1f)
            canvas.DrawRipple(view, view.LastTouchPoint, view.RippleSize, view.RipplePercent);
        else
            canvas.DrawStateLayer(view, rect, view.ViewState);

        // Perform Layout & Render
        CalculateLayout(rect, recordBounds: true, canvas: canvas);

        canvas.RestoreState();
    }

    public float CalculateLayout(RectF bounds, bool recordBounds, ICanvas? canvas = null)
    {
        if (recordBounds)
            view.FormattedRuns.Clear();

        // 1. Resolve source runs (Either from Inlines or from Markdown Text)
        var sourceRuns = ResolveRuns();
        if (sourceRuns.Count == 0) return 0f;

        float defaultFontSize = view.FontSize > 0 ? view.FontSize : 14f;
        float currentX = bounds.Left - (view.EnableMarquee ? view.MarqueeOffset : 0f);
        float currentY = bounds.Top;
        float lineHeight = defaultFontSize + view.LineSpacing;
        float spaceWidth = 5f;
        float maxX = currentX;

        foreach (var run in sourceRuns)
        {
            float fontSize = run.FontSize > 0 ? run.FontSize : defaultFontSize;
            var font = ResolveFont(run);
            lineHeight = Math.Max(lineHeight, fontSize + view.LineSpacing);

            // Split into words for word-wrapping
            var words = run.Text.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (string.IsNullOrEmpty(word)) continue;

                var wordSize = MeasureWord(word, font, fontSize);

                // Word-Wrap (Only when marquee is disabled!)
                if (!view.EnableMarquee && currentX + wordSize.Width > bounds.Right && currentX > bounds.Left)
                {
                    currentX = bounds.Left;
                    currentY += lineHeight;
                }

                var wordRect = new RectF(currentX, currentY + (lineHeight - wordSize.Height) / 2f, wordSize.Width, wordSize.Height);

                if (recordBounds)
                {
                    var runInstance = new RichTextRun
                    {
                        Text = word,
                        Bounds = wordRect,
                        IsBold = run.IsBold,
                        IsItalic = run.IsItalic,
                        IsUnderlined = run.IsUnderlined,
                        IsStrikethrough = run.IsStrikethrough,
                        IsLink = run.IsLink,
                        LinkUrl = run.LinkUrl,
                        LinkSource = run.LinkSource,
                        Color = run.Color
                    };
                    view.FormattedRuns.Add(runInstance);
                }

                // Render to Canvas
                if (canvas != null)
                {
                    canvas.Font = font;
                    canvas.FontSize = fontSize;
                    canvas.FontColor = run.Color.WithAlpha(view.ViewState is ViewState.Disabled ? 0.38f : 1f);

                    canvas.DrawString(word, wordRect, HorizontalAlignment.Left, VerticalAlignment.Center);

                    // Underline
                    if (run.IsUnderlined || run.IsLink)
                    {
                        canvas.StrokeColor = run.IsLink ? view.LinkColor : run.Color;
                        canvas.StrokeSize = 1f;
                        float lineY = wordRect.Bottom - 1f;
                        canvas.DrawLine(wordRect.Left, lineY, wordRect.Right, lineY);
                    }

                    // Strikethrough
                    if (run.IsStrikethrough)
                    {
                        canvas.StrokeColor = run.Color;
                        canvas.StrokeSize = 1f;
                        canvas.DrawLine(wordRect.Left, wordRect.Center.Y, wordRect.Right, wordRect.Center.Y);
                    }
                }

                currentX += wordSize.Width + spaceWidth;
                maxX = Math.Max(maxX, currentX);
            }
        }

        view.TotalMeasuredWidth = maxX - bounds.Left;
        return (currentY - bounds.Top) + lineHeight;
    }

    private List<RichTextRun> ResolveRuns()
    {
        var runs = new List<RichTextRun>();

        // Priority 1: XAML Inlines
        if (view.Inlines.Count > 0)
        {
            foreach (var inline in view.Inlines)
            {
                if (string.IsNullOrEmpty(inline.Text)) continue;

                runs.Add(new RichTextRun
                {
                    Text = inline.Text,
                    IsBold = inline.FontAttributes.HasFlag(FontAttributes.Bold),
                    IsItalic = inline.FontAttributes.HasFlag(FontAttributes.Italic),
                    IsUnderlined = inline is UnderlinedInlineText,
                    IsLink = inline is LinkInlineText,
                    LinkUrl = (inline as LinkInlineText)?.Url ?? string.Empty,
                    LinkSource = inline as LinkInlineText,
                    Color = inline.TextColor ?? view.FontColor ?? Colors.Black,
                    FontSize = inline.FontSize ?? view.FontSize,
                    FontFamily = inline.FontFamily ?? view.FontFamily
                });
            }
            return runs;
        }

        // Priority 2: Markdown Text
        if (!string.IsNullOrEmpty(view.Text))
        {
            int lastIndex = 0;
            foreach (Match match in TokenRegex.Matches(view.Text))
            {
                if (match.Index > lastIndex)
                {
                    runs.Add(new RichTextRun { Text = view.Text.Substring(lastIndex, match.Index - lastIndex), Color = view.FontColor ?? Colors.Black });
                }

                if (match.Groups[1].Success)      // **bold**
                    runs.Add(new RichTextRun { Text = match.Groups[2].Value, IsBold = true, Color = view.FontColor ?? Colors.Black });
                else if (match.Groups[3].Success) // __underline__
                    runs.Add(new RichTextRun { Text = match.Groups[4].Value, IsUnderlined = true, Color = view.FontColor ?? Colors.Black });
                else if (match.Groups[5].Success) // ~~strikethrough~~
                    runs.Add(new RichTextRun { Text = match.Groups[6].Value, IsStrikethrough = true, Color = view.FontColor ?? Colors.Black });
                else if (match.Groups[7].Success) // *italic*
                    runs.Add(new RichTextRun { Text = match.Groups[8].Value, IsItalic = true, Color = view.FontColor ?? Colors.Black });
                else if (match.Groups[9].Success) // [link](url)
                    runs.Add(new RichTextRun { Text = match.Groups[9].Value, IsLink = true, LinkUrl = match.Groups[10].Value, Color = view.LinkColor });

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < view.Text.Length)
                runs.Add(new RichTextRun { Text = view.Text.Substring(lastIndex), Color = view.FontColor ?? Colors.Black });
        }

        return runs;
    }

    private Microsoft.Maui.Graphics.Font ResolveFont(RichTextRun run)
    {
        var family = run.FontFamily ?? view.FontFamily;
        int weight = run.IsBold ? 700 : (int)view.FontWeight;
        bool isItalic = run.IsItalic || view.FontIsItalic;

        return new Microsoft.Maui.Graphics.Font(family, weight, isItalic ? FontStyleType.Italic : FontStyleType.Normal);
    }

    private SizeF MeasureWord(string text, Microsoft.Maui.Graphics.Font font, float fontSize)
    {
        // Safe character-width approximation: fixes ICanvas.Font getter error completely!
        return new SizeF(text.Length * (fontSize * 0.55f), fontSize + 2f);
    }
}