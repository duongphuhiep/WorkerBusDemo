using Core;
using Core.ExternalApiClient;
using Core.ExternalApiClient.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestAsyncLocalController(
    ILogger<TestAsyncLocalController> logger,
    ClientContextProvider clientContextProvider,
    IExternalApi api)
    : ControllerBase
{
    [HttpPost($"{nameof(TaskDotRun)}/{{environmentId}}")]
    public async Task<string> TaskDotRun(string environmentId)
    {
        await clientContextProvider.SetClientContextAsync(nameof(TaskDotRun), environmentId);
        await Task.Run(()=>Run(nameof(TaskDotRun),1, environmentId));
        return "OK";
    }
    [HttpPost($"{nameof(FireAndForget)}/{{environmentId}}")]
    public async Task<string> FireAndForget(string environmentId)
    {
        await clientContextProvider.SetClientContextAsync(nameof(FireAndForget), environmentId);
        await Task.Run(()=>Run(nameof(FireAndForget),1, environmentId));
        return "OK";
    }
    
    [HttpPost($"{nameof(WhenAll)}/{{environmentId}}")]
    public async Task<string> WhenAll(string environmentId)
    {
        await clientContextProvider.SetClientContextAsync(nameof(WhenAll), environmentId);
        await Task.WhenAll(
            Run(nameof(WhenAll),1, environmentId),
            Run(nameof(WhenAll),2, environmentId),
            Run(nameof(WhenAll),3, environmentId)
        );
        return "OK";
    }

    private async Task Run(string scenario, int count, string environmentId)
    {
        await Task.Delay(3000);
        var currentClientContext = clientContextProvider.CurrentClientContext;
        var expectedClientContext = new ClientContextDtos
        {
            EnvironmentId = environmentId,
            PlatformId = scenario
        };
        logger.LogInformation("{Scenario} {Count}: Check if the AuthHeaderMiddleware can see the ClientContext. Current= {ClientContext}, expected={ExpectedClientContext}", scenario, count, currentClientContext, expectedClientContext);
        try
        {
            await api.CreatePendingProduct(expectedClientContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed {Scenario} {Count}: Check if the AuthHeaderMiddleware can see the ClientContext. Current= {ClientContext}, expected={ExpectedClientContext}", scenario, count, currentClientContext, expectedClientContext);
        }
    }
}