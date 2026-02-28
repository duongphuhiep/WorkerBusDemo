using Core;
using MassTransit;
using Worker;

var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<MyWorker>();

builder.Services.AddLogging(cfg => cfg.AddConsole());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeploymentConsumer>();
    x.UsingAzureServiceBus((context, cfg) =>
    {
        string asbConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"] ?? "Missing ASB connection string";
        cfg.Host(asbConnectionString);
        cfg.ReceiveEndpoint("tuto1", e =>
        {
            e.ConfigureConsumeTopology = false;
            e.ConfigureConsumer<DeploymentConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();
