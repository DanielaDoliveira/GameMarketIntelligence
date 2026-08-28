namespace GameMarketIntel.Collector.ExternalServices.Igdb.Authentication;

public sealed record TwitchAccessToken(string Value, DateTimeOffset ExpiresAt);