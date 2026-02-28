using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Ensure shared settings load first, with app/environment overriding.
builder.Configuration.Sources.Clear();
var sharedSettingsPath = "./appsettings.shared.json";
builder.Configuration
    .AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);


builder.Services.AddMassTransit(x =>
{
    x.UsingAzureServiceBus((context, cfg) =>
    {
        string asbConnectionString = builder.Configuration["AzureServiceBus:ConnectionString"] ?? "Missing ASB connection string";
        cfg.Host(asbConnectionString);
    });
});

builder.Services.AddOpenApi();

builder.Services.AddLogging(cfg => cfg.AddConsole());
builder.Services.AddHttpLogging(cfg => cfg.CombineLogs = true);
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();