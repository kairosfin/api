using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.Shared.Contracts.MarketData;

public sealed record SearchStocksQuery(
    Guid CorrelationId,
    IEnumerable<string> Search
) : IQuery<IAsyncEnumerable<Stock>>
{
    public SearchStocksQuery(IEnumerable<string> searchTerms) : this(Guid.NewGuid(), searchTerms)
    {
    }
}