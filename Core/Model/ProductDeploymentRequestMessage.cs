namespace Core.Model;

public record ProductDeploymentRequestMessage
{
    public string? PlatformId { get; init; }
    public string? EnvironmentId { get; init; }
    public string? ProductId { get; init; }
}