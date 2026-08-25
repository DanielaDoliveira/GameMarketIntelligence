namespace GameMarketIntel.Domain.Enums;

public interface IGameImageUrlResolver
{
    string? Resolve(string dataSourceCode, string sourceImageId, GameImageType imageType);
}