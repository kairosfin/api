using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.SearchStocks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kairos.MarketData.Infra;

internal sealed class StockRepository(IMongoDatabase db) : IStockRepository
{
    readonly IMongoCollection<Stock> _stocks = db.GetCollection<Stock>("Stock");
    readonly IMongoCollection<Price> _prices = db.GetCollection<Price>("Price");

    public async IAsyncEnumerable<Stock> GetStocks(
        IEnumerable<string> searchTerms,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var filters = searchTerms.Select(term =>
        {
            var regex = new BsonRegularExpression(term, "i");
            return Builders<Stock>.Filter.Or(
                Builders<Stock>.Filter.Regex(stock => stock.Ticker, regex),
                Builders<Stock>.Filter.Regex(stock => stock.Name, regex),
                Builders<Stock>.Filter.Regex(stock => stock.Sector, regex)
            );
        });

        using var stocks = await _stocks.FindAsync(
            Builders<Stock>.Filter.Or(filters), 
            new FindOptions<Stock>
            { 
                Projection = Builders<Stock>.Projection.Exclude("_id") 
            },
            ct);

        while (await stocks.MoveNextAsync(cancellationToken: ct))
        {
            foreach (var stock in stocks.Current)
            {
                yield return stock;
            }
        }
    }

    public Task UpsertStocks(IEnumerable<Stock> stocks, CancellationToken ct) =>
        _stocks.InsertManyAsync(
            stocks, 
            new InsertManyOptions() { IsOrdered = false }, 
            ct);

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

    public Task AddPrices(IEnumerable<Price> prices, CancellationToken ct) =>
        _prices.InsertManyAsync(
            prices, 
            new InsertManyOptions() { IsOrdered = false }, 
            ct);
}
