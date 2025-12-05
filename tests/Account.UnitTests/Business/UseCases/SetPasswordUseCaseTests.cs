using AutoFixture;
using FluentAssertions;
using Kairos.Account.Business.UseCases;
using Kairos.Account.Domain;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Kairos.Account.UnitTests.Business.UseCases;

public sealed class SetPasswordUseCaseTests
{
    readonly ILogger<SetPasswordUseCase> _logger;
    readonly UserManager<Investor> _identity;
    readonly SetPasswordUseCase _sut;
    readonly Fixture _fixture;

    public SetPasswordUseCaseTests()
    {
        _logger = Substitute.For<ILogger<SetPasswordUseCase>>();
        _identity = MockUserManager();
        _sut = new SetPasswordUseCase(_identity, _logger);
        _fixture = new Fixture();
    }

    private static UserManager<Investor> MockUserManager()
    {
        var store = Substitute.For<IUserStore<Investor>>();
        return Substitute.For<UserManager<Investor>>(store, null, null, null, null, null, null, null, null);
    }

    [Fact]
    public async Task Handle_ShouldReturnOk_WhenCommandIsValid()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", "test@test.com", DateTime.UtcNow.AddYears(-25), true).Value!;
        investor.Id = command.AccountId;

        _identity.FindByIdAsync(command.AccountId.ToString()).Returns(investor);
        _identity.IsEmailConfirmedAsync(investor).Returns(true);
        _identity.ResetPasswordAsync(investor, command.Token, command.Pass).Returns(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.Ok);
        result.Messages.Should().Contain("Senha definida com sucesso!");
        await _identity.Received(1).ResetPasswordAsync(investor, command.Token, command.Pass);
    }

    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenAccountNotFound()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        _identity.FindByIdAsync(command.AccountId.ToString()).Returns((Investor)null!);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain($"A conta #{command.AccountId} não existe.");
    }

    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenEmailIsNotConfirmed()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", "test@test.com", DateTime.UtcNow.AddYears(-25), true).Value!;
        investor.Id = command.AccountId;

        _identity.FindByIdAsync(command.AccountId.ToString()).Returns(investor);
        _identity.IsEmailConfirmedAsync(investor).Returns(false);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain("O e-mail da conta precisa ser confirmado primeiro.");
        await _identity.DidNotReceive().ResetPasswordAsync(Arg.Any<Investor>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenTokenIsInvalid()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", "test@test.com", DateTime.UtcNow.AddYears(-25), true).Value!;
        investor.Id = command.AccountId;
        var identityError = new IdentityError { Description = "Invalid token." };

        _identity.FindByIdAsync(command.AccountId.ToString()).Returns(investor);
        _identity.IsEmailConfirmedAsync(investor).Returns(true);
        _identity.ResetPasswordAsync(investor, command.Token, command.Pass)
            .Returns(IdentityResult.Failed(identityError));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain("Invalid token.");
    }

    [Fact]
    public async Task Handle_ShouldReturnUnexpectedError_WhenIdentityThrowsException()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        var exception = new InvalidOperationException("Database connection failed");

        _identity.FindByIdAsync(command.AccountId.ToString()).ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain(exception.Message);
    }
}