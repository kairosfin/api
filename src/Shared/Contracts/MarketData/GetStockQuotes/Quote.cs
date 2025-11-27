namespace Kairos.Shared.Contracts.MarketData.GetStockQuotes;

public sealed record Quote(
    DateTime Date,
    double Close,
    double CloseWithEvents
)
{
    public Quote(long unixTimeSeconds, double close, double adjustedClose) : this(
        DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).Date,
        close,
        adjustedClose
    ) { }
}