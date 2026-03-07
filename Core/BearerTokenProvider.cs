using Core.ExternalApiClient;
using Core.ExternalApiClient.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Core;

public class BearerTokenProvider(
    ILogger<BearerTokenProvider> logger,
    IExternalApiConnector externalApiConnector,
    ClientContextProvider clientContextProvider)
{
    private static MemoryCache _Cache = new(new MemoryCacheOptions());

    public async Task<string> GetBearerTokenAsync()
    {
        var clientContext = clientContextProvider.CurrentClientContext ??
                            throw new InvalidOperationException("Client context not set");

        if (_Cache.TryGetValue<BearerToken>(clientContext, out var cached) && cached is not null)
        {
            logger.LogDebug("Cache Hit: Returning cached bearer token for {PlatformId}/{EnvironmentId}",
                clientContext.PlatformId, clientContext.EnvironmentId);
            return cached.Token;
        }

        //request a new BearerToken
        var currentBearerToken = await externalApiConnector.GetBearerToken(
            clientContext.PlatformId ?? throw new InvalidOperationException("PlatformId not set in client, context"),
            clientContext.EnvironmentId ??
            throw new InvalidOperationException("EnvironmentId not set in client, context")
        );

        var expirationTime = currentBearerToken.ExpiryAt.AddSeconds(-2);
        if (expirationTime > DateTimeOffset.UtcNow)
        {
            _Cache.Set(clientContext, currentBearerToken, expirationTime);
            logger.LogDebug("Cached bearer token for {PlatformId}/{EnvironmentId}, expires at {ExpiryAt}",
                clientContext.PlatformId, clientContext.EnvironmentId, expirationTime);
        }
        else
        {
            logger.LogWarning("Bearer token for {PlatformId}/{EnvironmentId} expires too soon to cache",
                clientContext.PlatformId, clientContext.EnvironmentId);
        }

        return currentBearerToken.Token;
    }
}