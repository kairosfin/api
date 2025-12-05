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

public sealed class ConfirmEmailUseCaseTests
{
    readonly ILogger<ConfirmEmailUseCase> _logger;
    readonly UserManager<Investor> _identity;
    readonly ConfirmEmailUseCase _sut;
    readonly Fixture _fixture;

    public ConfirmEmailUseCaseTests()
    {
        _logger = Substitute.For<ILogger<ConfirmEmailUseCase>>();
        _identity = MockUserManager();
        _sut = new ConfirmEmailUseCase(_logger, _identity);
        _fixture = new Fixture();
    }
    
    static UserManager<Investor> MockUserManager()
    {
        var store = Substitute.For<IUserStore<Investor>>();
        return Substitute.For<UserManager<Investor>>(store, null, null, null, null, null, null, null, null);
    }
    
    [Fact]
    public async Task Handle_ShouldReturnOk_WhenAccountAndTokenAreValid()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        
        var investor = Investor.OpenAccount("Test", "12378398202", "45692789302", "test@test.com", DateTime.UtcNow.AddYears(-20), true).Value!;
        investor.Id = command.AccountId;

        _identity.FindByIdAsync(command.AccountId.ToString()).Returns(investor);
        _identity.ConfirmEmailAsync(investor, command.ConfirmationToken).Returns(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.Ok);
        result.Messages.Should().Contain("E-mail confirmado com sucesso!");
        await _identity.Received(1).ConfirmEmailAsync(investor, command.ConfirmationToken);
    }

    [Theory]
    [InlineData(0, "valid-token")]
    [InlineData(1, "")]
    [InlineData(1, null)]
    public async Task Handle_ShouldReturnInvalidInput_WhenCommandIsInvalid(long accountId, string? token)
    {
        // Arrange
        var command = new ConfirmEmailCommand(accountId, token!, Guid.NewGuid());
        
        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.InvalidInput);
        result.Messages.Should().Contain("A conta e seu token de confirmação devem ser especificados.");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenAccountNotFound()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();
        _identity.FindByIdAsync(command.AccountId.ToString()).Returns((Investor)null!);

        // Act
        var result = await _sut.Handle(command, default);
        
        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain($"A conta {command.AccountId} não existe.");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenConfirmationFails()
    {
        // Arrange
        var command = _fixture.Create<ConfirmEmailCommand>();

        var investor = Investor.OpenAccount(
            "Test", "12378398202", "45692789302", "test@test.com", DateTime.UtcNow.AddYears(-20), true).Value!;
        investor.Id = command.AccountId;
        
        var identityError = new IdentityError { Description = "Invalid token." };
        
        _identity.FindByIdAsync(Arg.Any<string>()).Returns(investor);
        _identity.ConfirmEmailAsync(investor, command.ConfirmationToken)
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
        var command = _fixture.Create<ConfirmEmailCommand>();
        var exception = new InvalidOperationException("Database error");

        _identity.FindByIdAsync(command.AccountId.ToString()).ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(command, default);
        
        // Assert
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain(exception.Message);
    }
}