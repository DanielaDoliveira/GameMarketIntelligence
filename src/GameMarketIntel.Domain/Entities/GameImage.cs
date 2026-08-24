using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.Entities;

public sealed class GameImage
{
    public const int MaxExternalIdLength = 100;
    public const int MaxSourceImageIdLength = 200;

    public Guid Id { get; private set; }

    public Guid GameId { get; private set; }

    public Guid ExternalGameRecordId { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string SourceImageId { get; private set; } = string.Empty;

    public GameImageType Type { get; private set; }

    public int? Width { get; private set; }

    public int? Height { get; private set; }

    public int? SortOrder { get; private set; }

    private GameImage()
    {
    }

    public GameImage(
        ExternalGameRecord externalGameRecord,
        string externalId,
        string sourceImageId,
        GameImageType type,
        int? width = null,
        int? height = null,
        int? sortOrder = null)
    {
        ArgumentNullException.ThrowIfNull(externalGameRecord);

        if (!externalGameRecord.GameId.HasValue)
            throw new InvalidOperationException(
                "The external game record must be linked to a canonical game.");

        ValidateExternalId(externalId);
        ValidateSourceImageId(sourceImageId);
        ValidateDimension(width, nameof(width));
        ValidateDimension(height, nameof(height));
        ValidateSortOrder(sortOrder);

        Id = Guid.NewGuid();
        GameId = externalGameRecord.GameId.Value;
        ExternalGameRecordId = externalGameRecord.Id;
        ExternalId = externalId.Trim();
        SourceImageId = sourceImageId.Trim();
        Type = type;
        Width = width;
        Height = height;
        SortOrder = sortOrder;
    }

    private static void ValidateExternalId(string externalId)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException(
                "External image id is required.",
                nameof(externalId));

        if (externalId.Trim().Length > MaxExternalIdLength)
            throw new ArgumentException(
                $"External image id must not exceed {MaxExternalIdLength} characters.",
                nameof(externalId));
    }

    private static void ValidateSourceImageId(string sourceImageId)
    {
        if (string.IsNullOrWhiteSpace(sourceImageId))
            throw new ArgumentException(
                "Source image id is required.",
                nameof(sourceImageId));

        if (sourceImageId.Trim().Length > MaxSourceImageIdLength)
            throw new ArgumentException(
                $"Source image id must not exceed {MaxSourceImageIdLength} characters.",
                nameof(sourceImageId));
    }

    private static void ValidateDimension(int? dimension, string parameterName)
    {
        if (dimension.HasValue && dimension.Value <= 0)
            throw new ArgumentOutOfRangeException(
                parameterName,
                "Image dimensions must be greater than zero.");
    }

    private static void ValidateSortOrder(int? sortOrder)
    {
        if (sortOrder.HasValue && sortOrder.Value < 0)
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Image sort order cannot be negative.");
    }
}