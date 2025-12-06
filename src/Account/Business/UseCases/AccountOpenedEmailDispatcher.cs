using Kairos.Account.Configuration;
using Kairos.Account.Domain.Abstraction;
using Kairos.Account.Infra.Configuration;
using Kairos.Shared.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kairos.Account.Business.Command;

internal sealed class AccountOpenedEmailDispatcher(
    ILogger<AccountOpenedEmailDispatcher> logger,
    IEmailSender emailSender,
    IOptions<Settings> settings
) : IRequestHandler<DispatchAccountOpenedEmailCommand, Output>
{
    readonly MailingOptions _options = settings.Value.Mailing;

    public async Task<Output> Handle(
        DispatchAccountOpenedEmailCommand input, 
        CancellationToken cancellationToken)
    {
        var enrichers = new Dictionary<string, object?>
        {
            ["CorrelationId"] = input.CorrelationId,
            ["AccountId"] = input.Id,
        };

        using (logger.BeginScope(enrichers))
        {
            try
            {
                logger.LogInformation("Sending opened account confirmation e-mail");

                var emailToken = Uri.EscapeDataString(input.EmailConfirmationToken);
                var passwordToken = Uri.EscapeDataString(input.PasswordResetToken);
                var baseUrl = _options.RedirectToBaseUrl.ToString();

                var confirmationLink = $"{baseUrl}/login?accountId={input.Id}&emailToken={emailToken}&passToken={passwordToken}";

                var emailBody = $"""
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Bem-vindo ao Kairos</title>
                </head>
                <body style="font-family: Arial, sans-serif; text-align: center; color: #333;">
                    <div style="width: 100%; max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;">
                        <h1 style="color: #000;">Bem-vindo ao Kairos, {input.Name.Split(" ")[0]}!</h1>
                        <p style="font-size: 16px;">Não deixe a oportunidade passar e confirme a abertura da sua conta clicando no botão abaixo.</p>
                        <a href="{confirmationLink}" 
                           style="display: inline-block; 
                                  padding: 12px 24px; 
                                  margin: 20px 0; 
                                  font-size: 18px; 
                                  font-weight: bold;
                                  color: #ffffff; 
                                  background-color: #1C1C1E; 
                                  border-radius: 5px; 
                                  text-decoration: none;">
                            Confirmar Abertura
                        </a>
                        <p style="font-size: 12px; color: #777;">Se você não criou essa conta, por favor, ignore este e-mail.</p>
                    </div>
                </body>
                </html>
                """;

                logger.LogInformation(
                    "Sending confirmation email to {Email} with link {Link}", 
                    input.Email, 
                    confirmationLink);

                await emailSender.SendAsync(
                    input.Email, 
                    "⏳ Kairos | Abertura de conta", 
                    emailBody, 
                    cancellationToken);

                logger.LogInformation("Opened account confirmation e-mail sent");

                return Output.Empty;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An unexpected error occurred");
                throw;
            }
        }
    }
}
