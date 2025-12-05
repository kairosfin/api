using System;
using Kairos.Account.Domain;
using Kairos.Shared.Contracts;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Business.UseCases;

internal sealed class ConfirmEmailUseCase(
    ILogger<ConfirmEmailUseCase> logger,
    UserManager<Investor> identity
) : IRequestHandler<ConfirmEmailCommand, Output>
{
    public async Task<Output> Handle(ConfirmEmailCommand input, CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?>
        {
            ["CorrelationId"] = input.CorrelationId,
            ["AccountId"] = input.AccountId,
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                return await ConfirmEmail(input);
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

    async Task<Output> ConfirmEmail(ConfirmEmailCommand input)
    {
        if (input.AccountId is 0 || string.IsNullOrEmpty(input.ConfirmationToken))
        {
            return Output.InvalidInput(["A conta e seu token de confirmação devem ser especificados."]);
        }

        var account = await identity.FindByIdAsync(input.AccountId.ToString());

        if (account is null)
        {
            logger.LogWarning("Account not found");
            return Output.PolicyViolation([$"A conta {input.AccountId} não existe."]);
        }

        var confirmationResult = await identity.ConfirmEmailAsync(
            account,
            input.ConfirmationToken);

        if (confirmationResult.Succeeded is false)
        {
            var errors = confirmationResult.Errors
                .Select(e => e.Description)
                .ToList();

            logger.LogWarning("E-mail confirmation failed. Errors: {@Errors}", errors);
            return Output.PolicyViolation(errors);
        }

        return Output.Ok([
            "E-mail confirmado com sucesso!",
            "Defina uma senha para acesso à conta."]);
    }
}
