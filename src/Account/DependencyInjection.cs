using System.Net.Security;
using Kairos.Account.Infra.Consumers;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Kairos.Account;

public static class DependencyInjection
{
    public static IServiceCollection AddAccount(this IServiceCollection services)
    {
        return services.AddMediatR(cfg =>
        {
            // TOOD: puxar do key vault
            cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzkzNjY0MDAwIiwiaWF0IjoiMTc2MjEzNzU3MCIsImFjY291bnRfaWQiOiIwMTlhNDc5NDcwZDI3ZmRkYWNlYWZiZmYxYWY3ZmU2YSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazkzc2F6ZmNoOWN2aGh3ZGdkankzenMwIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.loUqeZNlhh3F2cRqmNQPqLe0rPznU8_8xCRtJLpKQegpnByd_k0MezyUecL5b1TG4i8JOelB831isggOopeqcmin7qJ5XWXJxwUlbiP_OGwDQRfZCjx0IB4NKeYVGljPL2og1MK9GsxiIa4deHnzHSZm4imBP-QdqDQ_a5wEAG9h6JGttueQ0E-_sL8GvKrmPfvaMlqwaT0HxlYefUdNeux6Ej7u1wahnPcbSsbp1Toc-VDIRe_N1W9lgG1Vnf07CNihufErAYDIvOcSENJR2Zt33h9v2152ThB5opR-0IdOQ-YZhMmaKOvxf4pZiuCorNcLIu_QLk6rRcI2SrnNXA";
            cfg.RegisterServicesFromAssemblyContaining<SendOpenedAccountEmailConsumer>();
        });
    }

    public static void AddAccountConsumers(this IBusRegistrationConfigurator x)
    {
        x.AddConsumers(typeof(DependencyInjection).Assembly);
    }

    public static IRabbitMqBusFactoryConfigurator ConfigureAccountEndpoints(
        this IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<AccountOpened>(x => x.SetEntityName("account-opened"));

        cfg.ReceiveEndpoint("account.send-opened-account-confirmation-email", e =>
        {
            e.UseDelayedRedelivery(r => r.Intervals(
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(10)));

            e.ConfigureConsumer<SendOpenedAccountEmailConsumer>(context);
            e.Bind<AccountOpened>();
        });

        return cfg;
    }
}
