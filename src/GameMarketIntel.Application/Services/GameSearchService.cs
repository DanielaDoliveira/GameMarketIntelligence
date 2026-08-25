using FluentValidation;

using GameMarketIntel.Application.Abstractions.Persistence;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Domain.Enums;
using GameMarketIntel.Shared.Contracts.Games.Search;

namespace GameMarketIntel.Application.Services;

public sealed class GameSearchService(
    IGameSearchRepository repository,
    IValidator<SearchGamesQuery> validator,
    IGameImageRepository gameImageRepository,
    IGameImageUrlResolver gameImageUrlResolver) : IGameSearchService
{
    public async Task<SearchGamesResult> SearchAsync(
        SearchGamesQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await validator.ValidateAndThrowAsync(
            query,
            cancellationToken);

        var result = await repository.SearchAsync(
            query,
            cancellationToken);

        if (result.Items.Count == 0)
            return result;

        var gameIds = result.Items
            .Select(item => item.Id)
            .ToArray();

        var primaryCovers = await gameImageRepository.GetPrimaryCoversAsync(
            gameIds,
            cancellationToken);

        var items = result.Items
            .Select(item =>
            {
                if (!primaryCovers.TryGetValue(
                        item.Id,
                        out var primaryCover))
                    return item;

                var imageUrl = gameImageUrlResolver.Resolve(
                    primaryCover.DataSourceCode,
                    primaryCover.SourceImageId,
                    primaryCover.Type);

                return item with
                {
                    ImageUrl = imageUrl
                };
            })
            .ToArray();

        return result with
        {
            Items = items
        };
    }
}