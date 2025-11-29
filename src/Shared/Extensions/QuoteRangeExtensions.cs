using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using MassTransit;

namespace Kairos.Shared.Extensions;

public static class QuoteRangeExtensions
{
    static readonly string[] _testTickers = [ "PETR4", "MGLU3", "VALE3", "ITUB4" ];
    static readonly QuoteRange[] _freeRanges = [
        QuoteRange.Day,
        QuoteRange.FiveDays,
        QuoteRange.Month,
        QuoteRange.Quarter
    ];

    /// <summary>
    /// Gets a brapi.dev compatible range for a given ticker
    /// </summary>
    /// <remarks>
    /// If ticker is PETR4, MGLU3, VALE3, ITUB4, any range is compatible.
    /// </remarks>
    /// <param name="range"></param>
    /// <param name="ticker"></param>
    /// <returns></returns>
    public static QuoteRange GetCompatibleRange(
        this QuoteRange range, 
        string ticker)
    {
        if (_freeRanges.Contains(range))
        {
            return range;
        }

        if (_testTickers.Contains(ticker))
        {
            return range;
        }

        QuoteRange compatibleRange = _freeRanges
            .FirstOrDefault(r => r.ToNumber() >= range.ToNumber());

        return compatibleRange is 0 ? QuoteRange.Quarter : compatibleRange;
    }

    /// <summary>
    /// Gets the amount of days inside the given range.
    /// </summary>
    /// <remarks>e.g, QuoteRange.Week has 7 days</remarks>
    /// <param name="range"></param>
    /// <returns></returns>
    public static int ToNumber(this QuoteRange range)
    {
        const int month = 30;
        const int year = month * 12;

        return range switch
        {
            QuoteRange.Day => 1,
            QuoteRange.FiveDays => 5,
            QuoteRange.Week => 7,
            QuoteRange.Month => month,
            QuoteRange.Quarter => month * 3,
            QuoteRange.Semester => month * 6,
            QuoteRange.Year => year,
            QuoteRange.TwoYears => year * 2,
            QuoteRange.FiveYears => year * 5,
            QuoteRange.Decade => year * 10,
            QuoteRange.YearToDate => DateTime.Today.DayOfYear,
            _ => int.MaxValue
        };
    }

    /// <summary>
    /// Gets the range start date.
    /// </summary>
    /// <remarks>e.g., if today is january 10th and QuoteRange.Week, then the min date is january 3rd (10 - 7)</remarks>
    /// <param name="range"></param>
    /// <returns></returns>
    public static DateTime GetMinDate(this QuoteRange range) =>
        DateTime.Today
            .AddDays(-ToNumber(range))
            .Date;
}
