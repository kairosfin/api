using Kairos.Account.Domain.Enum;

namespace Kairos.Shared.Contracts.Account.GetAccountInfo;

public sealed record AccountInfo(
    long Id,
    string Name,
    AccountDocument Document,
    AccountPhone Phone,
    AccountEmail Email,
    DateTime Birthdate,
    Gender Gender,
    string? Address,
    Uri? ProfilePicUrl
);