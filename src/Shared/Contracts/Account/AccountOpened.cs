namespace Kairos.Shared.Contracts.Account;

public sealed record AccountOpened(
    int Id,
    string FirstName,
    string LastName,
    string Document,
    string Email,
    DateTime Birthdate);