using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.Account.AccessAccount;

namespace Kairos.Shared.Contracts;

public sealed record AccessAccountCommand(
    Identifier Identifier,
    string Password,
    Guid CorrelationId
) : ICommand<string>;