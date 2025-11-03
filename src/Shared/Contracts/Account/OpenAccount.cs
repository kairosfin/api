using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.Account;

public sealed record OpenAccount(
    string FirstName,
    string LastName,
    string Document,
    string Email,
    DateTime Birthdate,
    bool AcceptTerms,
    Guid CorrelationId) : ICommand;