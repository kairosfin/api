namespace Kairos.Shared.Contracts.Account;

public sealed record AccountOpened(
    string Id,
    string Name,
    string PhoneNumber,
    string Document,
    string Email,
    DateTime Birthdate,
    string ConfirmationTokenUrlEncoded,
    Guid CorrelationId);