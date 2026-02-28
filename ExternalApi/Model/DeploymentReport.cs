namespace ExternalApi.Model;

public record DeploymentReport
{
    public string? BearerToken { get; init; }
    public ClientContext? ClientContext { get; init; }
    public Product? Product { get; init; }
}