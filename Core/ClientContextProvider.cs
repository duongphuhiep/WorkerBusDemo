using Core.Model;
using Microsoft.Extensions.Logging;

namespace Core;

/// <summary>
///     Equivalent to HttpContextAccessor. It holds a cross's scope context.
///     Consumer shouldn't run concurrent await branches (WhenAll) before SetCurrentBearerToken finishes.
///     For eg:
///     <code>
/// await SetClientContextAsync(); //good ✅
/// await Task.WhenAll(
///     TaskA(), // SetClientContextAsync() ❌
///     TaskB()  // GetClientContextAsync() cannot see the ClientContext set by TaskA
///  );
/// </code>
/// </summary>
public class ClientContextProvider(ILogger<ClientContextProvider> logger)
{
    private static readonly AsyncLocal<ClientContext?> CurrentClientContextAsyncLocal = new();

    public ClientContext CurrentClientContext => CurrentClientContextAsyncLocal.Value ??
                                                 throw new InvalidOperationException("Client context not set");

    public Task<ClientContext> SetClientContextAsync(string platformId, string environmentId)
    {
        if (CurrentClientContextAsyncLocal.Value != null) throw new InvalidOperationException("Already set");
        var currentClientContext = new ClientContext
        {
            EnvironmentId = environmentId,
            PlatformId = platformId
        };

        logger.LogInformation("SetClientContextAsync {platformId} {environmentId}", platformId, environmentId);
        CurrentClientContextAsyncLocal.Value = currentClientContext;

        return Task.FromResult(currentClientContext);
    }
}