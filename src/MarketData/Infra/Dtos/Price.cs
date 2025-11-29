namespace Kairos.MarketData.Infra.Dtos;

internal sealed record Price(
    string Ticker,
    DateTime Date,
    decimal Value,
    decimal AdjustedValue  
);