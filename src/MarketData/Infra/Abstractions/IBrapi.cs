using Kairos.MarketData.Infra.Dtos;
using Refit;

namespace Kairos.MarketData.Infra.Abstractions;

/// <summary>
/// Open API reference for brapi.dev: https://brapi.dev/swagger/latest.json
/// </summary>
internal interface IBrapi
{
    [Get("/quote/{ticker}")]
    public Task<QuoteResponse> GetQuote(
        string ticker, 
        [Query] string range = "3mo", 
        [Query] string interval = "1d");

    [Get("/quote/list")]
    public Task<StockSearchResponse> GetStocks(
        [Query] string sortBy = "change", 
        [Query] string sortOrder = "desc");
}