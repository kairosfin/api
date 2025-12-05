using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.Account;

public sealed record SetPasswordCommand(
    long AccountId, 
    string Pass, 
    string PassConfirmation,
    string Token,
    Guid CorrelationId) : ICommand;