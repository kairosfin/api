using Kairos.Shared.Configuration;

namespace Kairos.MarketData.Configuration;

internal partial class Settings
{
    public sealed partial class Api
    {
        public required ApiOptions Brapi { get; init; }
    }

    public sealed partial class Database
    {
        public required DbOptions MarketData { get; init; }
    }

    public sealed partial class Database
    {
        public required DbOptions MarketData { get; init; }
    }
}