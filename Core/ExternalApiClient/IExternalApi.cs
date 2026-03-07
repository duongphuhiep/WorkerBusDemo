using Core.ExternalApiClient.Dtos;
using Refit;

namespace Core.ExternalApiClient;

[ApiClient("ExternalApi:BaseUrlConnector",
    TimeoutSeconds = 10,
    UserAgent = "ExternalApiConnector/1.0",
    MessageHandlers = [typeof(HttpLoggingMiddleware)])]
public interface IExternalApiConnector
{
    [Get("/bearerToken/{platformId}/{environmentId}")]
    Task<BearerToken> GetBearerToken(string platformId, string environmentId);
}

[ApiClient("ExternalApi:BaseUrlService",
    TimeoutSeconds = 30,
    UserAgent = "ExternalApiService/2.0",
    MessageHandlers = [typeof(AuthHeaderMiddleware), typeof(HttpLoggingMiddleware)])]
public interface IExternalApi
{
    [Put("/product")]
    Task<Product> CreatePendingProduct(ClientContext clientContext);

    [Post("/queryPendingProduct/{productId}")]
    Task<Product> GetPendingProduct(string productId, ClientContext clientContext);

    [Post("/deploy")]
    Task<DeploymentReport> DeployProduct(DeploymentRequest deploymentRequest);
}

[AttributeUsage(AttributeTargets.Interface)]
public class ApiClientAttribute(string baseUrlConfigKey) : Attribute
{
    /// <summary>
    ///     Placeholder for configuration value
    /// </summary>
    public string BaseUrlConfigKey { get; } = baseUrlConfigKey;

    public int TimeoutSeconds { get; set; } = 30;

    public string UserAgent { get; set; } = string.Empty;

    public Type[] MessageHandlers { get; set; } = [];
}