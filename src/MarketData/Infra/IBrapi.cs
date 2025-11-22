using Refit;

namespace Kairos.MarketData.Infra;

internal interface IBrapi
{
    [Get("/stocks")]
    public Task GetStocks();
}