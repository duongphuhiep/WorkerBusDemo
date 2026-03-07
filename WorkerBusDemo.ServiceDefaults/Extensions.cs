using Core;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ToolsPack.Config;
using WorkerBusDemo.ServiceDefaults;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;
using MassTransit.Logging;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        builder.Services.AddHttpLogging(cfg => cfg.CombineLogs = true);
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
            .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
            .CreateLogger();
        builder.Services.AddSerilog();
        builder.Services.AddLogging();
        
        return builder;
    }

    private static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName, DiagnosticHeaders.DefaultListenerName)
                    .AddAspNetCoreInstrumentation(instrumentationOptions =>
                        // Exclude health check requests from tracing
                        instrumentationOptions.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                    //.AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();
        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter) builder.Services.AddOpenTelemetry().UseOtlpExporter();

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        builder.Services.Configure<AzureServiceBusConfig>(
            builder.Configuration.GetSection("AzureServiceBus")
        );

        return builder;
    }

    private static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath);

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static void RegisterConfiguration(this ConfigurationManager configuration, IHostEnvironment environment,
        string[] args)
    {
        configuration
            .AddJsonFile(ConfigFileLocator.Find("appsettings.shared.json"), false, true)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);
        if (environment.IsDevelopment()) configuration.AddUserSecrets<SecretReference>(true);
        configuration.AddEnvironmentVariables();
        configuration.AddCommandLine(args);
    }

    public static void AddAzureServiceBusMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        Type[] consumerTypes
    )
    {
        services.AddMassTransit(x =>
        {
            x.UsingAzureServiceBus((context, cfg) =>
            {
                var azureBusConfig =
                    configuration.GetRequiredSection("AzureServiceBus").Get<AzureServiceBusConfig>()
                    ?? throw new MissingFieldException(
                        "AzureServiceBus configuration section is missing or invalid");
                cfg.Host(azureBusConfig.ConnectionString);
                if (consumerTypes.Length > 0)
                    cfg.ReceiveEndpoint(azureBusConfig.QueueName, e =>
                    {
                        e.ConfigureConsumeTopology = false;
                        foreach (var consumerType in consumerTypes) e.ConfigureConsumer(context, consumerType);
                    });
            });
            foreach (var consumerType in consumerTypes) x.AddConsumer(consumerType);
        });
    }
}