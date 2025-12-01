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
        int page,
        int pageSize,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var minDate = DateTime.UtcNow.Add(-_cacheTtl);

        var cacheTtlFilter = Builders<Stock>.Filter.Gte(s => s.UpdatedAt, minDate);

        var tickerNameOrSectorFilter = Builders<Stock>.Filter.Or(
            searchTerms.Select(term =>
            {
                var regex = new BsonRegularExpression(term, "i");
                return Builders<Stock>.Filter.Or(
                    Builders<Stock>.Filter.Regex(stock => stock.Ticker, regex),
                    Builders<Stock>.Filter.Regex(stock => stock.Name, regex),
                    Builders<Stock>.Filter.Regex(stock => stock.Sector, regex)
                );
            }));

        var stocks = _stocks
            .Find(Builders<Stock>.Filter.And(cacheTtlFilter, tickerNameOrSectorFilter))
            // .SortByDescending(s => s.DailyYield)
            .Skip(pageSize * (page - 1))
            .Limit(pageSize)
            .Project<Stock>(Builders<Stock>.Projection.Exclude("_id"))
            .ToAsyncEnumerable();

        await foreach (var stock in stocks) yield return stock;
    }

    public Task Upsert(Stock[] stocks, CancellationToken ct) =>
        _stocks.BulkWriteAsync(
            stocks.Select(stock =>
            {
                var filter = Builders<Stock>.Filter.Eq(s => s.Ticker, stock.Ticker);
                
                return new ReplaceOneModel<Stock>(filter, stock) { IsUpsert = true };
            }), 
            new BulkWriteOptions { IsOrdered = false }, 
            ct);
}
