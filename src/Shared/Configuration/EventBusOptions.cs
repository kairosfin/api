namespace Kairos.Shared.Configuration;

public sealed record EventBusOptions
{
    required public string HostAddress { get; init; } = string.Empty;
}
