using Core.Db;
using Core.ExternalApiClient;
using Core.ExternalApiClient.Dtos;
using Core.Model;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core;

public class DeploymentHandler(
    ILogger<DeploymentHandler> logger,
    ClientContextProvider clientContextProvider,
    IExternalApi externalApi,
    ISendEndpointProvider senderProvider,
    IOptions<AzureServiceBusConfig> azureServiceBusConfig,
    NorthwindDbContext dbContext
    )
{
    public async Task<DeploymentReport> DeploySync(string platformId, string environmentId, string? productId)
    {
        logger.LogInformation("Deploying Async {platformId} {environmentId}", platformId, environmentId);
        await clientContextProvider.SetClientContextAsync(platformId, environmentId);

        var currentClientContext = clientContextProvider.CurrentClientContext;
        var clientContextDto = new ClientContextDtos
        {
            PlatformId = currentClientContext.PlatformId,
            EnvironmentId = currentClientContext.EnvironmentId
        };
        
        await dbContext.Employees.FirstAsync();
        
        logger.LogInformation("currentClientContext Sync {platformId} {environmentId}", currentClientContext.PlatformId, currentClientContext.EnvironmentId);
        
        var product =
            string.IsNullOrEmpty(productId)
                ? await externalApi.CreatePendingProduct(clientContextDto)
                : await externalApi.GetPendingProduct(productId, clientContextDto);
        var deploymentReport = await externalApi.DeployProduct(new DeploymentRequest
        {
            ClientContext = clientContextDto,
            Product = product
        });

        await dbContext.Categories.FirstAsync();
        
        //make sure everything matches together to confirm that the target architecture worked
        ValidateDeploymentReport(deploymentReport);
        
        logger.LogInformation("Deployment success {report}", deploymentReport);
        return deploymentReport;
    }

    public async Task DeployAsync(string platformId, string environmentId)
    {
        logger.LogInformation("Deploying Async {platformId} {environmentId}", platformId, environmentId);
        await clientContextProvider.SetClientContextAsync(platformId, environmentId);

        var currentClientContext = clientContextProvider.CurrentClientContext;
        var clientContextDto = new ClientContextDtos
        {
            PlatformId = currentClientContext.PlatformId,
            EnvironmentId = currentClientContext.EnvironmentId
        };

        logger.LogInformation("currentClientContext Async {platformId} {environmentId}", currentClientContext.PlatformId, currentClientContext.EnvironmentId);
        
        var product = await externalApi.CreatePendingProduct(clientContextDto);

        var r = new ProductDeploymentRequestMessage
        {
            PlatformId = platformId,
            EnvironmentId = environmentId,
            ProductId = product.ProductId
        };
        var sender =
            await senderProvider.GetSendEndpoint(new Uri($"queue:{azureServiceBusConfig.Value.QueueName}"));
        await sender.Send(r);
        logger.LogInformation("Deployment request message sent {message}", r);
    }

    private static void ValidateDeploymentReport(DeploymentReport report)
    {
        var bearerToken = report.BearerToken ?? throw new ArgumentNullException(nameof(report.BearerToken));
        var clientContext = report.ClientContext ?? throw new ArgumentNullException(nameof(report.ClientContext));
        var product = report.Product ?? throw new ArgumentNullException(nameof(report.Product));
        if (!bearerToken.EndsWith(
                $"token.{clientContext.PlatformId}.{clientContext.EnvironmentId}"))
            throw new InvalidOperationException("bearerToken not match ClientContext");
        if (bearerToken != product.BearerToken) throw new InvalidOperationException("bearerToken not match Product");
        if (product.ClientContext != clientContext)
            throw new InvalidOperationException("Product's ClientContext not match report's ClientContext");
    }
}