namespace Kairos.MarketData.Infra.Dtos;

public sealed class StockQuote
{
    // Date formatted as Unix Timestamp (e.g., 1756126800)
    public required long Date { get; init; }

    public required double Open { get; init; }

    public required double High { get; init; }

    public required double Low { get; init; }

    public required double Close { get; init; }

    public required long Volume { get; init; }

    public required double AdjustedClose { get; init; }
}