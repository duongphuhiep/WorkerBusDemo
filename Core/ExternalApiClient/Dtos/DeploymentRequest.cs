namespace Core.ExternalApiClient.Dtos;

public record DeploymentRequest
{
    public ClientContext? ClientContext { get; init; }
    public Product? Product { get; init; }
}