namespace Kairos.Shared.Configuration;

/// <summary>
/// Represents a config inside the appsettings Api section
/// </summary>
public sealed class ApiOptions
{
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Authentication token
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public required int Timeout { get; init; }

    public required ResilienceOptions Resilience { get; init; }
}