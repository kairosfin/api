using Kairos.Account;
using Kairos.Shared.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace Kairos.Gateway;

public static class DependencyInjection
{
    public static IServiceCollection AddGateway(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddInMemoryStorage();

        return services
            .AddEventBus(configuration)
            .AddEndpointsApiExplorer()
            .AddSwagger()
            .AddResponseCompression();
    }

    static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBusOptions>(configuration.GetSection("EventBus"));

        return services.AddMassTransit(x =>
        {
            x.AddConfigureEndpointsCallback((name, cfg) => cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))));

            x.AddAccountConsumers();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                EventBusOptions options = ctx.GetRequiredService<IOptions<EventBusOptions>>().Value;

                cfg.Host(options.HostAddress);

                cfg.UseRawJsonSerializer();

                ctx
                    .ConfigureAccountEndpoints(cfg)
                    .ConfigureEndpoints(ctx);
            });
        });
    }

    static IServiceCollection AddSwagger(this IServiceCollection services) =>
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen(o => o.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Kairos",
            Description = "Kairos Brokerage back-end services",
            Contact = new OpenApiContact
            {
                Name = "Kairos Dev Team",
                Email = "kairos.fintech@gmail.com",
            },
        }));
}
