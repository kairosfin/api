using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IStockRepository
{
    IAsyncEnumerable<Stock> Get(
        IEnumerable<string> searchTerms, 
        CancellationToken ct);

    Task Upsert(Stock[] stocks, CancellationToken ct);
}
