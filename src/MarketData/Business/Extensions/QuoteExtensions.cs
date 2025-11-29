using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;

namespace Kairos.MarketData.Business.Extensions;

public static class QuoteExtensions
{
    internal static async IAsyncEnumerable<Quote> ToStreamedQuote(this IEnumerable<StockQuote> quotes)
    {
        foreach (var quote in quotes)
        {
            yield return new Quote(
                quote.Date, 
                quote.Close ?? 0, 
                quote.AdjustedClose ?? 0);
        }
    }

    internal static async IAsyncEnumerable<Quote> ToStreamedQuote(this IEnumerable<Price> prices)
    {
        foreach (var price in prices)
        {
            yield return new Quote(
                price.Date, 
                price.Value, 
                price.AdjustedValue);
        }
    }

    internal static IEnumerable<Price> ToPrices(
        this IEnumerable<StockQuote> quotes, 
        string ticker)
    {
        foreach (var quote in quotes)
        {
            yield return new Price(
                ticker, 
                quote.Date, 
                quote.Close ?? 0, 
                quote.AdjustedClose ?? 0);
        }
    }
}
