using Kairos.Account.Configuration;
using Kairos.Account.Infra.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Kairos.Account.Infra.HealthChecks;

public sealed class SendGridHealthCheck(
    IHttpClientFactory httpClientFactory, 
    IOptions<Settings> settings) : IHealthCheck
{
    readonly MailingOptions _options = settings.Value.Mailing;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return HealthCheckResult.Degraded("SendGrid API Key is not configured.");
        }

        try
        {
            using var client = httpClientFactory.CreateClient("SendGrid");

            var response = await client.GetAsync("v3/scopes", cancellationToken);

            if (response.IsSuccessStatusCode is false)
            {    
                return HealthCheckResult.Degraded($"SendGrid returned {response.StatusCode} status code.");
            }

            return HealthCheckResult.Healthy("SendGrid is responding and API key is valid.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Failed to connect to SendGrid API.", ex);
        }
    }
}