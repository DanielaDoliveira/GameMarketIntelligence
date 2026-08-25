
using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Infrastructure.ExternalServices.Images;

public sealed class GameImageUrlResolver : IGameImageUrlResolver
{
    private const string IgdbDataSourceCode = "igdb";
    private const string IgdbImageBaseUrl = "https://images.igdb.com/igdb/image/upload";

    public string? Resolve(string dataSourceCode, string sourceImageId, GameImageType imageType)
    {
        if (string.IsNullOrWhiteSpace(dataSourceCode) || string.IsNullOrWhiteSpace(sourceImageId))
            return null;

        if (!string.Equals(dataSourceCode.Trim(), IgdbDataSourceCode, StringComparison.OrdinalIgnoreCase))
            return null;

        var size = imageType switch
        {
            GameImageType.Cover => "cover_big",
            GameImageType.Screenshot => "screenshot_big",
            _ => null
        };

        if (size is null)
            return null;

        var normalizedSourceImageId = sourceImageId.Trim();

        return $"{IgdbImageBaseUrl}/t_{size}/{normalizedSourceImageId}.jpg";
    }
}