using System.Reflection;
using Kairos.Account.Infra.Consumers;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kairos.Account;

public static class DependencyInjection
{
    public static IServiceCollection AddAccount(
        this IServiceCollection services,
        IConfiguration config)
    {
        return services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = config["Keys:MediatR"];
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
    }

    public static IBusRegistrationConfigurator AddAccountConsumers(this IBusRegistrationConfigurator x)
    {
        x.AddConsumers(Assembly.GetExecutingAssembly());

        return x;
    }

    public static IRabbitMqBusFactoryConfigurator ConfigureAccountEndpoints(
        this IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<AccountOpened>(x => x.SetEntityName("account-opened"));

        cfg.ReceiveEndpoint("account.send-account-opened-email", e =>
        {
            e.UseDelayedRedelivery(r => r.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(10)));

            e.ConfigureConsumer<SendAccountOpenedEmailConsumer>(context);
            e.Bind<AccountOpened>();
        });

        return cfg;
    }
}
