using System.Globalization;
using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.MarketData.GetStockQuotes;

public sealed record GetQuotesQuery(
    Guid CorrelationId,
    string Ticker,
    QuoteRange Range
) : IQuery<IAsyncEnumerable<Quote>>
{
    public GetQuotesQuery(string ticker, QuoteRange? range = null)
        : this(
            Guid.NewGuid(), 
            ticker.ToUpper(CultureInfo.InvariantCulture), 
            range ?? QuoteRange.FiveDays) { }
}