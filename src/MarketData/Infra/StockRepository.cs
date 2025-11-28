using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using MongoDB.Driver;

namespace Kairos.MarketData.Infra;

internal sealed class StockRepository(IMongoDatabase db) : IStockRepository
{
    readonly IMongoCollection<StockDetail> _stocks = db.GetCollection<StockDetail>("Stock");
    readonly IMongoCollection<Quote> _prices = db.GetCollection<Quote>("Price");
}
