namespace GameMarketIntel.Domain.Entities;

public sealed class ExternalCompanyRecord
{
    public const int MaxExternalIdLength = 100;

    public Guid Id { get; private set; }

    public Guid DataSourceId { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public Guid? CompanyId { get; private set; }

    public DateTimeOffset FirstSeenAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset? SourceUpdatedAt { get; private set; }

    private ExternalCompanyRecord() { }

    public ExternalCompanyRecord(Guid dataSourceId, string externalId, DateTimeOffset observedAt)
    {
        if (dataSourceId == Guid.Empty)
            throw new ArgumentException(
                "The data source ID cannot be empty.",
                nameof(dataSourceId));

        if (observedAt == default)
            throw new ArgumentException(
                "The observed timestamp cannot be the default value.",
                nameof(observedAt));

        Id = Guid.NewGuid();
        DataSourceId = dataSourceId;
        ExternalId = NormalizeAndValidateExternalId(externalId);
        FirstSeenAt = observedAt;
        LastSeenAt = observedAt;
    }

    public void LinkToCompany(Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException(
                "The company ID cannot be empty.",
                nameof(companyId));

        if (CompanyId.HasValue && CompanyId.Value != companyId)
            throw new InvalidOperationException(
                "The external company record is already linked to another company.");

        CompanyId = companyId;
    }

    public void MarkSeen(DateTimeOffset observedAt, DateTimeOffset? sourceUpdatedAt = null)
    {
        if (observedAt == default)
            throw new ArgumentException(
                "The observed timestamp cannot be the default value.",
                nameof(observedAt));

        if (observedAt > LastSeenAt)
            LastSeenAt = observedAt;

        if (sourceUpdatedAt.HasValue &&
            (!SourceUpdatedAt.HasValue || sourceUpdatedAt.Value > SourceUpdatedAt.Value))
            SourceUpdatedAt = sourceUpdatedAt;
    }

    private static string NormalizeAndValidateExternalId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException(
                "The external ID cannot be null, empty or whitespace.",
                nameof(externalId));

        var normalizedExternalId = externalId.Trim();

        if (normalizedExternalId.Length > MaxExternalIdLength)
            throw new ArgumentException(
                $"The external ID cannot exceed {MaxExternalIdLength} characters.",
                nameof(externalId));

        return normalizedExternalId;
    }
}