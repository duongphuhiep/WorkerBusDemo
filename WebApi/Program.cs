using Core;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.RegisterConfiguration(builder.Environment, args);
builder.AddServiceDefaults();
builder.Services.AddAzureServiceBusMassTransit(builder.Configuration, []);
builder.Services.AddCoreService(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.MapDefaultEndpoints();

// log incoming request and response time
app.UseSerilogRequestLogging();

app.MapControllers();
await app.RunAsync();