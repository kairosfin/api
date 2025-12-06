using Kairos.Account.Infra;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Contracts.Account.GetAccountInfo;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

internal sealed class GetAccountInfoUseCase(
    ILogger<GetAccountInfoUseCase> logger,
    AccountContext db) : IRequestHandler<GetAccountInfoQuery, Output<AccountInfo>>
{
    public async Task<Output<AccountInfo>> Handle(GetAccountInfoQuery input, CancellationToken cancellationToken)
    {
        try
        {
            var account = await db.Investors.FindAsync(input.Id, cancellationToken);

            if (account is null)
            {
                logger.LogWarning(
                    "Attempted to access non-existent account with ID {AccountId}", 
                    input.Id);
                return Output<AccountInfo>.PolicyViolation(["Conta não encontrada."]);
            }

            return Output<AccountInfo>.Ok(new AccountInfo(
                account.Id,
                account.Name,
                new AccountDocument(account.Document),
                new AccountPhone(
                    account.PhoneNumber!,
                    account.PhoneNumberConfirmed),
                new AccountEmail(
                    account.Email!,
                    account.EmailConfirmed),
                account.Birthdate,
                account.Gender,
                Address: null,
                ProfilePicUrl: null
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            return Output<AccountInfo>.UnexpectedError([
                "Um erro inesperado ocorreu...", 
                ex.Message]);
        }
    }
}
