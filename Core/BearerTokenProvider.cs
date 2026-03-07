using Core.ExternalApiClient;
using Microsoft.Extensions.Logging;

namespace Core;

public class BearerTokenProvider(
    ILogger<BearerTokenProvider> logger,
    IExternalApiConnector externalApiConnector,
    ClientContextProvider clientContextProvider)
{
    //TODO create an in-memory cache, the Key is the ClientContext, the value is the BearerToken.
    //the cached value must to be expired 2 sec before the bearerToken expired.
    
    public async Task<string> GetBearerTokenAsync()
    {
        var clientContext = clientContextProvider.CurrentClientContext ??
                            throw new InvalidOperationException("Client context not set");
        
        //TODO check the cache first. return the cached token for the clientContext if exists and not expired.
        
        //request a new BearerToken
        var currentBearerToken = await externalApiConnector.GetBearerToken(
            clientContext.PlatformId ?? throw new InvalidOperationException("PlatformId not set in client, context"),
            clientContext.EnvironmentId ??
            throw new InvalidOperationException("EnvironmentId not set in client, context")
        );
        
        //TODO put it into the cache. Make sure to set the expiration time of the cache entry to be 2 sec before the bearerToken expired.
        
        return currentBearerToken.Token;
    }
}