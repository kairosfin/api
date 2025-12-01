using Carter;
using Kairos.Gateway.Filters;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kairos.Gateway.Modules;

public sealed class AccountModule : CarterModule
{
    readonly IMediator _mediator;

    public AccountModule(IMediator mediator) : base("/api/v1/account")
    {
        WithTags("Account");

        _mediator = mediator;
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/open", 
            ([FromBody] OpenAccountCommand command) =>
            _mediator.Send(command))
            .WithSummary("Open an investment account")
            .Produces<Response<object>>(StatusCodes.Status201Created)
            .Produces<Response<object>>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response<object>>(StatusCodes.Status400BadRequest)
            .Produces<Response<object>>(StatusCodes.Status500InternalServerError);
    }
}