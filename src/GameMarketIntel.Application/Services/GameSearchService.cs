using FluentValidation;
using GameMarketIntel.Application.Abstractions.Services;
using GameMarketIntel.Application.Games.Search;
using GameMarketIntel.Shared.Contracts.Games.Search;

namespace GameMarketIntel.Application.Services;

public sealed class GameSearchService : IGameSearchService
{
    private readonly IGameSearchRepository _repository;
    private readonly IValidator<SearchGamesQuery> _validator;

    public GameSearchService( IGameSearchRepository repository,IValidator<SearchGamesQuery> validator)
    {
        _repository = repository;
        _validator = validator;
    }


    public async Task<SearchGamesResult> SearchAsync( SearchGamesQuery query,CancellationToken cancellationToken = default)
    {

        ArgumentNullException.ThrowIfNull(query);
        await _validator.ValidateAndThrowAsync(query,cancellationToken);

        return await _repository.SearchAsync(query, cancellationToken);
    }
}