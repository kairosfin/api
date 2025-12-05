namespace Kairos.Account.Configuration;

public sealed class JwtOptions
{
    public required string CookieName { get; init; }
    public required string Secret { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int ExpiryMinutes { get; init; }
}