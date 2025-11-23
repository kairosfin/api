using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Refit;

namespace Kairos.MarketData.Infra;

internal interface IBrapi
{
    [Get("/quote/{ticker}")]
    public Task<QuoteResponse> GetQuote(
        string ticker, 
        [Query] string range = "3mo", 
        [Query] string interval = "1d");
}