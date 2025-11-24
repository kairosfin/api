namespace Kairos.MarketData.Infra.Dtos;

public sealed partial class QuoteResponse
{
    public required List<StockDetail> Results { get; init; }

    public required DateTime RequestedAt { get; init; }

    public required string Took { get; init; }
}