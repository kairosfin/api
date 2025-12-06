using Kairos.Account.Business.Command;
using Kairos.Shared.Contracts.Account;
using MassTransit;
using MediatR;

namespace Kairos.Account.Infra.Consumers;

public sealed class SendAccountOpenedEmailConsumer(IMediator mediator) : IConsumer<AccountOpened>
{
    public Task Consume(ConsumeContext<AccountOpened> context)
    {
        var msg = context.Message;

        return mediator.Send(new DispatchAccountOpenedEmailCommand(
            msg.Id,
            msg.Name,
            msg.Email,
            msg.EmailConfirmationToken,
            msg.PasswordResetToken,
            msg.CorrelationId));
    }
}