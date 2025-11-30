using Kairos.MarketData.Business.Extensions;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.MarketData;
using Kairos.Shared.Contracts.MarketData.SearchStocks;
using MediatR;
using Microsoft.Extensions.Logging;
using Output = Kairos.Shared.Contracts.Output<System.Collections.Generic.IAsyncEnumerable<Kairos.Shared.Contracts.MarketData.SearchStocks.Stock>>;

namespace Kairos.MarketData.Business.UseCases;

internal sealed class SearchStocksUseCase(
    IBrapi brapi,
    IStockRepository stockRepo,
    ILogger<SearchStocksUseCase> logger
) : IRequestHandler<SearchStocksQuery, Output>
{
    public async Task<Output<IAsyncEnumerable<Stock>>> Handle(
        SearchStocksQuery input, 
        CancellationToken cancellationToken)
    {
        var terms = input.Search;

        try
        {
            var stocks = stockRepo.GetStocks(terms, cancellationToken);

            var isCached = await stocks
                .GetAsyncEnumerator(cancellationToken)
                .MoveNextAsync();

            if (isCached is false)
            {
                var res = await brapi.GetStocks();

                if (res.Stocks.Length == 0)
                {
                    return Output.Empty;
                }

                Task.Run(async () => await CacheStocks(res.Stocks));

                var filteredStocks = res.Stocks
                    .Stream()
                    .Where(s => terms.Any(t => 
                        s.Ticker.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        s.Sector.Contains(t, StringComparison.OrdinalIgnoreCase)));

                return Output.Ok(filteredStocks);
            }

            return Output.Ok(stocks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving stocks. Input: {@Input}", input);
            return Output.UnexpectedError([ex.Message]);
        }
    }

    async Task CacheStocks(StockSummary[] stocks)
    {
        const string method = nameof(CacheStocks);

        try
        {   
            logger.LogInformation(
                "[{Method}] Upserting stocks into cache. Quantity: {StockQuantity}",
                method, 
                stocks.Length);

            var stocksToCache = await stocks.Stream().ToArrayAsync();

            await stockRepo.UpsertStocks(stocksToCache, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogInformation(
                ex, 
                "[{Method}] An unexpected error occurred", 
                method);
        }
    }
}
