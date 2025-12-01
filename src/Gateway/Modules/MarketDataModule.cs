using Carter;
using Kairos.Gateway.Filters;
using Kairos.Shared.Contracts.MarketData;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Kairos.Shared.Contracts.MarketData.SearchStocks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

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
            .MapGet("/search", (
                [FromQuery] string[] q,
                [FromQuery] int? page = null,
                [FromQuery] int? limit = null) => 
                _mediator.Send(new SearchStocksQuery(q, page, limit)))
            .WithSummary("Search for stocks")
            .WithDescription("Searches for stocks by ticker, name or sector based on a list of search terms.")
            .WithOpenApi(operation =>
            {
                var query = operation.Parameters.First(p => p.Name == "q");
                query.Description = "A list of terms to search for. Can be partial tickers, names or sectors.";
                query.Required = true;
                query.Example = new OpenApiArray { new OpenApiString("banco"), new OpenApiString("xpbr31") };
                return operation;
            })
            .Produces<Response<IAsyncEnumerable<Stock>>>(StatusCodes.Status200OK)
            .Produces<Response<object>>(StatusCodes.Status404NotFound)
            .Produces<Response<object>>(StatusCodes.Status500InternalServerError);

        app.MapGet("/{ticker}/quote", (
                [FromRoute] string ticker,
                [FromQuery] QuoteRange? range = null) => 
                _mediator.Send(new GetQuotesQuery(ticker, range)))
            .WithSummary("Get historical quotes")
            .WithDescription("Tickers for test: PETR4, MGLU3, VALE3 and ITUB4 ")
            .Produces<Response<IAsyncEnumerable<Quote>>>(StatusCodes.Status200OK)
            .Produces<Response<object>>(StatusCodes.Status500InternalServerError);
    }
}