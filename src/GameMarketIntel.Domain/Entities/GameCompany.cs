using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.Entities;

public sealed class GameCompany
{
    public Guid GameId { get; private set; }

    public Guid CompanyId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    public Guid ExternalCompanyRecordId { get; private set; }

    public GameCompanyRole Role { get; private set; }

    private GameCompany()
    {
    }

    public GameCompany(ExternalGameRecord externalGameRecord, ExternalCompanyRecord externalCompanyRecord, GameCompanyRole role)
    {
        ArgumentNullException.ThrowIfNull(externalGameRecord);
        ArgumentNullException.ThrowIfNull(externalCompanyRecord);

        if (externalGameRecord.GameId is null)
            throw new ArgumentException(
                "The external game record must be linked to a canonical game.",
                nameof(externalGameRecord));

        if (externalCompanyRecord.CompanyId is null)
            throw new ArgumentException(
                "The external company record must be linked to a canonical company.",
                nameof(externalCompanyRecord));

        if (externalGameRecord.DataSourceId != externalCompanyRecord.DataSourceId)
            throw new ArgumentException(
                "The external game and company records must belong to the same data source.",
                nameof(externalCompanyRecord));

        GameId = externalGameRecord.GameId.Value;
        CompanyId = externalCompanyRecord.CompanyId.Value;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalCompanyRecordId = externalCompanyRecord.Id;
        Role = role;
    }
}