using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Shared.Contracts.Games;

public sealed record GameDetails(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? ReleaseDate,
    string? ImageUrl,
    IReadOnlyList<GenreDetails> Genres,
    IReadOnlyList<PlatformDetails> Platforms);