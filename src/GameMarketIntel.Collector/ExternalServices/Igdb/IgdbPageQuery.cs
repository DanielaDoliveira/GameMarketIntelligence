namespace GameMarketIntel.Collector.ExternalServices.Igdb;

/// <summary>
/// Builds a single-page query with explicit fields and identifier ordering.
/// This is a restricted query builder, not a general APICalypse parser.
/// </summary>
public sealed class IgdbPageQuery

{
    private readonly string _fields;
    private readonly string? _filter;
    public int PageSize { get; }

    /// <param name="fields">Comma-separated field paths, without the fields keyword or wildcards.</param>
    /// <param name="pageSize">Maximum number of records requested per page, from 1 to 500.</param>
    /// <param name="filter">
    /// Optional trusted application-defined filter, without the where keyword or semicolons.
    /// Do not interpolate unescaped user input. Filter semantics are validated by IGDB.
    /// </param>
    public IgdbPageQuery(string fields, int pageSize, string? filter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fields);

        if (pageSize < 1 || pageSize > 500)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                "Page size must be between 1 and 500."
            );
        var fieldPaths = fields.Split(',', StringSplitOptions.TrimEntries);

        if (fieldPaths.Any(field => !IsValidFieldPath(field)))
            throw new ArgumentException
            (
                "Fields must be explicit comma-separated field paths.",
                nameof(fields)
            );
        // Deliberately reject all semicolons, including those inside string literals.
        if (filter?.Contains(';') == true)
            throw new ArgumentException("The filter must not contain semicolons or additional query clauses.", nameof(filter));

        _fields = string.Join(",", fieldPaths);
        _filter = string.IsNullOrWhiteSpace(filter) ? null : filter.Trim();
        PageSize = pageSize;
    }
    public string Build(int offset)
    {
        if (offset < 0)
            throw new ArgumentOutOfRangeException
                (nameof(offset),
                    "Offset must not be negative."
                    );

        var filterClause = _filter is null ? string.Empty : $"where {_filter}; ";

        return FormattableString.Invariant($"fields {_fields}; {filterClause}sort id asc; limit {PageSize}; offset {offset};");
    }

    private static bool IsValidFieldPath(string field) =>
        field.Split('.').All(IsValidIdentifier);
    private static bool IsValidIdentifier(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return false;

        if (!char.IsAsciiLetter(identifier[0]) && identifier[0] != '_')
            return false;
        return identifier.All(character => char.IsAsciiLetterOrDigit(character) || character == '_');
    }
}