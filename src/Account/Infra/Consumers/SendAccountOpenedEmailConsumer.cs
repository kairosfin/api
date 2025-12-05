using Kairos.Shared.Contracts.Account;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Infra.Consumers;

public sealed class SendAccountOpenedEmailConsumer(
    ILogger<SendAccountOpenedEmailConsumer> logger,
    IConfiguration config
) : IConsumer<AccountOpened>
{
    public async Task Consume(ConsumeContext<AccountOpened> context)
    {
        var message = context.Message;

        using (logger.BeginScope("AccountId: {AccountId}", context.Message.Id))
        {
            try
            {
                logger.LogInformation("Sending opened account confirmation e-mail");

                var baseUrl = config["KairosUI:BaseUrl"];

                if (string.IsNullOrEmpty(baseUrl))
                {
                    logger.LogError("KairosUI:BaseUrl is not configured. Cannot send confirmation email.");
                    return;
                }

                var confirmationLink = $"{baseUrl}/auth?accountId={message.Id}&token={Uri.EscapeDataString(message.EmailConfirmationToken)}";

                var emailBody = $"<h1>Welcome to Kairos!</h1>" +
                                $"<p>Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.</p>";

                logger.LogInformation(
                    "Sending confirmation email to {Email} with link {Link}", 
                    message.Email, 
                    confirmationLink);

                // await _emailService.SendAsync(message.Email, "Kairos - Confirm your account", emailBody);

                await Task.Delay(TimeSpan.FromSeconds(3));

                logger.LogInformation("Opened account confirmation e-mail sent");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending the confirmation e-mail");
                throw;
            }
        }
    }
}