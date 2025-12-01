using System;
using System.Text.Json.Serialization;

namespace Kairos.MarketData.Infra.Dtos;

public sealed class StockSummary
{
    public required string Stock { get; init; }
    public required string Name { get; init; }
    public decimal Close { get; init; }
    public decimal Change { get; init; }
    public decimal? Volume { get; init; }
    [JsonPropertyName("market_cap")]
    public decimal? MarketCap { get; init; }
    public required string Logo { get; init; }
    public string? Sector { get; init; }
};