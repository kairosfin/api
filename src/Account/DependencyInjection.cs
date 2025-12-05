using System.Reflection;
using System.Text;
using Kairos.Account.Configuration;
using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Account.Infra.Consumers;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Kairos.Account;

public static class DependencyInjection
{
    public static IServiceCollection AddAccount(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services.Configure<Settings>(config);

        return services
            .AddIdentity(config)
            .AddMediatR(cfg =>
            {
                cfg.LicenseKey = config["Keys:MediatR"];
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            })
            .AddAuth();
    }

    public static IBusRegistrationConfigurator ConfigureAccountBus(this IBusRegistrationConfigurator x)
    {
        x.AddConsumers(Assembly.GetExecutingAssembly());
        x.AddEntityFrameworkOutbox<AccountContext>(c => 
        {
            c.UseSqlServer();
            c.UseBusOutbox();    
        });

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

    static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfigurationManager config)
    {
        services
            .AddDbContext<AccountContext>(o => o.UseSqlServer(config["Database:Broker:ConnectionString"]!))
            .AddIdentity<Investor, IdentityRole<long>>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 6;

                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.AllowedForNewUsers = true;

                o.SignIn.RequireConfirmedEmail = true;
                o.Tokens.EmailConfirmationTokenProvider = "Default";
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AccountContext>() 
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    static IServiceCollection AddAuth(this IServiceCollection services)
    {
        var jwt = services
            .BuildServiceProvider()
            .GetRequiredService<IOptions<Settings>>()
            .Value.Jwt;

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[jwt.CookieName];
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
