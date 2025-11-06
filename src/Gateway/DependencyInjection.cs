using Carter;
using Kairos.Account;
using Kairos.Shared.Settings;
using MassTransit;
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
            .AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(30);
                options.AddHealthCheckEndpoint("Kairos", "/_health");
            })
            .AddInMemoryStorage();

        services.AddCarter();

        return services
            .AddEventBus(configuration)
            .AddEndpointsApiExplorer()
            .AddSwagger()
            .AddResponseCompression();
    }

    static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBusOptions>(configuration.GetSection("EventBus"));

        return services.AddMassTransit(bus =>
        {
            bus
                .AddAccountConsumers()
                .ConfigureHealthCheckOptions(o => o.Name = "rabbitmq")
                .AddConfigureEndpointsCallback((name, cfg) => cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5))));

            bus.UsingRabbitMq((ctx, cfg) =>
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
