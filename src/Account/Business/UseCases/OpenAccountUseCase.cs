using System.Data.Common;
using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

internal sealed class OpenAccountUseCase(
    ILogger<OpenAccountUseCase> logger,
    IBus bus,
    UserManager<Investor> identity,
    AccountContext db
) : IRequestHandler<OpenAccountCommand, Output>
{
    public async Task<Output> Handle(
        OpenAccountCommand req,
        CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?>
        {
            ["CorrelationId"] = req.CorrelationId,
            ["Email"] = req.Email,
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                logger.LogInformation("Starting account opening process");

                var alreadyExists = await db.Investors
                    .FirstOrDefaultAsync(
                        i => 
                            i.Email == req.Email ||
                            i.PhoneNumber == req.PhoneNumber ||
                            i.Document == req.Document, 
                        cancellationToken);

                // TODO: também validar se existe conta com o mesmo phoneNumber or document
                Investor? existingAccount = await identity.FindByEmailAsync(req.Email);

                if (existingAccount is not null)
                {
                    logger.LogWarning("Email already in use.");
                    return Output.PolicyViolation(["O e-mail fornecido já está em uso."]);
                }

                var openAccountResult = Investor.OpenAccount(
                    req.Name,
                    req.Document,
                    req.PhoneNumber,
                    req.Email,
                    req.Birthdate,
                    req.AcceptTerms
                );

                if (openAccountResult.IsFailure)
                {
                    return new Output(openAccountResult);
                }

                Investor investor = openAccountResult.Value!;

                var identityResult = await identity.CreateAsync(investor);

                if (identityResult.Succeeded is false)
                {
                    var errors = identityResult.Errors.Select(e => e.Description).ToList();

                    logger.LogWarning("Account opening failed: {@Errors}", errors);

                    return Output.PolicyViolation(errors);
                }

                var token = await identity.GenerateEmailConfirmationTokenAsync(investor);

                logger.LogInformation("Account {AccountId} opened!", investor.Id);

                await bus.Publish(
                    new AccountOpened(
                        investor.Id,
                        req.Name,
                        req.PhoneNumber,
                        req.Document,
                        req.Email,
                        req.Birthdate,
                        Uri.EscapeDataString(token),
                        req.CorrelationId),
                    ctx => ctx.CorrelationId = req.CorrelationId,
                    cancellationToken);

                return Output.Created([
                    $"Conta de investimento {investor.Id} aberta!",
                    "Confirme a abertura no e-mail que será enviado em instantes."]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred");
                return Output.UnexpectedError(["Algum erro inesperado ocorreu... tente novamente mais tarde."]);
            }
        }
    }
}