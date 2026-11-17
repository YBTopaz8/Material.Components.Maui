#if WINDOWS
using Microsoft.UI.Xaml;
using System.Globalization;
using System.Text;
using WinIValueConverter = Microsoft.UI.Xaml.Data.IValueConverter;
using WinBorder = Microsoft.UI.Xaml.Controls.Border;
namespace Material.Components.Maui.Platform;

/// <summary>
/// Two-way converter providing DisplayFormat (currency, numeric, dates) and InputMask
/// on WinUI.TableView columns with inline editing parse-back and validation.
/// </summary>
internal  sealed partial class MaterialTableColumnValueConverter(
    MaterialTableView table,
    MaterialTableColumn column) : WinIValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is null)
            return column.NullDisplayText ?? string.Empty;

        if (!string.IsNullOrEmpty(column.InputMask) && value is string rawText)
            return ApplyMask(rawText, column.InputMask);

        if (!string.IsNullOrEmpty(column.DisplayFormat))
        {
            if (column.DisplayFormat.Contains("{0", StringComparison.Ordinal))
                return string.Format(CultureInfo.CurrentCulture, column.DisplayFormat, value);

            if (value is IFormattable formattable)
                return formattable.ToString(column.DisplayFormat, CultureInfo.CurrentCulture);
        }

        return value.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        string text = value?.ToString()?.Trim() ?? string.Empty;
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        bool isNullable = Nullable.GetUnderlyingType(targetType) is not null || !targetType.IsValueType;

        if (string.IsNullOrEmpty(text))
        {
            object? emptyVal = underlying == typeof(string) ? string.Empty : (isNullable ? null : DependencyProperty.UnsetValue);
            if (emptyVal == DependencyProperty.UnsetValue)
                return DependencyProperty.UnsetValue;

            var emptyCheck = table.RaiseCellValidating(column, text, emptyVal);
            if (emptyCheck.Cancel) return DependencyProperty.UnsetValue;
            table.RaiseCellEdited(column, emptyCheck.ParsedValue);
            return emptyCheck.ParsedValue;
        }

        if (!TryParseForTargetType(text, underlying, out object? parsed))
            return DependencyProperty.UnsetValue;

        var validation = table.RaiseCellValidating(column, text, parsed);
        if (validation.Cancel)
            return DependencyProperty.UnsetValue;

        table.RaiseCellEdited(column, validation.ParsedValue);
        return validation.ParsedValue;
    }

    private bool TryParseForTargetType(string text, Type targetType, out object? result)
    {
        if (targetType == typeof(string))
        {
            if (!string.IsNullOrEmpty(column.InputMask))
            {
                result = column.StripMaskOnSave
                    ? StripMask(text, column.InputMask)
                    : ApplyMask(StripMask(text, column.InputMask), column.InputMask);
            }
            else
            {
                result = text;
            }
            return true;
        }

        // Clean numeric input (strips currency symbols, trailing unit text like "pcs", and whitespace)
        string numericText = CleanNumericText(text);

        if (targetType == typeof(decimal) && decimal.TryParse(numericText, NumberStyles.Any, CultureInfo.CurrentCulture, out var dec))
        { result = dec; return true; }

        if (targetType == typeof(double) && double.TryParse(numericText, NumberStyles.Any, CultureInfo.CurrentCulture, out var dbl))
        { result = dbl; return true; }

        if (targetType == typeof(float) && float.TryParse(numericText, NumberStyles.Any, CultureInfo.CurrentCulture, out var flt))
        { result = flt; return true; }

        if (targetType == typeof(int) && int.TryParse(numericText, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var i32))
        { result = i32; return true; }

        if (targetType == typeof(long) && long.TryParse(numericText, NumberStyles.Integer | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var i64))
        { result = i64; return true; }

        if (targetType == typeof(DateTime))
        {
            if (!string.IsNullOrEmpty(column.DisplayFormat) &&
                DateTime.TryParseExact(text, column.DisplayFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dtExact))
            { result = dtExact; return true; }

            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, out var dt))
            { result = dt; return true; }
        }

        if (targetType == typeof(DateOnly))
        {
            if (!string.IsNullOrEmpty(column.DisplayFormat) &&
                DateOnly.TryParseExact(text, column.DisplayFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var doExact))
            { result = doExact; return true; }

            if (DateOnly.TryParse(text, CultureInfo.CurrentCulture, out var dOnly))
            { result = dOnly; return true; }
        }

        if (targetType == typeof(TimeOnly))
        {
            if (!string.IsNullOrEmpty(column.DisplayFormat) &&
                TimeOnly.TryParseExact(text, column.DisplayFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out var toExact))
            { result = toExact; return true; }

            if (TimeOnly.TryParse(text, CultureInfo.CurrentCulture, out var tOnly))
            { result = tOnly; return true; }
        }

        if (targetType == typeof(bool) && bool.TryParse(text, out var b))
        { result = b; return true; }

        result = null;
        return false;
    }

    private static string CleanNumericText(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (char.IsDigit(c) || c is '-' or '+' or '.' or ',')
                sb.Append(c);
        }
        return sb.ToString();
    }

    private static string ApplyMask(string raw, string mask)
    {
        var clean = StripMask(raw, mask);
        if (clean.Length == 0) return string.Empty;

        var sb = new StringBuilder(mask.Length);
        int idx = 0;

        foreach (char m in mask)
        {
            if (idx >= clean.Length) break;

            if (m == '0')
            {
                while (idx < clean.Length && !char.IsDigit(clean[idx])) idx++;
                if (idx < clean.Length) sb.Append(clean[idx++]);
            }
            else if (m == 'A')
            {
                while (idx < clean.Length && !char.IsLetterOrDigit(clean[idx])) idx++;
                if (idx < clean.Length) sb.Append(clean[idx++]);
            }
            else if (m == '#')
            {
                sb.Append(clean[idx++]);
            }
            else
            {
                sb.Append(m);
            }
        }

        return sb.ToString();
    }

    private static string StripMask(string masked, string mask)
    {
        // Keep only characters matching placeholder positions or raw alphanumerics
        var maskLiterals = new HashSet<char>(mask.Where(c => c is not ('0' or 'A' or '#')));
        var sb = new StringBuilder(masked.Length);
        foreach (char c in masked)
        {
            if (!maskLiterals.Contains(c))
                sb.Append(c);
        }
        return sb.ToString();
    }
}

#endif