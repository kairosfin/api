using System.Reflection;
using System.Text.Json;
using Kairos.MarketData.Configuration;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.Shared.Infra.HttpClient;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;

namespace Kairos.MarketData;

public static class DependencyInjection
{
    public static IServiceCollection AddMarketData(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services.Configure<Settings.Api>(config.GetSection("Api"));
        
        var api = services.BuildServiceProvider()
            .GetRequiredService<IOptions<Settings.Api>>()
            .Value;

        return services
            .AddApiClients(api)
            .AddDatabase(config)
            .AddHealthCheck()
            .AddMediatR(cfg =>
            {
                cfg.LicenseKey = config["Keys:MediatR"];
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });
    }

    static IServiceCollection AddApiClients(this IServiceCollection services, Settings.Api api)
    {
        var brapi = api.Brapi;

        var refitSettings = new RefitSettings()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase      
            })
        };

        services
            .AddRefitClient<IBrapi>(refitSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(brapi.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(brapi.Timeout);
            })
            .AddHttpMessageHandler(() => new QueryParamHttpHandler("token", brapi.Token))
            .AddTransientHttpErrorPolicy(policyBuilder =>
            {
                IEnumerable<TimeSpan> jitteredDelays = Backoff.DecorrelatedJitterBackoffV2(
                    TimeSpan.FromSeconds(brapi.Resilience.MedianFirstRetryDelay),
                    brapi.Resilience.RetryCount
                );

                return policyBuilder.WaitAndRetryAsync(jitteredDelays);
            });

        return services;
    }

    static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<BrapiHealthCheck>("brapi")
            .AddMongoDb(
                dbFactory: sp => sp.GetRequiredService<IMongoDatabase>(),
                tags: ["db", "mongo"],
                failureStatus: HealthStatus.Unhealthy,
                name: "mongodb"
            );

        return services;
    }

    static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services.Configure<Settings.Database>(config.GetSection("Database"));

        return services.AddSingleton<IMongoDatabase>(sp =>
        {
            var connString = sp.GetRequiredService<IOptions<Settings.Database>>()
                .Value.MarketData
                .ConnectionString;

            var marketDataDb = MongoUrl.Create(connString).DatabaseName;
            
            return new MongoClient(connString).GetDatabase(marketDataDb);
        });
    }
}
