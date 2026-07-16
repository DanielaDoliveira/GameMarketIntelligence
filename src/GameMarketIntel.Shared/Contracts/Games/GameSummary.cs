using GameMarketIntel.Shared.Contracts.Genres;
using GameMarketIntel.Shared.Contracts.Platforms;

namespace GameMarketIntel.Shared.Contracts.Games;

public sealed record GameSummary(
    Guid Id,
    string Name,
    DateOnly? ReleaseDate,
    string? ImageUrl,
    IReadOnlyList<GenreDetails> Genres,
    IReadOnlyList<PlatformDetails> Platforms);