using Core;
using Serilog;
using ToolsPack.String;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration,
    [typeof(DeploymentConsumer)]);
builder.Services.AddCoreService(builder.Configuration);
var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Worker started... try to log a very long message that will be truncated: {LongMessage}", StringGenerator.CreateRandomString(300)+"_hiep");

await host.RunAsync();