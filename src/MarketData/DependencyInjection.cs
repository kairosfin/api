using System.Reflection;
using System.Text.Json;
using Kairos.MarketData.Configuration;
using Kairos.MarketData.Infra;
using Kairos.Shared.Configuration;
using Kairos.Shared.Infra.HttpClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            .AddHealthCheck(api)
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
            })
        };

        services
            .AddRefitClient<IBrapi>(refitSettings)
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(brapi.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(brapi.Timeout);
            })
            .SetupHttpClient(brapi);

        return services;
    }

    static IServiceCollection AddHealthCheck(this IServiceCollection services, Settings.Api api)
    {
        var brapi = api.Brapi;

        // services
        //     .AddHttpClient(BrapiHealthCheck.HttpClientName, c =>
        //     {
        //         c.BaseAddress = new Uri(brapi.BaseUrl);
        //         c.Timeout = TimeSpan.FromSeconds(brapi.Timeout);
        //     })
        //     .SetupHttpClient(brapi);

        // services.AddHealthChecks().AddCheck<BrapiHealthCheck>("brapi");

        return services;
    }

    static IHttpClientBuilder SetupHttpClient(this IHttpClientBuilder builder, ApiOptions api) =>
        builder
            .AddHttpMessageHandler(() => new QueryParamHttpHandler("token", api.Token))
            .AddTransientHttpErrorPolicy(policyBuilder =>
            {
                IEnumerable<TimeSpan> jitteredDelays = Backoff.DecorrelatedJitterBackoffV2(
                    TimeSpan.FromSeconds(api.Resilience.MedianFirstRetryDelay),
                    api.Resilience.RetryCount
                );

                return policyBuilder.WaitAndRetryAsync(jitteredDelays);
            });
}
