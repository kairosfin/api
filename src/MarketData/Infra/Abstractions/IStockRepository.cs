using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IStockRepository
{
    IAsyncEnumerable<Price> GetPrices(
        string ticker, 
        DateTime from,
        [EnumeratorCancellation] CancellationToken ct);

    Task AddPrices(IEnumerable<Price> prices, CancellationToken ct);
}
