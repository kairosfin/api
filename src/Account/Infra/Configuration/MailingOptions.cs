namespace Kairos.Account.Infra.Configuration;

public sealed record MailingOptions(
    string ApiKey, 
    string FromEmail, 
    string FromName,
    Uri RedirectToBaseUrl);