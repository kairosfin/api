using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IStockRepository
{
    IAsyncEnumerable<Stock> GetStocks(
        IEnumerable<string> searchTerms, 
        CancellationToken ct);

    Task UpsertStocks(IEnumerable<Stock> stocks, CancellationToken ct);

    IAsyncEnumerable<Price> GetPrices(
        string ticker, 
        DateTime from,
        CancellationToken ct);

    Task AddPrices(IEnumerable<Price> prices, CancellationToken ct);
}
