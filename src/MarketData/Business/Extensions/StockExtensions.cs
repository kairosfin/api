using System;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.SearchStocks;

namespace Kairos.MarketData.Business.Extensions;

public static class StockExtensions
{
    internal static async IAsyncEnumerable<Stock> Stream(
        this IEnumerable<StockSummary> stocks)
    {
        foreach (var stock in stocks)
        {
            yield return new Stock(
                stock.Stock,
                stock.Name,
                stock.Close,
                stock.Change,
                stock.MarketCap ?? 0,
                stock.Volume ?? 0,
                new Uri(stock.Logo),
                stock.Sector ?? string.Empty
            );
        }
    }
}
