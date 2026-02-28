using Microsoft.Extensions.Logging;

namespace Core.ExternalApiClient;

public static partial class HttpLoggingMiddlewareLoggerMessages
{
    [LoggerMessage(LogLevel.Information,
        "Sent HTTP Request {method} {uri} - \nBody: {requestBody} - \nHeaders: {headers} - \nGot response {httpStatus} {responseBody} - \nElapsed {elapsedMilliseconds}")]
    public static partial void LogHttpRequestResponse(
        this ILogger logger, HttpMethod method, Uri? uri, string requestBody, string headers,
        int httpStatus, string responseBody, long elapsedMilliseconds);

    [LoggerMessage(LogLevel.Warning,
        "Sent HTTP Request {method} {uri} - \nBody: {requestBody} - \nHeaders: {headers} - \nGot error - \nElapsed {elapsedMilliseconds}")]
    public static partial void LogHttpRequestErrorResponse(
        this ILogger logger, HttpMethod method, Uri? uri, string requestBody, string headers,
        long elapsedMilliseconds, Exception exception);
}