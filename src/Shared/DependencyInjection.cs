using Microsoft.Extensions.DependencyInjection;

namespace Kairos.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        return services;
    }
}
