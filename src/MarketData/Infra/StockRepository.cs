using System.Runtime.CompilerServices;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.Shared.Contracts.MarketData.SearchStocks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kairos.MarketData.Infra;

internal sealed class StockRepository(IMongoDatabase db) : IStockRepository
{
    readonly IMongoCollection<Stock> _stocks = db.GetCollection<Stock>("Stock");
    readonly static TimeSpan _cacheTtl = TimeSpan.FromHours(1);

    public async IAsyncEnumerable<Stock> GetByTickerOrNameOrSector(
        IEnumerable<string> searchTerms,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var minDate = DateTime.UtcNow.Add(-_cacheTtl);

        var filters = searchTerms.Select(term =>
        {
            var regex = new BsonRegularExpression(term, "i");
            return Builders<Stock>.Filter.And(
                Builders<Stock>.Filter.Gte(s => s.UpdatedAt, minDate),
                Builders<Stock>.Filter.Or(
                    Builders<Stock>.Filter.Regex(stock => stock.Ticker, regex),
                    Builders<Stock>.Filter.Regex(stock => stock.Name, regex),
                    Builders<Stock>.Filter.Regex(stock => stock.Sector, regex)
                )
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

    public Task Upsert(Stock[] stocks, CancellationToken ct)
    {
        var writes = stocks.Select(stock =>
        {
            var filter = Builders<Stock>.Filter.Eq(s => s.Ticker, stock.Ticker);
            
            return new ReplaceOneModel<Stock>(filter, stock) { IsUpsert = true };
        });

        return _stocks.BulkWriteAsync(
            writes, 
            new BulkWriteOptions { IsOrdered = false }, 
            ct);
    }
}
