using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Identifier = Kairos.Shared.Contracts.Account.AccessAccount.AccountIdentifier;

namespace Kairos.Account.Business.UseCases;

internal sealed class SetPasswordUseCase(
    UserManager<Investor> identity,
    ILogger<SetPasswordUseCase> logger,
    AccountContext db
) : IRequestHandler<SetPasswordCommand, Output>
{
    public async Task<Output> Handle(SetPasswordCommand input, CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?> 
        { 
            ["AccountIdentifier"] = input.AccountIdentifier.Value, 
            ["CorrelationId"] = input.CorrelationId
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                var id = input.AccountIdentifier;

                var account = await db.Investors.FirstOrDefaultAsync(
                    i =>
                        (id.Type == Identifier.Document && i.Document == id.Value) ||
                        (id.Type == Identifier.Email && i.Email == id.Value) ||
                        (id.Type == Identifier.PhoneNumber && i.PhoneNumber == id.Value) ||
                        (id.Type == Identifier.AccountNumber && i.Id.ToString() == id.Value),
                    cancellationToken);

                if (account is null)
                {
                    logger.LogWarning("Account not found");
                    return Output.PolicyViolation([$"A conta com identificador #{input.AccountIdentifier.Value} não existe."]);
                }

                if (await identity.IsEmailConfirmedAsync(account) is false)
                {
                    logger.LogWarning("Attempted to set password for an unconfirmed email");
                    return Output.PolicyViolation(["O e-mail da conta precisa ser confirmado primeiro."]);
                }

                if (input.Pass != input.PassConfirmation)
                {
                    return Output.PolicyViolation(["A senha e sua confirmação não conferem."]);
                }

                var result = await identity.ResetPasswordAsync(
                    account, 
                    input.Token,
                    input.Pass);

                if (result.Succeeded is false)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    logger.LogWarning("Failed to set password. Errors: {@Errors}", errors);
                    return Output.PolicyViolation(errors);
                }

                logger.LogInformation("Password successfully defined");
                return Output.Ok(["Senha definida com sucesso!"]);                
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
}