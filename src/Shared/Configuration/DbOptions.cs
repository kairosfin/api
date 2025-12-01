namespace Kairos.Shared.Configuration;

public class DbOptions
{
    public required string ConnectionString { get; init; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int Timeout { get; init; }

    public ResilienceOptions? Resilience { get; init; }
}
