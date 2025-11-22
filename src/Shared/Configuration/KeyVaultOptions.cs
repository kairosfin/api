using System;

namespace Kairos.Shared.Configuration;

public sealed class KeyVaultOptions
{
    /// <summary>
    /// Only the vault URL is required when the app is running in Azure,
    /// as it can use Managed Identity for automatic authentication
    /// </summary>
    public required string Url { get; init; }

    /* 
     * The following props are required when running locally
     * and must be set via .NET Secret Manager
    */
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
