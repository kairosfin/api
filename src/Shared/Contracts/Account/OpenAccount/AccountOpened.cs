namespace Kairos.Shared.Contracts.Account;

public sealed record AccountOpened(
    long Id,
    string Name,
    string PhoneNumber,
    string Document,
    string Email,
    DateTime Birthdate,
    string EmailConfirmationToken,
    string PasswordResetToken,
    Guid CorrelationId);