namespace Kairos.MarketData.Infra.Dtos;

public sealed class StockQuote
{
    // Date formatted as Unix Timestamp (e.g., 1756126800)
    public required long Date { get; init; }

    public required decimal? Open { get; init; }

    public required decimal? High { get; init; }

    public required decimal? Low { get; init; }

    public required decimal? Close { get; init; }

    public required ulong? Volume { get; init; }

    public required decimal? AdjustedClose { get; init; }
}