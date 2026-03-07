using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.ExternalApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;

namespace Core;

public static class Ioc
{
    public static void AddCoreService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExternalApi(configuration);
        
        //might be created twice for the same request
        services.AddScoped<BearerTokenProvider>();

        //HttpContextAccessor equivalent
        services.AddSingleton<ClientContextProvider>();

        services.AddScoped<DeploymentHandler>();
        
        services.Configure<AzureServiceBusConfig>(
            configuration.GetSection("AzureServiceBus")
        );
    }

    /// <summary>
    ///     Discover and register all Refit API clients annotated with ApiClientAttribute.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void AddExternalApi(this IServiceCollection services, IConfiguration configuration)
    {
        var apiClientInterfaces = typeof(Ioc).Assembly
            .GetTypes()
            .Where(t => t is { IsInterface: true } && t.GetCustomAttribute<ApiClientAttribute>() != null)
            .ToArray();

        RefitSettings refitSettings = new()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(
                new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
        };

        foreach (var apiClientInterface in apiClientInterfaces)
        {
            var attr = apiClientInterface.GetCustomAttribute<ApiClientAttribute>()!;
            var baseUrl = configuration[attr.BaseUrlConfigKey];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException(
                    $"Missing configuration value for '{attr.BaseUrlConfigKey}' (required by {apiClientInterface.Name}).");

            var httpClientBuilder = services.AddRefitClient(apiClientInterface, refitSettings)
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri(baseUrl);
                    httpClient.Timeout = TimeSpan.FromSeconds(attr.TimeoutSeconds);

                    if (!string.IsNullOrWhiteSpace(attr.UserAgent))
                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(attr.UserAgent);
                });

            foreach (var handlerType in attr.MessageHandlers)
            {
                if (!typeof(DelegatingHandler).IsAssignableFrom(handlerType))
                    throw new InvalidOperationException(
                        $"Message handler '{handlerType.FullName}' must inherit from DelegatingHandler.");
                
                //note: the AuthHeaderMiddleware is registered here as scoped
                services.TryAddScoped(handlerType);
                
                //note: the AuthHeaderMiddleware is added to the HttpClient pipeline here.
                httpClientBuilder.AddHttpMessageHandler(sp => (DelegatingHandler)sp.GetRequiredService(handlerType));
            }
        }
    }
}