using Polly.Contrib.WaitAndRetry;

namespace Kairos.Shared.Configuration;

/// <summary>
/// Main parameters of <see cref="Backoff.DecorrelatedJitterBackoffV2" />
/// </summary>
public sealed class ResilienceOptions
{
    public required double MedianFirstRetryDelay { get; init; }
    public required int RetryCount { get; init; }
}