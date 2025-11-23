using Kairos.MarketData.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using MediatR;

namespace Kairos.MarketData.Business.UseCases;

internal sealed class GetQuotesUseCase(
    IBrapi brapi
) : IRequestHandler<GetQuotesQuery, Output<IEnumerable<StockQuote>>>
{
    public async Task<Output<IEnumerable<StockQuote>>> Handle(
        GetQuotesQuery input, 
        CancellationToken cancellationToken)
    {
        try
        {
            QuoteResponse? quoteRes = await brapi.GetQuote(input.Ticker);

            List<StockQuote> quotes = quoteRes.Results[0].HistoricalDataPrice;

            return quotes.Count switch
            {
                0 => Output<IEnumerable<StockQuote>>.Empty,
                _ => Output<IEnumerable<StockQuote>>.Ok(quotes)
            };
        }
        catch (Exception ex)
        {
            return Output<IEnumerable<StockQuote>>.UnexpectedError([ex.Message]);
        }
    }
}
