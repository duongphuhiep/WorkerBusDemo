using Core;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration,
    [typeof(DeploymentConsumer)]);
builder.Services.AddCoreService(builder.Configuration);
var host = builder.Build();
host.Run();