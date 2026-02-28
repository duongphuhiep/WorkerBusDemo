using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Core.ExternalApiClient;

public class AuthHeaderMiddleware(BearerTokenProvider bearerTokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await bearerTokenProvider.GetBearerTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}