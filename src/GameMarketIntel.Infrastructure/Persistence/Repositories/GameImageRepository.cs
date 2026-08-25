using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GameMarketIntel.Infrastructure.Persistence.Repositories;

public sealed class GameImageRepository(
    GameMarketIntelDbContext dbContext) : IGameImageRepository
{
    public async Task<GameImageReference?> GetPrimaryCoverAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.GameImages
            .AsNoTracking()
            .Where(image =>
                image.GameId == gameId &&
                image.Type == GameImageType.Cover)
            .Join(
                dbContext.ExternalGameRecords.AsNoTracking(),
                image => image.ExternalGameRecordId,
                externalGameRecord => externalGameRecord.Id,
                (image, externalGameRecord) => new
                {
                    Image = image,
                    externalGameRecord.DataSourceId
                })
            .Join(
                dbContext.DataSources.AsNoTracking(),
                item => item.DataSourceId,
                dataSource => dataSource.Id,
                (item, dataSource) => new
                {
                    item.Image.SourceImageId,
                    item.Image.Type,
                    item.Image.SortOrder,
                    DataSourceCode = dataSource.Code,
                    item.Image.Id
                })
            .OrderBy(item => item.SortOrder == null)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .Select(item => new GameImageReference(
                item.DataSourceCode,
                item.SourceImageId,
                item.Type))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, GameImageReference>> GetPrimaryCoversAsync(
        IReadOnlyCollection<Guid> gameIds,
        CancellationToken cancellationToken = default)
    {
        if (gameIds.Count == 0)
            return new Dictionary<Guid, GameImageReference>();

        var covers = await dbContext.GameImages
            .AsNoTracking()
            .Where(image =>
                gameIds.Contains(image.GameId) &&
                image.Type == GameImageType.Cover)
            .Join(
                dbContext.ExternalGameRecords.AsNoTracking(),
                image => image.ExternalGameRecordId,
                externalGameRecord => externalGameRecord.Id,
                (image, externalGameRecord) => new
                {
                    Image = image,
                    externalGameRecord.DataSourceId
                })
            .Join(
                dbContext.DataSources.AsNoTracking(),
                item => item.DataSourceId,
                dataSource => dataSource.Id,
                (item, dataSource) => new
                {
                    item.Image.GameId,
                    item.Image.SourceImageId,
                    item.Image.Type,
                    item.Image.SortOrder,
                    DataSourceCode = dataSource.Code,
                    item.Image.Id
                })
            .OrderBy(item => item.GameId)
            .ThenBy(item => item.SortOrder == null)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return covers
            .GroupBy(cover => cover.GameId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var cover = group.First();

                    return new GameImageReference(
                        cover.DataSourceCode,
                        cover.SourceImageId,
                        cover.Type);
                });
    }
}