namespace Core.ExternalApiClient.Dtos;

public record Product
{
    public string? BearerToken { get; init; }
    public ClientContext? ClientContext { get; init; }
    public string? ProductId { get; init; }
}