using Serilog.Core;
using Serilog.Events;

namespace WorkerBusDemo.ServiceDefaults;

public class TruncatingEnricher(int maxLength = 100) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var key in logEvent.Properties.Keys.ToList())
        {
            if (logEvent.Properties[key] is ScalarValue { Value: string s } && s.Length > maxLength)
            {
                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty(key, s[..maxLength] + "…")
                );
            }
        }
    }
}