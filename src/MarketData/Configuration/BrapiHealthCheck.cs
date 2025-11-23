using Kairos.MarketData.Infra;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kairos.MarketData.Configuration;

internal sealed class BrapiHealthCheck(IBrapi brapi) : IHealthCheck
{
    static readonly SemaphoreSlim _lock = new(1, 1);
    static readonly TimeSpan _cacheTtl = TimeSpan.FromHours(1);
    static HealthCheckResult? _cachedResult;
    static DateTimeOffset _cachedAt = DateTimeOffset.MinValue;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        // Caching mechanism used to avoid making too many requests to brapi.dev
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedResult is not null && (DateTimeOffset.UtcNow - _cachedAt) < _cacheTtl)
            {
                return (HealthCheckResult)_cachedResult;
            }

            _cachedAt = DateTimeOffset.UtcNow;
            _cachedResult = await CheckBrapiHealth();

            return (HealthCheckResult)_cachedResult;
        }
        finally
        {
            _lock.Release();
        }
    }

    async Task<HealthCheckResult> CheckBrapiHealth()
    {
        try
        {
            QuoteResponse? res = await brapi.GetQuote("FIQE3", "1d");

            var stock = res.Results[0];

            return stock switch
            {
                { RegularMarketPrice: > 0 } => HealthCheckResult.Healthy($"Brapi returned {stock.Currency} {stock.RegularMarketPrice} for {stock.Symbol}"),
                _ => HealthCheckResult.Unhealthy("Brapi is not returning the expected quote data")
            };
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
