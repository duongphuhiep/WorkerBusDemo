namespace Core.ExternalApiClient.Dtos;

public record ClientContextDtos
{
    public string? PlatformId { get; init; }
    public string? EnvironmentId { get; init; }
}