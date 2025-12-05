using Carter;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Response = Kairos.Gateway.Filters.Response<object>;

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
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError);
            
        app.MapPatch("/confirm-email", 
            ([FromBody] ConfirmEmailCommand command) =>
            _mediator.Send(command))
            .WithSummary("Account opening e-mail confirmation")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError);
    }
}