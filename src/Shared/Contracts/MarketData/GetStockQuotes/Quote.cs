namespace Kairos.Shared.Contracts.MarketData.GetStockQuotes;

public sealed record Quote(
    DateTime Date,
    decimal Close,
    decimal CloseWithEvents
)
{
    public Quote(long unixTimeSeconds, decimal close, decimal adjustedClose) : this(
        DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).Date,
        close,
        adjustedClose
    ) { }
}