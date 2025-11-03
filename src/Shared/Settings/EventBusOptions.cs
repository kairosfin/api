namespace Kairos.Shared.Settings;

public sealed record EventBusOptions
{
    required public string HostAddress { get; init; } = string.Empty;
}
