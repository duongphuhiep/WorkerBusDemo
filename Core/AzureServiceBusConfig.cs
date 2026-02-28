namespace Core;

public record AzureServiceBusConfig
{
    public string ConnectionString { get; init; } = "";
    public string QueueName { get; init; } = "";
}