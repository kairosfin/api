using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Dtos;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IStockRepository
{
    IAsyncEnumerable<Price> GetPrices(
        string ticker, 
        DateTime from,
        DateTime to,
        [EnumeratorCancellation] CancellationToken ct);

    Task AddPrices(IEnumerable<Price> prices, CancellationToken ct);
}
