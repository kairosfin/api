using Kairos.MarketData.Infra;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Kairos.Shared.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Output = Kairos.Shared.Contracts.Output<System.Collections.Generic.IAsyncEnumerable<Kairos.Shared.Contracts.MarketData.GetStockQuotes.Quote>>;

namespace Kairos.MarketData.Business.UseCases;

internal sealed class GetQuotesUseCase(IBrapi brapi, ILogger<GetQuotesUseCase> logger) 
    : IRequestHandler<GetQuotesQuery, Output>
{
    static readonly string[] _testTickers = [ "PETR4", "MGLU3", "VALE3", "ITUB4" ];
    static readonly QuoteRange[] _freeRanges = [
        QuoteRange.Day,
        QuoteRange.FiveDays,
        QuoteRange.Month,
        QuoteRange.Quarter
    ];

    public async Task<Output> Handle(
        GetQuotesQuery input, 
        CancellationToken cancellationToken)
    {
        try
        {
            QuoteResponse? quoteRes = await brapi.GetQuote(
                input.Ticker,
                GetValidRange(input).GetDescription());

            List<StockQuote> quotes = quoteRes.Results[0].HistoricalDataPrice;

            return quotes.Count switch
            {
                0 => Output.Empty,
                _ => Output.Ok(FormatQuotes(quotes))
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving quotes. Input: {@Input}", input);
            return Output.UnexpectedError([ex.Message]);
        }
    }

    static async IAsyncEnumerable<Quote> FormatQuotes(IEnumerable<StockQuote> quotes)
    {
        foreach (var quote in quotes)
        {
            yield return new Quote(
                quote.Date, 
                quote.Close, 
                quote.AdjustedClose);
        }
    }

    static QuoteRange GetValidRange(GetQuotesQuery input)
    {
        if (_freeRanges.Contains(input.Range))
        {
            return input.Range;
        }

        if (_testTickers.Contains(input.Ticker))
        {
            return input.Range;
        }

        return QuoteRange.Quarter;
    }
}
