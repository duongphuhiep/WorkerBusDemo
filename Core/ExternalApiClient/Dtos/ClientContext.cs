namespace Core.ExternalApiClient.Dtos;

public record ClientContext
{
    public string? PlatformId { get; init; }
    public string? EnvironmentId { get; init; }
}