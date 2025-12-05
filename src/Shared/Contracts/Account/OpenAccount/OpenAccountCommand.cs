using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.Account;

public sealed record OpenAccountCommand(
    string Name,
    string PhoneNumber,
    string Document,
    string Email,
    DateTime Birthdate,
    bool AcceptTerms,
    Guid CorrelationId) : ICommand;