using System.Text.Json.Serialization;

namespace GameMarketIntel.Collector.Igdb.Contracts;

public sealed class IgdbGameSample
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("first_release_date")]
    public long? FirstReleaseDate { get; set; }

    [JsonPropertyName("updated_at")]
    public long? UpdatedAt { get; set; }

    [JsonPropertyName("game_type")]
    public IgdbGameTypeReference? GameType { get; set; }

    [JsonPropertyName("game_status")]
    public IgdbGameStatusReference? GameStatus { get; set; }

    [JsonPropertyName("version_title")]
    public string? VersionTitle { get; set; }

    [JsonPropertyName("version_parent")]
    public IgdbNamedReference? VersionParent { get; set; }

    [JsonPropertyName("parent_game")]
    public IgdbNamedReference? ParentGame { get; set; }

    [JsonPropertyName("platforms")]
    public IReadOnlyList<IgdbNamedReference> Platforms { get; set; } = [];

    [JsonPropertyName("genres")]
    public IReadOnlyList<IgdbNamedReference> Genres { get; set; } = [];

    [JsonPropertyName("game_modes")]
    public IReadOnlyList<IgdbNamedReference> GameModes { get; set; } = [];

    [JsonPropertyName("player_perspectives")]
    public IReadOnlyList<IgdbNamedReference> PlayerPerspectives { get; set; } = [];

    [JsonPropertyName("franchises")]
    public IReadOnlyList<IgdbNamedReference> Franchises { get; set; } = [];

    [JsonPropertyName("collections")]
    public IReadOnlyList<IgdbNamedReference> Collections { get; set; } = [];

    [JsonPropertyName("themes")]
    public IReadOnlyList<IgdbNamedReference> Themes { get; set; } = [];

    [JsonPropertyName("keywords")]
    public IReadOnlyList<IgdbNamedReference> Keywords { get; set; } = [];

    [JsonPropertyName("alternative_names")]
    public IReadOnlyList<IgdbAlternativeNameReference> AlternativeNames { get; set; } = [];

    [JsonPropertyName("game_localizations")]
    public IReadOnlyList<IgdbGameLocalizationReference> GameLocalizations { get; set; } = [];

    [JsonPropertyName("involved_companies")]
    public IReadOnlyList<IgdbInvolvedCompanyReference> InvolvedCompanies { get; set; } = [];

    [JsonPropertyName("external_games")]
    public IReadOnlyList<IgdbExternalGameReference> ExternalGames { get; set; } = [];

    [JsonPropertyName("websites")]
    public IReadOnlyList<IgdbWebsiteReference> Websites { get; set; } = [];
    [JsonPropertyName("bundles")]
    public IReadOnlyList<IgdbNamedReference> Bundles { get; set; } = [];
    [JsonPropertyName("release_dates")]
    public IReadOnlyList<IgdbReleaseDateReference> ReleaseDates { get; set; } = [];
}
