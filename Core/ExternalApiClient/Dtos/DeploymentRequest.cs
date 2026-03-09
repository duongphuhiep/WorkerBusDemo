namespace Core.ExternalApiClient.Dtos;

public record DeploymentRequest
{
    public ClientContextDtos? ClientContext { get; init; }
    public Product? Product { get; init; }
}