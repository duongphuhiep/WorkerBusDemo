namespace Core.Model;

public record ClientContext
{
    public string? PlatformId { get; init; }
    public string? EnvironmentId { get; init; }
}