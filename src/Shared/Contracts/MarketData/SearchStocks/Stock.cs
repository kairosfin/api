namespace Kairos.Shared.Contracts.MarketData.SearchStocks;

public sealed record Stock(
    string Ticker,
    string Name,
    decimal Price,
    decimal DailyYield,
    decimal MarketCap,
    decimal TradeVolume,
    Uri Logo,
    string Sector
)
{
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
}