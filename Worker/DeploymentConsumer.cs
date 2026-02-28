using Core;
using Core.Model;
using MassTransit;

namespace Worker;

public class DeploymentConsumer(ILogger<DeploymentConsumer> logger, DeploymentHandler deploymentHandler)
    : IConsumer<ProductDeploymentRequestMessage>
{
    public async Task Consume(ConsumeContext<ProductDeploymentRequestMessage> context)
    {
        logger.LogInformation("Processing deployment {message}", context.Message);
        var message = context.Message;

        var report = await deploymentHandler.DeploySync(
            message.PlatformId ?? throw new InvalidOperationException("PlatformId is null"),
            message.EnvironmentId ?? throw new InvalidOperationException("EnvironmentId is null"),
            message.ProductId);
    }
}