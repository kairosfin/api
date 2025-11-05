using System.Net.Security;
using Azure.Identity;
using Kairos.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Kairos.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(
        this IServiceCollection services,
        IConfigurationManager config,
        IHostBuilder builder)
    {
        services
            .AddKeyVault(config)
            .AddHealthChecking(config);

        builder.AddSeq(config);

        return services;
    }

    static IServiceCollection AddKeyVault(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        var keyVault = config
            .GetRequiredSection("KeyVault")
            .Get<KeyVaultOptions>()!;

        if (string.IsNullOrEmpty(keyVault.Url))
        {
            return services;
        }

        config.AddAzureKeyVault(
               new Uri(keyVault.Url),
               new DefaultAzureCredential()
           );

        return services;
    }

    static IServiceCollection AddHealthChecking(
        this IServiceCollection services,
        IConfiguration config)
    {
        services
            .AddHealthChecks()
            .AddUrlGroup(
                new Uri(config["Health:Seq:Url"]!),
                name: "seq",
                tags: ["logging"],
                failureStatus: HealthStatus.Degraded);

        return services;
    }

    static void AddSeq(this IHostBuilder builder, IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        builder.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
    }
}
