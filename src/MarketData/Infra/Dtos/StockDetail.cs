using System.Text.Json.Serialization;

namespace Kairos.MarketData.Infra.Dtos;

public sealed class StockDetail
{
    public required string Currency { get; init; }

    public required long MarketCap { get; init; }

    public required string ShortName { get; init; }

    public required string LongName { get; init; }

    public required double RegularMarketChange { get; init; }

    public required double RegularMarketChangePercent { get; init; }

    public required DateTime RegularMarketTime { get; init; }

    public required double RegularMarketPrice { get; init; }

    public required double RegularMarketDayHigh { get; init; }

    public required string RegularMarketDayRange { get; init; }

    public required double RegularMarketDayLow { get; init; }

    public required long RegularMarketVolume { get; init; }

    public required double RegularMarketPreviousClose { get; init; }

    public required double RegularMarketOpen { get; init; }

    public required string FiftyTwoWeekRange { get; init; }

    public required double FiftyTwoWeekLow { get; init; }

    public required double FiftyTwoWeekHigh { get; init; }

    public required string Symbol { get; init; }

    [JsonPropertyName("logourl")]
    public required string LogoUrl { get; init; }

    public required string UsedInterval { get; init; }

    public required string UsedRange { get; init; }

    public required List<StockQuote> HistoricalDataPrice { get; init; } = new();

    public required List<string> ValidRanges { get; init; } = new();

    public required List<string> ValidIntervals { get; init; } = new();

    public required double PriceEarnings { get; init; }

    public required double EarningsPerShare { get; init; }
}
