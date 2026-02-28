using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Core;

public static class SharedSetup
{
    public static void RegisterConfiguration(ConfigurationManager configuration,  IHostEnvironment environment)
    {
        // var sharedSettingsPath = "./appsettings.shared.json";
        // configuration
        //     .AddJsonFile(sharedSettingsPath, optional: true, reloadOnChange: true)
        //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //     .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
        // if (environment.IsDevelopment())
        // {
        //     configuration.AddUserSecrets<Program>(optional: true);
        // }
        // configuration.AddEnvironmentVariables();
        // configuration.AddCommandLine(args);
        
    }
    public static void RegisterCoreServices(IServiceCollection services)
    {
        // Register any shared services or interfaces here
    }
}