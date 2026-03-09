namespace Core.ExternalApiClient.Dtos;

public record DeploymentReport
{
    public string? BearerToken { get; init; }
    public ClientContextDtos? ClientContext { get; init; }
    public Product? Product { get; init; }
}