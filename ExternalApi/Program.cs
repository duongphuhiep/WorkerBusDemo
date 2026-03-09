using ExternalApi.Model;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using ToolsPack.String;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging(); // logging

//GetBearerToken
app.MapGet("bearerToken/{platformId}/{environmentId}",
    (string platformId, string environmentId, [FromHeader(Name = "user-agent")] string userAgent) =>
    {
        if (userAgent != "ExternalApiConnector/1.0")
        {
            throw new BadHttpRequestException("UserAgent is invalid");
        }

        if (platformId.StartsWith("root"))
        {
            throw new UnauthorizedAccessException("Platform not accessible");
        }

        return new BearerToken
        {
            Token = $"token.{platformId}.{environmentId}",
            ExpiryAt = DateTimeOffset.UtcNow.AddSeconds(10)
        };
    });

//CreatePendingProduct
app.MapPut("product", ([FromHeader(Name = "Authorization")] string bearerToken,
        [FromHeader(Name = "user-agent")] string userAgent,
        [FromBody] ClientContext clientContext,
        [FromServices] ILogger<Program> logger
    )
    =>
{
    if (userAgent != "ExternalApiService/2.0") throw new BadHttpRequestException("UserAgent is invalid");

    if (!bearerToken.EndsWith($"token.{clientContext.PlatformId}.{clientContext.EnvironmentId}"))
    {
        logger.LogWarning("BearerToken is invalid {BearerToken}", bearerToken);
        return Results.Unauthorized();
    }

    if (clientContext.EnvironmentId.StartsWith("readonly"))
    {
        throw new InvalidOperationException(
            $"Unable to create product for the environment {clientContext.EnvironmentId}");
    }

    logger.LogInformation("Success createPendingProduct {ClientContext}", clientContext);
    return Results.Ok(new Product
    {
        BearerToken = bearerToken,
        ClientContext = clientContext,
        ProductId = StringGenerator.CreateRandomString(3, 2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
    });
});

//GetPendingProduct
app.MapPost("queryPendingProduct/{productId}", ([FromHeader(Name = "Authorization")] string bearerToken,
    [FromHeader(Name = "user-agent")] string userAgent,
    [FromBody] ClientContext clientContext, string productId,
    [FromServices] ILogger<Program> logger) =>
{
    if (userAgent != "ExternalApiService/2.0")
    {
        logger.LogWarning("BearerToken is invalid {BearerToken}", bearerToken);
        throw new BadHttpRequestException("UserAgent is invalid");
    }

    if (!bearerToken.EndsWith($"token.{clientContext.PlatformId}.{clientContext.EnvironmentId}"))
        return Results.Unauthorized();

    logger.LogInformation("Success query product {ProductId}", productId);
    return Results.Ok(new Product
    {
        BearerToken = bearerToken,
        ClientContext = clientContext,
        ProductId = productId
    });
});

app.MapPost("deploy",
    ([FromHeader(Name = "Authorization")] string bearerToken,
        [FromHeader(Name = "user-agent")] string userAgent,
        [FromBody] DeploymentRequest deploymentRequest,
        [FromServices] ILogger<Program> logger) =>
    {
        if (userAgent != "ExternalApiService/2.0") throw new BadHttpRequestException("UserAgent is invalid");

        var clientContext = deploymentRequest.ClientContext ??
                            throw new BadHttpRequestException("ClientContext is null");
        if (!bearerToken.EndsWith($"token.{clientContext.PlatformId}.{clientContext.EnvironmentId}"))
        {
            logger.LogWarning("BearerToken is invalid {BearerToken}", bearerToken);
            return Results.Unauthorized();
        }

        var product = deploymentRequest.Product ?? throw new BadHttpRequestException("Product is null");
        if (bearerToken != product.BearerToken)
        {
            logger.LogWarning("BearerToken not match product info {BearerToken} != {ProductBearerToken}", bearerToken,
                product.BearerToken);
            return Results.Unauthorized();
        }

        if (product.ClientContext != clientContext)
            throw new BadHttpRequestException("ClientContext of product does not match ClientContext of request");

        logger.LogInformation("Success deployment {DeploymentRequest}", deploymentRequest);

        return Results.Ok(new DeploymentReport
        {
            BearerToken = bearerToken,
            ClientContext = clientContext,
            Product = product
        });
    });

await app.RunAsync();