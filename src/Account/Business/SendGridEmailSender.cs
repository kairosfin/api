using Kairos.Account.Configuration;
using Kairos.Account.Domain.Abstraction;
using Kairos.Account.Infra.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Kairos.Account.Business;

internal sealed class SendGridEmailSender(IOptions<Settings> settings) : IEmailSender
{
    readonly MailingOptions _options = settings.Value.Mailing;

    public Task SendAsync(
        string to, 
        string subject, 
        string htmlContent, 
        CancellationToken cancellationToken)
    {
        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var toAddress = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, htmlContent);

        return client.SendEmailAsync(msg, cancellationToken);
    }
}
