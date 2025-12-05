using Kairos.Account.Domain;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

internal sealed class SetPasswordUseCase(
    UserManager<Investor> identity,
    ILogger<SetPasswordUseCase> logger
) : IRequestHandler<SetPasswordCommand, Output>
{
    public async Task<Output> Handle(SetPasswordCommand input, CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?> 
        { 
            ["AccountId"] = input.AccountId, ["CorrelationId"] = input.CorrelationId
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                var account = await identity.FindByIdAsync(input.AccountId.ToString());

                if (account is null)
                {
                    logger.LogWarning("Account not found");
                    return Output.PolicyViolation([$"A conta #{input.AccountId} não existe."]);
                }

                if (await identity.IsEmailConfirmedAsync(account) is false)
                {
                    logger.LogWarning("Attempted to set password for an unconfirmed email");
                    return Output.PolicyViolation(["O e-mail da conta precisa ser confirmado primeiro."]);
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