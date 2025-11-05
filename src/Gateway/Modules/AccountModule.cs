using Carter;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Gateway.Modules;

public sealed class AccountModule : CarterModule
{
    public AccountModule() : base("/api/v1/account")
    {
        WithTags("Account");
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app
            .MapPost(
                "/open",
                async (IMediator mediator, [FromBody] OpenAccount command) =>
                {
                    Output? res = await mediator.Send(command);

                    if (res.IsFailure)
                    {
                        return Results.Json(
                            res,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }

                    return Results.Ok(res);
                })
                .WithDescription("Open an investment account");
    }
}