using Microsoft.Extensions.DependencyInjection;

namespace Kairos.Account;

public static class DependencyInjection
{
    public static IServiceCollection AddAccount(this IServiceCollection services)
    {
        // services.AddScoped<IAccountService, AccountService>();
        return services;
    }
}
