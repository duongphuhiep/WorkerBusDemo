using Core;
using Core.ExternalApiClient.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DeploymentController(
    ILogger<DeploymentController> logger,
    DeploymentHandler deploymentHandler)
    : ControllerBase
{
    [HttpPost("platform/{platformId}/environment/{environmentId}/sync")]
    public async Task<DeploymentReport> DeploySync(string platformId, string environmentId)
    {
        logger.LogInformation("Deploying (sync) platform {platformId} environment {environmentId}", platformId,
            environmentId);
        var report = await deploymentHandler.DeploySync(platformId, environmentId, null);
        logger.LogInformation("Deployment finished: {report}", report);
        return report;
    }

    [HttpPost("platform/{platformId}/environment/{environmentId}/async")]
    public async Task<string> DeployAsync(string platformId, string environmentId)
    {
        logger.LogInformation("Deploying (async) platform {platformId} environment {environmentId}", platformId,
            environmentId);
        await deploymentHandler.DeployAsync(platformId, environmentId);
        return "Deployment started";
    }
}