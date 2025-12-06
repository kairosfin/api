using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.Account.AccessAccount;

namespace Kairos.Shared.Contracts.Account;

public sealed record SetPasswordCommand(
    Identifier AccountIdentifier, 
    string Pass, 
    string PassConfirmation,
    string Token,
    Guid CorrelationId) : ICommand;