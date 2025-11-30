namespace Kairos.Shared.Contracts.MarketData.SearchStocks;

public sealed record Stock(
    string Ticker,
    string Name,
    decimal Price,
    double DailyYield,
    decimal MarketCap,
    Uri Logo,
    string Sector
);