using System.ComponentModel;

namespace Kairos.Shared.Contracts.MarketData.GetStockQuotes;

public enum QuoteRange
{
    [Description("1d")]
    Day = 1,
    
    [Description("5d")]
    FiveDays,

    [Description("7d")]
    Week,

    [Description("1mo")]
    Month,

    [Description("3mo")]
    Quarter,

    [Description("6mo")]
    Semester,

    [Description("1y")]
    Year,

    [Description("2y")]
    TwoYears,

    [Description("5y")]
    FiveYears,

    [Description("10y")]
    Decade,

    [Description("ytd")]
    YearToDate,

    [Description("max")]
    Max
}
