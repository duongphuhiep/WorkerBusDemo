using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Core.ExternalApiClient;

/// <summary>
///     HttpClient middleware to log outgoing HTTP requests and responses, including method, URI, headers, body, response
///     status code, and elapsed time.
///     Request and Response (whatever Success or Error) are in the same structured log entry, no need to correlate them in
///     the log viewer.
///     Sensitive headers like Authorization are masked in the logs.
///     Warning: this middleware is not designed for streaming traffic.
/// </summary>
/// <param name="logger"></param>
public class HttpLoggingMiddleware(ILogger<HttpLoggingMiddleware> logger) : DelegatingHandler
{
    private const int MaxBodyLength = 65536 * 2;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await LogRequestResponse(request, response, sw.ElapsedMilliseconds).ConfigureAwait(false);
            return response;
        }
        catch (Exception ex)
        {
            await LogRequestException(request, ex, sw.ElapsedMilliseconds).ConfigureAwait(false);
            throw;
        }
    }

    private async Task LogRequestResponse(HttpRequestMessage request, HttpResponseMessage response,
        long elapsedMilliseconds)
    {
        // Short-circuit: skip all body/header work if logging is disabled
        if (!logger.IsEnabled(LogLevel.Information)) return;

        var headers = FormatRequestHeader(request);
        var requestBody = await FormatRequestBody(request).ConfigureAwait(false);
        var responseBody = await FormatResponseBody(response).ConfigureAwait(false);
        logger.LogHttpRequestResponse(request.Method, request.RequestUri,
            requestBody, headers, (int)response.StatusCode, responseBody, elapsedMilliseconds);
    }

    private async Task LogRequestException(HttpRequestMessage request, Exception exception, long elapsedMilliseconds)
    {
        // Short-circuit: errors are typically logged at Warning/Error level.
        // Adjust the level below to match what LogHttpRequestErrorResponse actually uses.
        if (!logger.IsEnabled(LogLevel.Warning)) return;

        var headers = FormatRequestHeader(request);
        var requestBody = await FormatRequestBody(request).ConfigureAwait(false);
        logger.LogHttpRequestErrorResponse(request.Method,
            request.RequestUri, requestBody, headers, elapsedMilliseconds, exception);
    }

    private string FormatRequestHeader(HttpRequestMessage request)
    {
        StringBuilder formattedHeaders = new();

        formattedHeaders.AppendLine("[");
        if (request.Headers.Any())
        {
            
            foreach (var header in request.Headers)
            {
                var key = header.Key.ToLowerInvariant();
                
                //the Authorization header is not masked for Demo purpose.
                // var value = key.Contains("authorization", StringComparison.Ordinal) ||
                //             key.Contains("token", StringComparison.Ordinal)
                //     ? "(MASKED)"
                //     : string.Join(", ", header.Value);
                var value = string.Join(", ", header.Value);
                
                formattedHeaders.AppendLine($" {header.Key}: {value};");
            }

            if (request.Content?.Headers != null && request.Content.Headers.Any())
                foreach (var header in request.Content.Headers)
                    formattedHeaders.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)};");
        }

        formattedHeaders.AppendLine("]");

        return formattedHeaders.ToString();
    }

    private async Task<string> FormatRequestBody(HttpRequestMessage request)
    {
        if (request.Content == null) return string.Empty;

        var contentType = request.Content.Headers.ContentType?.MediaType;

        if (!IsTextContentType(contentType))
            return contentType != null ? "(Binary content not logged)" : "(Unknown contentType, content not logged)";

        // Buffer the content so it can be read multiple times (once for logging, once for the actual request)
        await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        var body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);

        return TrimIfExceedsMaxLength(body);
    }

    private async Task<string> FormatResponseBody(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (!IsTextContentType(contentType))
            return contentType != null ? "(Binary content not logged)" : "(Unknown contentType, content not logged)";

        // Buffer the content so it can be read multiple times (once for logging, once for the actual response consumption by the caller)
        await response.Content.LoadIntoBufferAsync().ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return TrimIfExceedsMaxLength(body);
    }

    private static string TrimIfExceedsMaxLength(string body)
    {
        if (body.Length <= MaxBodyLength) return body;

        // Trim to MaxBodyLength characters
        var trimmed = body[..MaxBodyLength];
        return trimmed + $"... (trimmed, exceeded {MaxBodyLength} characters)";
    }

    private bool IsTextContentType(string? contentType)
    {
        return contentType != null && (contentType.Contains("json", StringComparison.Ordinal) ||
                                       contentType.Contains("text", StringComparison.Ordinal) ||
                                       contentType.Contains("xml", StringComparison.Ordinal));
    }
}