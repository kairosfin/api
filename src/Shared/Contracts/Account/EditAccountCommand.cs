using Kairos.Account.Domain.Enum;
using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.Account;

public sealed record EditAccountCommand(
    long Id,
    Gender? Gender,
    string? Address,
    Guid CorrelationId) : ICommand;