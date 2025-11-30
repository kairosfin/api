using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using MongoDB.Driver;

namespace Kairos.MarketData.Infra;

internal sealed class PriceRepository(IMongoDatabase db) : IPriceRepository
{
    readonly IMongoCollection<Price> _prices = db.GetCollection<Price>("Price");

    public async IAsyncEnumerable<Price> Get(
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

    public async Task Append(
        string ticker, 
        Price[] prices, 
        CancellationToken ct)
    {
        var maxDate = (await _prices
            .Find(p => p.Ticker == ticker)
            .SortByDescending(p => p.Date)
            .FirstOrDefaultAsync(ct))
            ?.Date ?? DateTime.MinValue;

        await _prices.InsertManyAsync(
            prices.Where(p => p.Date > maxDate),
            new InsertManyOptions() { IsOrdered = false },
            ct);
    }
}
