using System.Globalization;

namespace litnovel_frontend.Services;

public static class ODataQuery
{
    public static string Build(int page = 1, int size = 20, string? orderBy = null, IEnumerable<string>? filters = null)
    {
        var parts = new List<string>
        {
            $"$top={size}",
            $"$skip={Math.Max(0, page - 1) * size}"
        };

        var activeFilters = filters?.Where(f => !string.IsNullOrWhiteSpace(f)).ToList() ?? [];
        if (activeFilters.Count > 0)
        {
            parts.Add("$filter=" + Uri.EscapeDataString(string.Join(" and ", activeFilters)));
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            parts.Add("$orderby=" + Uri.EscapeDataString(orderBy));
        }

        return "?" + string.Join("&", parts);
    }

    public static string Eq(string property, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "" : $"{property} eq '{Escape(value)}'";
    }

    public static string Eq(string property, int value)
    {
        return value <= 0 ? "" : $"{property} eq {value.ToString(CultureInfo.InvariantCulture)}";
    }

    public static string ContainsAny(string value, params string[] properties)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        var escaped = Escape(value.Trim().ToLowerInvariant());
        return "(" + string.Join(" or ", properties.Select(p => $"contains(tolower({p}),'{escaped}')")) + ")";
    }

    public static string OrderBy(string sort, bool descending = true)
    {
        var property = sort switch
        {
            "viewCount" => "ViewCount",
            "ratingAverage" => "RatingAverage",
            "chapterNumber" => "ChapterNumber",
            "updatedAt" => "UpdatedAt",
            _ => ToPascalCase(sort)
        };

        return property + (descending ? " desc" : " asc");
    }

    private static string Escape(string value) => value.Replace("'", "''");

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "UpdatedAt";
        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}
