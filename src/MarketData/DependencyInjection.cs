using System.Reflection;
using System.Text.Json;
using Kairos.MarketData.Configuration;
using Kairos.MarketData.Infra;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.Shared.Infra.HttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;

namespace Kairos.MarketData;

public static class DependencyInjection
{
    public static async Task<IServiceCollection> AddMarketData(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services.Configure<Settings.Api>(config.GetSection("Api"));
        
        var api = services.BuildServiceProvider()
            .GetRequiredService<IOptions<Settings.Api>>()
            .Value;

        services.AddDatabase(config).Wait();

        return services
            .AddApiClients(api)
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

    async static Task<IServiceCollection> AddDatabase(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services.Configure<Settings.Database>(config.GetSection("Database"));

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var settings = services.BuildServiceProvider()
                .GetRequiredService<IOptions<Settings.Database>>()
                .Value;
            var connString = settings.MarketData.ConnectionString;

            var marketDataDb = MongoUrl.Create(connString).DatabaseName;
            
            return new MongoClient(connString).GetDatabase(marketDataDb);
        });

        var db = services
            .BuildServiceProvider()
            .GetRequiredService<IMongoDatabase>();

        const string priceCollection = "Price";

        var collections = await db.ListCollectionsAsync(new ListCollectionsOptions 
        { 
            Filter = new BsonDocument("name", priceCollection) 
        });

        if (await collections.AnyAsync() is false)
        {
            var options = new CreateCollectionOptions
            {
                TimeSeriesOptions = new TimeSeriesOptions(
                    timeField: "Date", 
                    metaField: "Ticker", 
                    granularity: TimeSeriesGranularity.Seconds)
            };

            await db.CreateCollectionAsync(priceCollection, options);
        }

        return services
            .AddSingleton<IStockRepository, StockRepository>();
    }
}
