namespace Kairos.MarketData.Infra.Dtos;

internal sealed record Price(
    string Ticker,
    DateTime Date,
    decimal Value,
    decimal AdjustedValue,
    DateTime CreatedAt
)
{
    public Price(string ticker, long unixTimeSeconds, decimal value, decimal adjustedValue) : this(
        ticker,
        DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).Date,
        value,
        adjustedValue,
        DateTime.UtcNow
    ) { }   
}