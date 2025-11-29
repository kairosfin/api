namespace Kairos.MarketData.Infra.Dtos;

public sealed class QuoteResponse
{
    public required List<StockDetail> Results { get; init; }

    public required DateTime RequestedAt { get; init; }

    public required int Took { get; init; }
}