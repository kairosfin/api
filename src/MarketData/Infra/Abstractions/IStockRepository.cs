using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IStockRepository
{
    IAsyncEnumerable<Stock> GetByTickerOrNameOrSector(
        IEnumerable<string> searchTerms, 
        int page,
        int pageSize,
        CancellationToken ct);

    Task Upsert(Stock[] stocks, CancellationToken ct);
}
