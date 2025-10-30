using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kairos.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentity(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        return services;
    }
}
