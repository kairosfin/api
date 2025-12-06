using AutoFixture;
using FluentAssertions;
using Kairos.Account.Business.UseCases;
using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Contracts.Account.AccessAccount;
using Kairos.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kairos.Account.UnitTests.Business.UseCases;

public sealed class SetPasswordUseCaseTests
{
    readonly ILogger<SetPasswordUseCase> _logger;
    readonly UserManager<Investor> _identity;
    readonly AccountContext _db;
    readonly SetPasswordUseCase _sut;
    readonly Fixture _fixture;

    public SetPasswordUseCaseTests()
    {
        _logger = Substitute.For<ILogger<SetPasswordUseCase>>();
        _identity = MockUserManager();
        
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AccountContext(options);
        
        _sut = new SetPasswordUseCase(_identity, _logger, _db);
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
        const string userEmail = "test@test.com";
        var identifier = new Identifier(
            userEmail, 
            AccountIdentifier.Email);
        var command = _fixture.Build<SetPasswordCommand>()
            .With(c => c.AccountIdentifier, identifier)
            .With(c => c.Pass, "111111")
            .With(c => c.PassConfirmation, "111111")
            .Create();
        
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", userEmail, DateTime.UtcNow.AddYears(-25), true).Value!;
        _db.Investors.Add(investor);
        await _db.SaveChangesAsync();

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
        var identifier = new Identifier("notfound@test.com", AccountIdentifier.Email);
        var command = _fixture.Build<SetPasswordCommand>()
            .With(c => c.AccountIdentifier, identifier)
            .With(c => c.Pass, "111111")
            .With(c => c.PassConfirmation, "111111")
            .Create();
        
        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain($"A conta com identificador #{command.AccountIdentifier.Value} não existe.");
    }

    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenEmailIsNotConfirmed()
    {
        // Arrange
        const string userEmail = "unconfirmed@test.com";
        var identifier = new Identifier(userEmail, AccountIdentifier.Email);
        var command = _fixture.Build<SetPasswordCommand>()
            .With(c => c.AccountIdentifier, identifier)
            .With(c => c.Pass, "111111")
            .With(c => c.PassConfirmation, "111111")
            .Create();
            
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", userEmail, DateTime.UtcNow.AddYears(-25), true).Value!;
        _db.Investors.Add(investor);
        await _db.SaveChangesAsync();

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
        const string userEmail = "valid@test.com";
        var identifier = new Identifier(userEmail, AccountIdentifier.Email);
        var command = _fixture.Build<SetPasswordCommand>()
            .With(c => c.AccountIdentifier, identifier)
            .With(c => c.Pass, "111111")
            .With(c => c.PassConfirmation, "111111")
            .Create();
            
        var investor = Investor.OpenAccount("Test", "12345678901", "11987654321", userEmail, DateTime.UtcNow.AddYears(-25), true).Value!;
        _db.Investors.Add(investor);
        await _db.SaveChangesAsync();
        
        var identityError = new IdentityError { Description = "Invalid token." };
        
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
    public async Task Handle_ShouldReturnUnexpectedError_WhenDatabaseThrowsException()
    {
        // Arrange
        var command = _fixture.Create<SetPasswordCommand>();
        var exception = new InvalidOperationException("Database connection failed");
        
        var options = new DbContextOptionsBuilder<AccountContext>().Options;
        var dbContextMock = Substitute.For<AccountContext>(options);
        dbContextMock.Investors.Returns(_ => throw exception);
        
        var sutWithBadDb = new SetPasswordUseCase(_identity, _logger, dbContextMock);

        // Act
        var result = await sutWithBadDb.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain(exception.Message);
    }
}