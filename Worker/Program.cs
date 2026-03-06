using Core;
using Serilog;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration,
    [typeof(DeploymentConsumer)]);
builder.Services.AddCoreService(builder.Configuration);
builder.Services.AddSerilog();
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Worker starting up...");
await host.RunAsync();