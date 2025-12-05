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

                Investor? existingAccount = await db.Investors
                    .FirstOrDefaultAsync(
                        i =>
                            i.Email == req.Email ||
                            i.PhoneNumber == req.PhoneNumber ||
                            i.Document == req.Document,
                        cancellationToken);

                if (existingAccount is not null)
                {
                    logger.LogWarning("Account identifier(s) already taken.");
                    return Output.PolicyViolation(["O e-mail, telefone e/ou documento já está(ão) em uso."]);
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

                await RaiseEvent(req, investor, cancellationToken);

                logger.LogInformation("Account {AccountId} opened!", investor.Id);

                return Output.Created([
                    $"Conta de investimento #{investor.Id} aberta!",
                    "Confirme a abertura no e-mail que será enviado em instantes."]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred");
                return Output.UnexpectedError([
                    "Algum erro inesperado ocorreu... tente novamente mais tarde.",
                    ex.Message]);
            }
        }
    }

    async Task RaiseEvent(
        OpenAccountCommand req, 
        Investor investor, 
        CancellationToken cancellationToken)
    {
        var emailConfirmationToken = string.Empty;
        var passwordResetToken = string.Empty;

        await Task.WhenAll(
            Task.Run(async () => emailConfirmationToken = await identity.GenerateEmailConfirmationTokenAsync(investor)),
            Task.Run(async () => passwordResetToken = await identity.GeneratePasswordResetTokenAsync(investor)));

        await bus.Publish(
            new AccountOpened(
                investor.Id,
                req.Name,
                req.PhoneNumber,
                req.Document,
                req.Email,
                req.Birthdate,
                emailConfirmationToken,
                passwordResetToken,
                req.CorrelationId),
            ctx => ctx.CorrelationId = req.CorrelationId,
            cancellationToken);
    }
}