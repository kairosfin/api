using Carter;
using Kairos.Shared.Contracts.MarketData;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Gateway.Modules;

public sealed class MarketDataModule : CarterModule
{
    readonly IMediator _mediator;

    public MarketDataModule(IMediator mediator) : base("/api/v1/stocks")
    {
        WithTags("MarketData");

        _mediator = mediator;
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app
            .MapGet(
                "/", 
                ([FromQuery] string[] search) => _mediator.Send(new GetStocksQuery(search)))
                .WithDescription("Get basic information about the specified stock(s)");

        app.MapGet(
            "/{ticker}/quote",
            (
                IMediator mediator,
                [FromRoute] string ticker,
                [FromQuery] QuoteRange? range = null) => 
                _mediator.Send(new GetQuotesQuery(ticker, range)))
            .WithDescription("Get a stock's historical quotes.\n\n Tickers for test: PETR4, MGLU3, VALE3 and ITUB4 ");
    }
}