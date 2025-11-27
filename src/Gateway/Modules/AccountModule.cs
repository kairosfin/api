using Carter;
using Kairos.Shared.Contracts;
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
        app
            .MapPost(
                "/open", 
                ([FromBody] OpenAccount command) => _mediator.Send(command))
                .WithDescription("Open an investment account");
    }
}