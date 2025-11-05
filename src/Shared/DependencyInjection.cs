using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace Kairos.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        services
            .AddHealthChecks()
            .AddUrlGroup(
                new Uri(config["Health:Seq:Url"]!),
                name: "seq",
                tags: ["logging"],
                failureStatus: HealthStatus.Degraded);

        return services;
    }
}
