namespace Core.ExternalApiClient.Dtos;

public record BearerToken
{
    public string Token { get; init; } = "";
    public DateTimeOffset ExpiryAt { get; set; } = DateTimeOffset.UtcNow.AddSeconds(10);
}