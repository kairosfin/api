using Kairos.MarketData.Business.Extensions;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Kairos.Shared.Extensions;
using MassTransit.Internals;
using MediatR;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using Output = Kairos.Shared.Contracts.Output<System.Collections.Generic.IAsyncEnumerable<Kairos.Shared.Contracts.MarketData.GetStockQuotes.Quote>>;

namespace Kairos.MarketData.Business.UseCases;

internal sealed class GetQuotesUseCase(
    IBrapi brapi, 
    ILogger<GetQuotesUseCase> logger,
    IStockRepository repo) 
    : IRequestHandler<GetQuotesQuery, Output>
{
    public async Task<Output> Handle(
        GetQuotesQuery input, 
        CancellationToken cancellationToken)
    {
        try
        {
            var range = input.Range.GetCompatibleRange(input.Ticker);

            var prices = await repo
                .GetPrices(input.Ticker, range.GetMinDate(), cancellationToken)
                .ToListAsync();

            var historicalPriceUpToDate = prices
                .Any(p => p.Date >= DateTime.Today.AddDays(-3));

            if (historicalPriceUpToDate is false)
            {
                return await GetUpToDatePrices(input, range);
            }

            return Output.Ok(prices.ToStreamedQuote());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving quotes. Input: {@Input}", input);
            return Output.UnexpectedError([ex.Message]);
        }
    }

    async Task<Output> GetUpToDatePrices(GetQuotesQuery input, QuoteRange range)
    {
        QuoteResponse? quoteRes = await brapi.GetQuote(
            input.Ticker,
            QuoteRange.Max.GetCompatibleRange(input.Ticker).GetDescription());

        List<StockQuote> quotes = quoteRes.Results[0].HistoricalDataPrice;

        Task.Run(async () => await SyncPriceData(quotes, input.Ticker));

        var pricesInsideRange = quotes
            .ToStreamedQuote()
            .Where(p => p.Date >= range.GetMinDate());

        return Output.Ok(pricesInsideRange);
    }

    async Task SyncPriceData(List<StockQuote> quotes, string ticker)
    {
        const string method = nameof(SyncPriceData);

        if (quotes.Count == 0)
        {
            return;
        }

        try
        {   
            logger.LogInformation(
                "[{Method}] Synchronizing {Ticker} prices from {FromDate} to {ToDate}",
                method, 
                ticker,
                quotes.MinBy(q => q.Date)!.Date,
                quotes.MaxBy(q => q.Date)!.Date);

            await repo.AddPrices(quotes.ToPrices(ticker), CancellationToken.None);     
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