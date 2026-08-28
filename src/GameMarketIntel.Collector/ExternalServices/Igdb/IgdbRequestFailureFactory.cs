namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public static class IgdbRequestFailureFactory
{
    public static HttpRequestException CreateException(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        if (response.IsSuccessStatusCode)
            throw new ArgumentException
            (
                "A successful response cannot represent an IGDB request failure.",
                nameof(response)
            );

        return new HttpRequestException
        (
            $"IGDB request failed with status code {(int)response.StatusCode}.",
            null,
            response.StatusCode
        );

    }
}