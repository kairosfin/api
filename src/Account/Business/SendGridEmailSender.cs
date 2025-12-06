using Kairos.Account.Domain.Abstraction;

namespace Kairos.Account.Business;

internal sealed class SendGridEmailSender() : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlContent, CancellationToken cancellationToken)
    {
        var apiKey = _config["EmailSettings:SendGridApiKey"];
        var fromEmail = _config["EmailSettings:FromEmail"];
        var fromName = _config["EmailSettings:FromName"];

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, htmlContent);

        await client.SendEmailAsync(msg);
    }
}
