using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

public sealed class OpenAccountUseCase(
    ILogger<OpenAccountUseCase> logger,
    IBus bus
) : IRequestHandler<OpenAccount, Output>
{
    public async Task<Output> Handle(OpenAccount req, CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?>
        {
            ["CorrelationId"] = req.CorrelationId,
            ["Document"] = req.Document
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                logger.LogInformation("Starting account opening process");

                await Task.Delay(3000, cancellationToken);

                var accountId = new Random().Next(1000, 9999999);

                logger.LogInformation("Account {AccountId} created", accountId);

                AccountOpened @event = new(
                    Id: accountId,
                    FirstName: req.FirstName,
                    LastName: req.LastName,
                    Document: req.Document,
                    Email: req.Email,
                    Birthdate: req.Birthdate);

                await bus.Publish(
                    @event,
                    ctx => ctx.CorrelationId = req.CorrelationId,
                    cancellationToken);

                return Output.Ok([
                    $"Conta de investimento {accountId} aberta!",
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