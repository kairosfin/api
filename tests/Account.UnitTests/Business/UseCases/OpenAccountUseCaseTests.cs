using FluentAssertions;
using Kairos.Account.Business.UseCases;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kairos.Account.UnitTests.Business.UseCases;

public sealed class OpenAccountUseCaseTests
{
    readonly IBus _bus = Substitute.For<IBus>();
    readonly ILogger<OpenAccountUseCase> _logger = Substitute.For<ILogger<OpenAccountUseCase>>();
    readonly OpenAccountUseCase _sut;

    public OpenAccountUseCaseTests() => _sut = new(_logger, _bus);

    [Fact(DisplayName = "Open Account - Happy path")]
    public async Task OpenAccount_HappyPath()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        OpenAccountCommand command = new(
            "Foo",
            "Bar",
            "51625637263",
            "foo.bar@baz.com",
            DateTime.Today,
            true,
            Guid.NewGuid());

        // Act
        var output = await _sut.Handle(command, ct);

        // Assert
        output.Status.Should().Be(OutputStatus.Created);

        await _bus.Received().Publish(
            Arg.Is<AccountOpened>(e => e.Document == command.Document),
            Arg.Any<IPipe<PublishContext<AccountOpened>>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}