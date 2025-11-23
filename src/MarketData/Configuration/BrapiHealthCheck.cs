using Kairos.MarketData.Infra;
using Kairos.Shared.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Kairos.MarketData.Configuration;

internal sealed class BrapiHealthCheck(
    IOptions<Settings.Api> api,
    IHttpClientFactory clientFactory) : IHealthCheck
{
    readonly ApiOptions _brapi = api.Value.Brapi;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: receber interface refit
            using var client = clientFactory.CreateClient(typeof(IBrapi).FullName);

            HttpResponseMessage? res = await client.GetAsync(_brapi.HealthCheckPath, cancellationToken);

            string result = $"Status Code {res.StatusCode}";

            return res.IsSuccessStatusCode switch
            {
                false => HealthCheckResult.Unhealthy(result),
                _ => HealthCheckResult.Healthy(result)
            };
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
