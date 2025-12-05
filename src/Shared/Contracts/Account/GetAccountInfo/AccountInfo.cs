using Kairos.Account.Domain.Enum;

namespace Kairos.Shared.Contracts.Account.GetAccountInfo;

public sealed record AccountInfo(
    long Id,
    string Name,
    DateTime Birthdate,
    Gender Gender,
    string PhoneNumber,
    string Document,
    string Email,
    string? Address,
    Uri? ProfilePicUrl
);
