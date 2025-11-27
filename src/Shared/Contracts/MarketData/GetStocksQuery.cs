using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.MarketData;

public sealed record GetStocksQuery(
    Guid CorrelationId,
    IEnumerable<string> SearchTerms
) : IQuery<int>
{
    public GetStocksQuery(IEnumerable<string> searchTerms) : this(Guid.NewGuid(), searchTerms)
    {
    }
}