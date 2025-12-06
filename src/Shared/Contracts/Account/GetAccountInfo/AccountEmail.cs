namespace Kairos.Shared.Contracts.Account.GetAccountInfo;

public sealed record AccountEmail(
    string Email,
    bool Confirmed);