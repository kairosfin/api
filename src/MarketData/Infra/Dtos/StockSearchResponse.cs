using System.Text.Json.Serialization;

namespace Kairos.MarketData.Infra.Dtos;

public sealed record StockSearchResponse(StockSummary[] Stocks);

public sealed class StockSummary
{
    public required string Stock { get; init; }
    public required string Name { get; init; }
    public decimal Close { get; init; }
    public double Change { get; init; }
    public decimal Volume { get; init; }
    [JsonPropertyName("market_cap")]
    public ulong MarketCap { get; init; }
    public required string Logo { get; init; }
    public required string Sector { get; init; }
};