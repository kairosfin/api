using System.Security.Claims;
using Carter;
using Kairos.Account.Configuration;
using Kairos.Gateway.Filters;
using Kairos.Gateway.Modules.Account.Request;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Contracts.Account.GetAccountInfo;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(e =>
            {
                e.Responses["201"].Description = "The account was opened successfully and is pending email confirmation.";
                e.Responses["422"].Description = "Policy violation, such as the user being underage or not accepting the terms.";
                e.Responses["400"].Description = "Invalid input, such as a malformed e-mail or document number.";
                e.Responses["500"].Description = "An unexpected server error occurred.";
                return e;
            });
            
        app.MapPatch("/{id}/confirm-email", 
            ([FromRoute] long id, [FromBody] ConfirmEmailRequest req) =>
            _mediator.Send(new ConfirmEmailCommand(
                id, 
                req.ConfirmationToken, 
                Guid.NewGuid())))
            .WithSummary("Account opening e-mail confirmation")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(e =>
            {
                e.Responses["200"].Description = "E-mail successfully confirmed.";
                e.Responses["422"].Description = "Policy violation, e.g., expired token or the account does not exist.";
                e.Responses["400"].Description = "Invalid input, such as an invalid account number";
                e.Responses["500"].Description = "An unexpected server error occurred.";
                return e;
            });

        app.MapPatch("/{id}/set-password",
            ([FromRoute] long id, [FromBody] SetPasswordRequest req) =>
            _mediator.Send(new SetPasswordCommand(
                id, 
                req.Pass, 
                req.PassConfirmation, 
                req.Token,
                Guid.NewGuid())))
            .WithSummary("Defines or resets an account's password")
            .WithDescription("A valid password reset token, which comes from e-mail, must be provided.")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(e =>
            {
                e.Responses["200"].Description = "Password successfully (re)defined.";
                e.Responses["422"].Description = "Policy violation, e.g., the e-mail is not confirmed.";
                e.Responses["400"].Description = "Invalid input, such as missing the pass confirmation.";
                e.Responses["500"].Description = "An unexpected server error occurred.";
                return e;
            });

        app.MapPost("/access",
            async (
                [FromBody] AccessAccountCommand command, 
                HttpContext ctx, 
                IOptions<Settings> accountSettings) =>
            {
                Output<string> output = await _mediator.Send(command);

                if (output.IsFailure)
                {
                    return output;
                }

                var jwt = accountSettings.Value.Jwt;

                ctx.Response.Cookies.Append(jwt.CookieName, output.Value!, new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes)
                });

                return output;
            })
            .WithSummary("Access an account")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status422UnprocessableEntity)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(e =>
            {
                e.Responses["200"].Description = "Authentication successful. The JWT is in the 'data' field.";
                e.Responses["422"].Description = "Policy violation, e.g., invalid credentials, locked out, or unconfirmed e-mail.";
                e.Responses["400"].Description = "Invalid input, such as a malformed e-mail.";
                e.Responses["500"].Description = "An unexpected server error occurred.";
                return e;
            });

        app.MapGet("/me", 
            async (HttpContext ctx) =>
            {
                var accountIdValue = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (long.TryParse(accountIdValue, out var accountId) is false)
                {
                    return Output.CredentialsRequired(["Acesse sua conta para visualizar os dados cadastrais."]);
                }
                
                var output = await _mediator.Send(new GetAccountInfoQuery(accountId));

                return output;
            })
            .RequireAuthorization()
            .WithSummary("Get the authenticated account's data")
            .Produces<Response<AccountInfo>>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status401Unauthorized)
            .Produces<Response>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(e =>
            {
                e.Responses["200"].Description = "Returns the account details for the authenticated user.";
                e.Responses["401"].Description = "Unauthorized if the auth cookie is missing or invalid.";
                e.Responses["500"].Description = "An unexpected server error occurred.";
                return e;
            });

        app.MapDelete("/exit",
            async (
                HttpContext ctx, 
                IOptions<Settings> accountSettings) =>
            {
                ctx.Response.Cookies.Delete(accountSettings.Value.Jwt.CookieName);

                return Output.Empty;
            })
            .WithSummary("Sign out")
            .Produces(StatusCodes.Status204NoContent)
            .WithOpenApi(e =>
            {
                e.Responses["204"].Description = "Logged out successfully.";
                return e;
            });
    }
}