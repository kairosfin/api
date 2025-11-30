using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.Shared.Contracts.MarketData;

public sealed record SearchStocksQuery(
    Guid CorrelationId,
    IEnumerable<string> Query,
    int Page,
    int Limit
) : IQuery<IAsyncEnumerable<Stock>>
{
    public SearchStocksQuery(
        IEnumerable<string> searchTerms,
        int? page,
        int? limit) : this(
            Guid.NewGuid(), 
            searchTerms, 
            page ?? 1, 
            limit ?? 10)
    {
    }
}