using Carter;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.MarketData;
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
            .MapGet("/stocks", async (IMediator mediator, [FromQuery(Name = "q")] string[] searchTerms) =>
                {
                    GetAssetsQuery query = new(searchTerms);

                    Output? res = await mediator.Send(query);

                    if (res.IsFailure)
                    {
                        return Results.Json(
                            res,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }

                    return Results.Ok(res);
                })
                .WithDescription("Get basic information about specific stocks");
    }
}