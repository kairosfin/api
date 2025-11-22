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
    public const string HttpClientName = "brapi-hc";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = clientFactory.CreateClient(HttpClientName);

            HttpResponseMessage? res = await client.GetAsync(_brapi.HealthCheckPath, cancellationToken);

            string result = $"Status Code {res.StatusCode}";

            if (res.IsSuccessStatusCode is false)
            {
                return HealthCheckResult.Unhealthy(result);
            }

            return HealthCheckResult.Healthy(result);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
