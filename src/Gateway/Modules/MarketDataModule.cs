using Carter;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.MarketData;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Gateway.Modules;

public sealed class MarketDataModule : CarterModule
{
    public MarketDataModule() : base("/api/v1/market-data")
    {
        WithTags("MarketData");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app
            .MapGet("/stocks", async (IMediator mediator, [FromQuery] string[] search) =>
                {
                    Output? res = await mediator.Send(new GetStocksQuery(search));

                    if (res.IsFailure)
                    {
                        return Results.Json(
                            res,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }

                    return Results.Ok(res);
                })
                .WithDescription("Get basic information about the specified stock(s)");

        app.MapGet(
            "/stocks/{ticker}/quote", 
            async (HttpContext http, IMediator mediator, [FromRoute] string ticker) =>
            {
                var res = await mediator.Send(new GetQuotesQuery(ticker));

                if (res.IsFailure)
                {
                    return Results.Json(
                        res,
                        statusCode: StatusCodes.Status500InternalServerError);
                }

                return Results.Ok(res);
            })
            .WithDescription("Get a stock's historical prices");
    }
}