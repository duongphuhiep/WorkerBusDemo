using Core;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentController(ILogger<DeploymentController> logger, ISendEndpointProvider senderProvider) : ControllerBase
{
    [HttpPost("platform/{platformId}/environment/{environmentId}/async")]
    public async Task<string> DeployAsync(string platformId, string environmentId)
    {
        DeploymentRequest r = new DeploymentRequest {
            Organization = $"Org/{platformId}/{environmentId}",
        };
        var sender = await senderProvider.GetSendEndpoint(new Uri("queue:tuto1"));
        await sender.Send(r);
        return "Deployment started";
    }
    
    [HttpPost("platform/{platformId}/environment/{environmentId}")]
    public async Task<string> DeploySync(string platformId, string environmentId)
    {
        DeploymentRequest r = new DeploymentRequest {
            Organization = $"Org/{platformId}/{environmentId}",
        };
        return "Deployment finished";
    }
}