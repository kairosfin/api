using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.MarketData;

public sealed record GetAssetsQuery(
    Guid CorrelationId,
    IEnumerable<string> SearchTerms
) : IQuery<int>
{
    public GetAssetsQuery(IEnumerable<string> searchTerms) : this(Guid.NewGuid(), searchTerms)
    {
    }
}