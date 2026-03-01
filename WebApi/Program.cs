using Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration, []);
builder.Services.AddCoreService(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.MapDefaultEndpoints();
app.MapControllers();
await app.RunAsync();