using Azure.Identity;
using Kairos.Shared.Infra;
using Kairos.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
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
            .AddHealthChecking(config)
            .AddDbContext<BrokerContext>(o =>
                o.UseSqlServer(config["Database:Broker:ConnectionString"]!));

        builder.AddSeq(config);

        return services;
    }

    static IServiceCollection AddKeyVault(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        KeyVaultOptions keyVault = GetKeyVault(config);

        config.AddAzureKeyVault(
            new Uri(keyVault.Url),
            new DefaultAzureCredential());

        return services;
    }

    static IServiceCollection AddHealthChecking(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        var kv = GetKeyVault(config);

        services
            .AddHealthChecks()
            .AddAzureKeyVault(
                new Uri(kv.Url),
                credential: new DefaultAzureCredential(),
                o => o.AddSecret("Database--Broker--ConnectionString"),
                name: "key-vault",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["azure", "keyvault"])
            .AddUrlGroup(
                new Uri(config["Health:Seq:Url"]!),
                name: "seq",
                tags: ["observability", "logging"],
                failureStatus: HealthStatus.Degraded)
            .AddSqlServer(
                connectionString: config["Database:Broker:ConnectionString"]!,
                name: "sql-broker",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["azure", "mssql"]);

        return services;
    }

    static void AddSeq(this IHostBuilder builder, IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        builder.UseSerilog((ctx, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
            .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));
    }

    static KeyVaultOptions GetKeyVault(IConfigurationManager config) =>
        config
            .GetRequiredSection("KeyVault")
            .Get<KeyVaultOptions>()!;
}
