using Kairos.Shared.Abstractions;

namespace Kairos.Account.Business.Command;

public sealed record DispatchAccountOpenedEmailCommand(
    long Id,
    string Name,
    string Email,
    string EmailConfirmationToken,
    string PasswordResetToken,
    Guid CorrelationId) : ICommand;
