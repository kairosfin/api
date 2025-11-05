using Kairos.Shared.Contracts.Account;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Kairos.Account.Infra.Consumers;

public sealed class SendAccountOpenedEmailConsumer(
 ILogger<SendAccountOpenedEmailConsumer> logger
) : IConsumer<AccountOpened>
{
    public async Task Consume(ConsumeContext<AccountOpened> context)
    {
        using (logger.BeginScope("AccountId: {AccountId}", context.Message.Id))
        {
            try
            {
                logger.LogInformation("Sending opened account confirmation e-mail");

                await Task.Delay(TimeSpan.FromSeconds(5));

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