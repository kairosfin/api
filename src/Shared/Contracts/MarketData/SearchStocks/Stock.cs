namespace Kairos.Shared.Contracts.MarketData.SearchStocks;

public sealed record Stock(
    string Ticker,
    string Name,
    decimal Price,
    double DailyYield,
    decimal MarketCap,
    decimal TradeVolume,
    Uri Logo,
    string Sector
);