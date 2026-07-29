using GameMarketIntel.Collector.Igdb.Authentication;

namespace GameMarketIntel.Collector.Workers;

    public sealed class Worker(
       IIgdbAuthenticationService authenticationService,
       IHostApplicationLifetime applicationLifetime,
       ILogger<Worker> logger
        ) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Starting IGDB authentication proof of concept");
                var tokenResponse = await authenticationService.GetAccessTokenAsync(stoppingToken);
                if(string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                    throw new InvalidOperationException("Twitch returned an empty access token");
                logger.LogInformation(
                    """
                    IGDB authentication succeeded.
                    Token type: {TokenType}
                    Expires in: {ExpiresIn} seconds
                    """,
                    tokenResponse.TokenType,
                    tokenResponse.ExpiresIn);;
                    
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("IGDB authentication proof of concept was cancelled.");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "IGDB authentication proof of concept failed.");
            }
            finally
            {
                applicationLifetime.StopApplication();
            }
        }
    }
