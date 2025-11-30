using System.Text.Json.Serialization;

namespace Kairos.MarketData.Infra.Dtos;

public sealed record StockSearchResponse(StockSummary[] Stocks);