namespace Kairos.Account.Domain.Abstraction;

public interface IEmailSender
{
    Task SendAsync(
        string to, 
        string subject, 
        string htmlContent,
        CancellationToken cancellationToken);
}
