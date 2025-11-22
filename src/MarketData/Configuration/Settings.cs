using Kairos.Shared.Configuration;

namespace Kairos.MarketData.Configuration;

public static partial class Settings
{
    public sealed partial class Api
    {
        public required ApiOptions Brapi { get; init; }
    }
}