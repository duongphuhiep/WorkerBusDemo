using Core;
using Serilog;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration,
    [typeof(DeploymentConsumer)]);
builder.Services.AddCoreService(builder.Configuration);
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Worker starting up...");

//var config = host.Services.GetRequiredService<IConfiguration>();
// var asbConn = config.GetValue<string>("AzureServiceBus:ConnectionString");
// if (string.IsNullOrEmpty(asbConn)) throw new InvalidOperationException("AzureServiceBus:ConnectionString is not set");
// var appinsConn = config.GetValue<string>("Serilog:WriteTo:1:Args:connectionString");
// if (string.IsNullOrEmpty(appinsConn)) throw new InvalidOperationException("AppInsight:ConnectionString is not set");
// Console.WriteLine($"Worker running {asbConn}, {appinsConn}");

await host.RunAsync();