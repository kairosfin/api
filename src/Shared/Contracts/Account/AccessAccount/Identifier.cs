namespace Kairos.Shared.Contracts.Account.AccessAccount;

public sealed record Identifier(
    string Value,
    AccountIdentifier Type);