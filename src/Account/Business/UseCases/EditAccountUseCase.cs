using Kairos.Account.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

internal sealed class EditAccountUseCase(
    AccountContext db,
    ILogger<EditAccountUseCase> logger
) : IRequestHandler<EditAccountCommand, Output>
{
    public async Task<Output> Handle(
        EditAccountCommand input,
        CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?> 
        { 
            ["AccountId"] = input.Id, 
            ["CorrelationId"] = input.CorrelationId 
        };
        
        using (logger.BeginScope(enrichers))
        {
            try
            {
                var account = await db.Investors.FindAsync(input.Id, cancellationToken);

                if (account is null)
                {
                    logger.LogWarning("Sign-in failed. Account not found.");
                    return Output.CredentialsRequired(["Credenciais inválidas."]);
                }

                // TODO: suportar upload de profile pic ao blob storage
                account
                    .SetAddress(input.Address)
                    .SetGender(input.Gender);

                await db.SaveChangesAsync(cancellationToken);

                return Output.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred while updating the account.");
                return Output.UnexpectedError([
                    "Houve um erro inesperado na edição da conta... tente novamente mais tarde.",
                    ex.Message]);
            }
        }
    }
}
