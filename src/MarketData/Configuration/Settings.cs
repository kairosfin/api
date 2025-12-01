using Kairos.Shared.Configuration;

namespace Kairos.MarketData.Configuration;

internal static partial class Settings
{
    public sealed partial class Api
    {
        public required ApiOptions Brapi { get; init; }
    }

    public sealed partial class Database
    {
        public required DbOptions MarketData { get; init; }
    }
}