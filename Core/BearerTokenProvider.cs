using Core.ExternalApiClient;
using Microsoft.Extensions.Logging;

namespace Core;

public class BearerTokenProvider(
    ILogger<BearerTokenProvider> logger,
    IExternalApiConnector externalApiConnector,
    ClientContextProvider clientContextProvider)
{
    public string? CurrentBearerToken { get; set; }

    public async Task<string> GetBearerTokenAsync()
    {
        var clientContext = clientContextProvider.CurrentClientContext ??
                            throw new InvalidOperationException("Client context not set");
        
        //TODO introduce an in-memory cache here for a very short expiration to make sure that the cache expiry before the JWT
        CurrentBearerToken = await externalApiConnector.GetBearerToken(
            clientContext.PlatformId ?? throw new InvalidOperationException("PlatformId not set in client, context"),
            clientContext.EnvironmentId ??
            throw new InvalidOperationException("EnvironmentId not set in client, context")
        );
        
        return CurrentBearerToken;
    }
}