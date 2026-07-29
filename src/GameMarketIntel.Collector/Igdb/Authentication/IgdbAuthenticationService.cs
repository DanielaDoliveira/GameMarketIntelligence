using System.Net.Http.Json;
using GameMarketIntel.Collector.Igdb.Poc;
using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.Igdb.Authentication;

public sealed class IgdbAuthenticationService(
    HttpClient httpClient, 
    IOptions<IgdbPocOptions> options ): IIgdbAuthenticationService
{
    private readonly IgdbPocOptions _options = options.Value;
    
    public async Task<IgdbAccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var endpoint =
            $"https://id.twitch.tv/oauth2/token" +
            $"?client_id={Uri.EscapeDataString(_options.ClientId)}" +
            $"&client_secret={Uri.EscapeDataString(_options.ClientSecret)}" +
            $"&grant_type=client_credentials";

        using var response = await httpClient.PostAsync(endpoint, content: null, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<IgdbAccessTokenResponse>(cancellationToken);
        
        return token ?? throw new InvalidOperationException("Twitch returned an empty token response");
        
    }
}