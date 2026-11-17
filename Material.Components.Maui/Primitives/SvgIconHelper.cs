using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Material.Components.Maui.Primitives;

public static class SvgIconHelper
{
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Synchronously loads SVG path data from Resources/Raw (e.g. "addcircle.svg" or "Icons/addcircle.svg").
    /// Safe for XAML MarkupExtensions on Windows & Android without UI-thread deadlocks.
    /// </summary>
    public static string GetPathData(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        return Cache.GetOrAdd(fileName, LoadPathDataInternal);
    }

    /// <summary>
    /// Async version for BindableProperty handlers (IconSource).
    /// </summary>
    public static async Task<string> GetPathDataAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        if (Cache.TryGetValue(fileName, out var cached))
            return cached;

        string svgXml = await ReadSvgTextAsync(fileName);
        string pathData = ExtractPathData(svgXml);
        Cache[fileName] = pathData;
        return pathData;
    }

    private static string LoadPathDataInternal(string fileName)
    {
        // On Windows, MauiAsset files exist directly under AppContext.BaseDirectory
        foreach (var candidate in GetCandidateNames(fileName))
        {
            string diskPath = Path.Combine(AppContext.BaseDirectory, candidate.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(diskPath))
                return ExtractPathData(File.ReadAllText(diskPath));
        }

        // Cross-platform fallback via Task.Run (avoids SynchronizationContext deadlock on WinUI)
        string xml = Task.Run(() => ReadSvgTextAsync(fileName)).GetAwaiter().GetResult();
        return ExtractPathData(xml);
    }

    private static async Task<string> ReadSvgTextAsync(string fileName)
    {
        foreach (var candidate in GetCandidateNames(fileName))
        {
            try
            {
                using Stream stream = await FileSystem.OpenAppPackageFileAsync(candidate);
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch
            {
                // Try next candidate path
            }
        }

        return string.Empty;
    }

    private static IEnumerable<string> GetCandidateNames(string fileName)
    {
        string normalized = fileName.TrimStart('/', '\\');
        if (!normalized.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            normalized += ".svg";

        yield return normalized;

        if (!normalized.StartsWith("Icons/", StringComparison.OrdinalIgnoreCase))
            yield return $"Icons/{normalized}";
    }

    private static string ExtractPathData(string svgXml)
    {
        if (string.IsNullOrWhiteSpace(svgXml))
            return string.Empty;

        var doc = XDocument.Parse(svgXml);
        var parts = new List<string>();

        foreach (var element in doc.Descendants())
        {
            string tag = element.Name.LocalName;

            if (tag == "path")
            {
                string? d = element.Attribute("d")?.Value;
                if (!string.IsNullOrWhiteSpace(d))
                    parts.Add(NormalizeSubPathStart(d.Trim()));
            }
            else if (tag == "rect")
            {
                if (TryGetRectPath(element, out var rectPath))
                    parts.Add(rectPath);
            }
            else if (tag == "circle")
            {
                if (TryGetCirclePath(element, out var circlePath))
                    parts.Add(circlePath);
            }
            else if (tag is "polygon" or "polyline")
            {
                if (TryGetPolyPath(element, close: tag == "polygon", out var polyPath))
                    parts.Add(polyPath);
            }
        }

        return string.Join(" ", parts);
    }

    private static string NormalizeSubPathStart(string d)
    {
        // When combining multiple <path> elements, a lowercase 'm' at the start becomes relative to the previous path end
        return d.StartsWith('m') ? "M" + d[1..] : d;
    }

    private static bool TryGetRectPath(XElement el, out string path)
    {
        double x = ParseDouble(el.Attribute("x")?.Value);
        double y = ParseDouble(el.Attribute("y")?.Value);
        double w = ParseDouble(el.Attribute("width")?.Value);
        double h = ParseDouble(el.Attribute("height")?.Value);
        path = FormattableString.Invariant($"M{x},{y} h{w} v{h} h{-w} Z");
        return w > 0 && h > 0;
    }

    private static bool TryGetCirclePath(XElement el, out string path)
    {
        double cx = ParseDouble(el.Attribute("cx")?.Value);
        double cy = ParseDouble(el.Attribute("cy")?.Value);
        double r = ParseDouble(el.Attribute("r")?.Value);
        path = FormattableString.Invariant($"M{cx - r},{cy} a{r},{r} 0 1,0 {r * 2},0 a{r},{r} 0 1,0 {-r * 2},0 Z");
        return r > 0;
    }

    private static bool TryGetPolyPath(XElement el, bool close, out string path)
    {
        string? raw = el.Attribute("points")?.Value;
        path = string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        var nums = Regex.Matches(raw, @"-?\d*\.?\d+")
                        .Select(m => m.Value)
                        .ToList();
        if (nums.Count < 4) return false;

        var cmds = new List<string> { $"M{nums[0]},{nums[1]}" };
        for (int i = 2; i + 1 < nums.Count; i += 2)
            cmds.Add($"L{nums[i]},{nums[i + 1]}");

        if (close) cmds.Add("Z");
        path = string.Join(" ", cmds);
        return true;
    }

    private static double ParseDouble(string? value)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0;
    }
}