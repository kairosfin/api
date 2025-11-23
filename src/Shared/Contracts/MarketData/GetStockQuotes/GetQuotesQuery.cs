using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.MarketData.GetStockQuotes;

public sealed record GetQuotesQuery(
    Guid CorrelationId,
    string Ticker
) : IQuery<IEnumerable<StockQuote>>
{
    public GetQuotesQuery(string ticker) : this(Guid.NewGuid(), ticker)
    {
    }
}