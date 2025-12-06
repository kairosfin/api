using Kairos.Account.Infra.Configuration;

namespace Kairos.Account.Configuration;

public partial class Settings
{
    public required JwtOptions Jwt { get; init; }
    public required MailingOptions Mailing { get; init; }
}