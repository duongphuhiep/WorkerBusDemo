namespace Core.ExternalApiClient.Dtos;

public record Product
{
    public string? BearerToken { get; init; }
    public ClientContextDtos? ClientContext { get; init; }
    public string? ProductId { get; init; }
}