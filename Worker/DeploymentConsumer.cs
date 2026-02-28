using Core;
using MassTransit;

namespace Worker;

public class DeploymentConsumer(ILogger<DeploymentConsumer> logger): IConsumer<DeploymentRequest>
{
    public Task Consume(ConsumeContext<DeploymentRequest> context)
    {
        logger.LogInformation("Processing deployment for organization: {Organization}", context.Message.Organization);
        return Task.CompletedTask;
    }
}