using System.Text;

#if WINDOWS
using Microsoft.UI;
using WinRT.Interop;
#endif

namespace Material.Components.Maui.Platform.Editable;

public class EditableHandler(TextField editor)
{
    public TextField editor = editor;

    TextRange selectionRange = new();
    public TextRange SelectionRange
    {
        get => this.selectionRange;
        set
        {
            if (!this.SelectionRange.Equals(value))
            {
                this.selectionRange = value;
                this.editor.UpdateSelectionRange(value);
                this.SelectionChanged?.Invoke(this, new(value));
            }
        }
    }
    public int TextLength => this.TextBuilder.Length;

    public event EventHandler<SelectionChangedArgs>? SelectionChanged;

    public StringBuilder TextBuilder { get; private set; } = new();

    readonly UndoRedoHelper undoRedoHelper = new();

    public void UpdateText(string? text)
    {
        if (text == null || text.CompareTo(this.ToString()) == 0)
            return;

        var range = this.SelectionRange.Normalized();
        this.TextBuilder.Clear();
        this.TextBuilder.Append(text);
        this.editor.Text = text;
        this.SelectionRange = new TextRange(
            Math.Min(range.Start, text.Length),
            Math.Min(range.End, text.Length)
        );
    }

    public void ReplaceText(string text)
    {
        if (!this.editor.IsReadOnly)
        {
            var range = this.SelectionRange.Normalized();
            this.ReplaceText(text, range);
        }
    }

    public void ReplaceText(string text, TextRange range)
    {
        if (this.editor.InputType is InputType.Numeric)
        {
            text = new string(text.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(text))
                return; // Drop invalid text completely!
        }
        else if (this.editor.InputType is InputType.DecimalNumber)
        {
            var current = this.TextBuilder.ToString();
            var alreadyHasDecimal = current.Contains('.') || current.Contains(',');

            var sb = new StringBuilder();
            foreach (var c in text)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
                else if ((c == '.' || c == ',') && !alreadyHasDecimal)
                {
                    sb.Append(c);
                    alreadyHasDecimal = true;
                }
            }
            text = sb.ToString();
            if (string.IsNullOrEmpty(text))
                return;
        }
        // ==========================================

        if (range.IsRange)
        {
            var replaceText = this.ToString(range.Start, range.Length);
            this.TextBuilder.Replace(replaceText, text, range.Start, range.Length);
        }
        else
            this.TextBuilder.Insert(range.End, text);

        var location = Math.Min(range.Start + text.Length, this.TextLength);
        var newText = this.TextBuilder.ToString();
        this.undoRedoHelper.Add(newText, location);
        this.editor.Text = newText;
        this.SelectionRange = new TextRange(location);
    }

    public void DeleteText()
    {
        var range = this.SelectionRange.Normalized();
        this.DeleteText(range);
    }

    public void DeleteText(TextRange range)
    {
        if (this.editor.IsReadOnly) return;

        var location = 0;
        if (range.IsRange)
        {
            var replaceText = this.TextBuilder.ToString(range.Start, range.Length);
            this.TextBuilder.Replace(replaceText, string.Empty, range.Start, range.Length);
            location = Math.Max(0, range.Start);
        }
        else if (this.TextLength > 0 && range.End > 0)
        {
            this.TextBuilder.Remove(range.End - 1, 1);
            location = Math.Max(0, range.End - 1);
        }

        var newText = this.TextBuilder.ToString();
        this.undoRedoHelper.Add(newText, location);
        this.editor.Text = newText;
        this.SelectionRange = new TextRange(location);
    }

    public void CommitText(string text)
    {
        if (!string.IsNullOrEmpty(text))
            this.ReplaceText(text);
    }

    public void Clear()
    {
        this.TextBuilder.Clear();
        this.SelectionRange = new(0);
    }

    public void SelectAll() => this.SelectionRange = new TextRange(0, this.TextBuilder.Length);

    public void CopyRangeTextToClipboard()
    {
        if (this.editor.InputType is InputType.Password) return;

        var range = this.SelectionRange.Normalized();
        if (range.IsRange)
        {
            var text = this.TextBuilder.ToString(range.Start, range.Length);
            _ = Clipboard.SetTextAsync(text);
        }
    }

    public async void CopyRangeTextFromClipboard()
    {
        try
        {
            var text = await Clipboard.Default.GetTextAsync();
            if (!string.IsNullOrEmpty(text))
                this.ReplaceText(text);
        }
        catch
        {
            // Clipboard access failed or empty
        }
    }

    public void CutRangeTextToClipboard()
    {
        if (this.editor.InputType is InputType.Password) return;

        var range = this.SelectionRange.Normalized();
        if (range.IsRange)
        {
            var text = this.TextBuilder.ToString(range.Start, range.Length);
            _ = Clipboard.Default.SetTextAsync(text);
            this.DeleteText();
        }
    }

    public void Undo()
    {
        var cache = this.undoRedoHelper.GetUndoCache();
        if (cache != null)
        {
            this.TextBuilder.Clear();
            this.TextBuilder.Append(cache.Value.Text);
            var newText = this.TextBuilder.ToString();
            this.editor.Text = newText;
            this.SelectionRange = cache.Value.SelectionRange;
        }
    }

    public void Redo()
    {
        var cache = this.undoRedoHelper.GetRedoCache();
        if (cache != null)
        {
            this.TextBuilder.Clear();
            this.TextBuilder.Append(cache.Value.Text);
            var newText = this.TextBuilder.ToString();
            this.editor.Text = newText;
            this.SelectionRange = cache.Value.SelectionRange;
        }
    }

    public override string ToString()
    {
        return this.TextBuilder.Length > 0 ? this.TextBuilder.ToString() : string.Empty;
    }

    public string ToString(int start, int length)
    {
        start = Math.Max(start, 0);
        length = Math.Clamp(length, 0, this.TextBuilder.Length - start);

        return length > 0 ? this.TextBuilder.ToString(start, length) : string.Empty;
    }

    internal CaretInfo GetCaretInfo()
    {
        var result = this.editor.GetCaretInfo(
            (float)this.editor.Bounds.Width,
            this.selectionRange.Start
        );
        result.X += (float)this.editor.EditablePadding.Left;
        result.Y += (float)this.editor.EditablePadding.Top;

        return result;
    }

#if WINDOWS
    internal void Navigate(NavigationKind kind)
    {
        switch (kind)
        {
            case NavigationKind.Left:
                this.SelectionRange = new(Math.Max(0, this.SelectionRange.End - 1));
                break;
            case NavigationKind.Right:
                this.SelectionRange = new(Math.Min(this.SelectionRange.End + 1, this.TextLength));
                break;
            case NavigationKind.Up:
                {
                    var caretInfo = this.editor.NavigateUp((float)this.editor.Bounds.Width);
                    this.SelectionRange = new TextRange(caretInfo.Position);
                    break;
                }
            case NavigationKind.Down:
                {
                    var caretInfo = this.editor.NavigateDown((float)this.editor.Bounds.Width);
                    this.SelectionRange = new TextRange(caretInfo.Position);
                    break;
                }
            case NavigationKind.SelectLeft:
                this.SelectionRange = new TextRange(
                    this.SelectionRange.Start,
                    Math.Max(0, this.SelectionRange.End - 1)
                );
                break;
            case NavigationKind.SelectRight:
                this.SelectionRange = new TextRange(
                    this.SelectionRange.Start,
                    Math.Min(this.SelectionRange.End + 1, this.TextLength)
                );
                break;
            case NavigationKind.SelectUp:
                {
                    var caretInfo = this.editor.NavigateUp((float)this.editor.Bounds.Width);
                    this.SelectionRange = new TextRange(this.SelectionRange.Start, caretInfo.Position);
                    break;
                }
            case NavigationKind.SelectDown:
                {
                    var caretInfo = this.editor.NavigateDown((float)this.editor.Bounds.Width);
                    this.SelectionRange = new TextRange(this.SelectionRange.Start, caretInfo.Position);
                    break;
                }
            //TODO
            case NavigationKind.WordLeft:
                this.SelectionRange = new(this.GetPreviousWordIndex(this.SelectionRange.Start));
                break;
            case NavigationKind.WordRight:
                this.SelectionRange = new(this.GetNextWordIndex(this.SelectionRange.Start));
                break;
            case NavigationKind.SelectWordleft:
                this.SelectionRange = new TextRange(
                    this.SelectionRange.Start,
                    this.GetPreviousWordIndex(this.SelectionRange.End)
                );
                break;
            case NavigationKind.SelectWordRight:
                this.SelectionRange = new TextRange(
                    this.SelectionRange.Start,
                    this.GetNextWordIndex(this.SelectionRange.End)
                );
                break;
            case NavigationKind.Home:
                this.SelectionRange = new(0);
                break;
            case NavigationKind.End:
                this.SelectionRange = new(this.TextLength);
                break;
            case NavigationKind.SelectHome:
                this.SelectionRange = new TextRange(this.SelectionRange.Start, 0);
                break;
            case NavigationKind.SelectEnd:
                this.SelectionRange = new TextRange(this.SelectionRange.Start, this.TextLength);
                break;
            default:
                break;
        }
    }

    internal WindowId? GetWindowId()
    {
        var hwnd = WindowNative.GetWindowHandle(this.editor.Window.Handler.PlatformView);

        return hwnd != IntPtr.Zero ? Win32Interop.GetWindowIdFromWindow(hwnd) : null;
    }

    internal Thickness GetEditablePadding() => this.editor.EditablePadding;
#endif


    private int GetPreviousWordIndex(int startIndex)
    {
        if (startIndex <= 0) return 0;
        var str = this.TextBuilder.ToString();
        int i = startIndex - 1;

        // Skip trailing spaces
        while (i > 0 && char.IsWhiteSpace(str[i]))
            i--;

        // Move backwards across word characters
        while (i > 0 && !char.IsWhiteSpace(str[i - 1]))
            i--;

        return Math.Max(0, i);
    }

    private int GetNextWordIndex(int startIndex)
    {
        var str = this.TextBuilder.ToString();
        int len = str.Length;
        if (startIndex >= len) return len;
        int i = startIndex;

        // Move forward across word characters
        while (i < len && !char.IsWhiteSpace(str[i]))
            i++;

        // Skip trailing whitespace
        while (i < len && char.IsWhiteSpace(str[i]))
            i++;

        return Math.Min(len, i);
    }

    // Shortcut: Ctrl + Backspace (Delete previous word)
    public void DeleteWordLeft()
    {
        if (this.editor.IsReadOnly) return;
        var range = this.SelectionRange.Normalized();
        if (range.IsRange)
        {
            this.DeleteText(range);
            return;
        }

        int prevWord = this.GetPreviousWordIndex(range.Start);
        if (prevWord < range.Start)
        {
            this.DeleteText(new TextRange(prevWord, range.Start));
        }
    }

    // Shortcut: Delete key (Delete next character forward)
    public void DeleteForward()
    {
        if (this.editor.IsReadOnly) return;
        var range = this.SelectionRange.Normalized();
        if (range.IsRange)
        {
            this.DeleteText(range);
            return;
        }

        if (range.Start < this.TextLength)
        {
            this.TextBuilder.Remove(range.Start, 1);
            var newText = this.TextBuilder.ToString();
            this.undoRedoHelper.Add(newText, range.Start);
            this.editor.Text = newText;
            this.SelectionRange = new TextRange(range.Start);
        }
    }

    // Shortcut: Ctrl + Delete (Delete next word forward)
    public void DeleteWordRight()
    {
        if (this.editor.IsReadOnly) return;
        var range = this.SelectionRange.Normalized();
        if (range.IsRange)
        {
            this.DeleteText(range);
            return;
        }

        int nextWord = this.GetNextWordIndex(range.Start);
        if (nextWord > range.Start)
        {
            this.DeleteText(new TextRange(range.Start, nextWord));
        }
    }
    public void SelectWordAt(int position)
    {
        if (this.TextLength == 0) return;

        var text = this.TextBuilder.ToString();
        position = Math.Clamp(position, 0, text.Length);

        // If clicked on whitespace or end of string, step back 1 char
        if (position > 0 && (position == text.Length || char.IsWhiteSpace(text[position])))
        {
            if (!char.IsWhiteSpace(text[position - 1]))
                position--;
        }

        int start = position;
        int end = position;

        // Expand Left until we hit a space or punctuation
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]) && !char.IsPunctuation(text[start - 1]))
        {
            start--;
        }

        // Expand Right until we hit a space or punctuation
        while (end < text.Length && !char.IsWhiteSpace(text[end]) && !char.IsPunctuation(text[end]))
        {
            end++;
        }

        // If clicked directly on a punctuation symbol (e.g. "@" or "."), select just that symbol
        if (start == end && position < text.Length)
        {
            end = Math.Min(text.Length, position + 1);
        }

        // Apply the selection
        this.SelectionRange = new TextRange(start, end);
    }
}
