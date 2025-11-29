using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using MongoDB.Driver;

namespace Kairos.MarketData.Infra;

internal sealed class StockRepository(IMongoDatabase db) : IStockRepository
{
    readonly IMongoCollection<StockDetail> _stocks = db.GetCollection<StockDetail>("Stock");
    readonly IMongoCollection<Price> _prices = db.GetCollection<Price>("Price");

    public async IAsyncEnumerable<Price> GetPrices(
        string ticker, 
        DateTime from,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var filter = Builders<Price>.Filter.And(
            Builders<Price>.Filter.Eq(x => x.Ticker, ticker),
            Builders<Price>.Filter.Gte(x => x.Date, from),
            Builders<Price>.Filter.Lte(x => x.Date, DateTime.Today)
        );

        var options = new FindOptions<Price>
        { 
            Projection = Builders<Price>.Projection.Exclude("_id") 
        };

        using var prices = await _prices.FindAsync(filter, options, ct);

        while (await prices.MoveNextAsync(cancellationToken: ct))
        {
            foreach (var price in prices.Current)
            {
                yield return price;
            }
        }
    }

    public Task AddPrices(IEnumerable<Price> prices, CancellationToken ct)
    {
        return _prices.InsertManyAsync(
            prices, 
            new InsertManyOptions() { IsOrdered = false }, 
            ct);
    }
}
