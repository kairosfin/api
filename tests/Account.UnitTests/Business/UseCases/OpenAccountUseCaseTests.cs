using AutoFixture;
using FluentAssertions;
using Kairos.Account.Business.UseCases;
using Kairos.Account.Domain;
using Kairos.Account.Infra;
using Kairos.Shared.Contracts.Account;
using Kairos.Shared.Enums;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Kairos.Account.UnitTests.Business.UseCases;

public sealed class OpenAccountUseCaseTests
{
    readonly ILogger<OpenAccountUseCase> _logger;
    readonly IBus _bus;
    readonly UserManager<Investor> _identity;
    readonly AccountContext _db;
    readonly OpenAccountUseCase _sut;
    readonly Fixture _fixture;

    public OpenAccountUseCaseTests()
    {
        _logger = Substitute.For<ILogger<OpenAccountUseCase>>();
        _bus = Substitute.For<IBus>();
        _identity = MockUserManager();

        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new AccountContext(options);

        _sut = new OpenAccountUseCase(_logger, _bus, _identity, _db);
        
        _fixture = new Fixture();
        _fixture.Customize<OpenAccountCommand>(composer =>
            composer.With(c => c.AcceptTerms, true)
            .With(c => c.Birthdate, DateTime.UtcNow.AddYears(-19))
            .With(c => c.Email, "gustax.dev@outlook.com")
            .With(c => c.PhoneNumber, "11994977777")
            .With(c => c.Document, "77283890273"));
    }

    private static UserManager<Investor> MockUserManager()
    {
        var store = Substitute.For<IUserStore<Investor>>();
        var userManager = Substitute.For<UserManager<Investor>>(store, null, null, null, null, null, null, null, null);
        
        userManager.CreateAsync(Arg.Any<Investor>()).Returns(IdentityResult.Success);
        
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<Investor>()).Returns("email-token");
        userManager.GeneratePasswordResetTokenAsync(Arg.Any<Investor>()).Returns("password-token");
        
        return userManager;
    }

    [Fact]
    public async Task Handle_ShouldReturnCreated_WhenCommandIsValidAndIdentifiersAreUnique()
    {
        // Arrange
        var command = _fixture.Build<OpenAccountCommand>()
            .With(c => c.AcceptTerms, true)
            .With(c => c.Birthdate, DateTime.UtcNow.AddYears(-19))
            .With(c => c.Email, "gustax.dev@outlook.com")
            .With(c => c.PhoneNumber, "11994977777")
            .With(c => c.Document, "77283890273")
            .Create();

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.Created);
        result.Messages.Should().Contain(m => m.Contains("aberta"));

        await _identity.Received(1).CreateAsync(Arg.Is<Investor>(i => i.Email == command.Email));
        await _bus.Received().Publish(
            Arg.Is<AccountOpened>(e => e.Document == command.Document),
            Arg.Any<IPipe<PublishContext<AccountOpened>>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenEmailIsAlreadyInUse()
    {
        // Arrange
        var openAccountResult = Investor.OpenAccount(
            "Existing User", "55388899922", "11999998888", "taken@email.com", 
            DateTime.UtcNow.AddYears(-30), true);
        var existingInvestor = openAccountResult.Value!;
        
        _db.Investors.Add(existingInvestor);
        await _db.SaveChangesAsync();

        var command = _fixture.Build<OpenAccountCommand>()
            .With(c => c.Email, "taken@email.com")
            .Create();

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain("O e-mail, telefone e/ou documento já está(ão) em uso.");

        await _identity.DidNotReceive().CreateAsync(Arg.Any<Investor>());
        await _bus.DidNotReceiveWithAnyArgs().Publish<AccountOpened>(default!);
    }


    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenUserIsUnderage()
    {
        // Arrange
        var command = _fixture.Build<OpenAccountCommand>()
            .With(c => c.Birthdate, DateTime.UtcNow.AddYears(-17))
            .With(c => c.AcceptTerms, true)
            .Create();

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain("É necessário ser maior de idade para abrir a conta.");
    }
    
    [Fact]
    public async Task Handle_ShouldReturnPolicyViolation_WhenIdentityCreationFails()
    {
        // Arrange
        var command = _fixture.Create<OpenAccountCommand>();
        
        var identityError = new IdentityError { Description = "Password is too weak." };
        _identity.CreateAsync(Arg.Any<Investor>()).Returns(IdentityResult.Failed(identityError));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.Status.Should().Be(OutputStatus.PolicyViolation);
        result.Messages.Should().Contain("Password is too weak.");
        
        await _bus.DidNotReceiveWithAnyArgs().Publish<AccountOpened>(default!);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnexpectedError_WhenDatabaseThrowsException()
    {
        // Arrange
        var command = _fixture.Create<OpenAccountCommand>();
        
        var options = new DbContextOptionsBuilder<AccountContext>().Options;
        var dbContextMock = Substitute.For<AccountContext>(options);
        dbContextMock.Investors.Returns(_ => { throw new Exception("Database connection failed"); });
        
        var sutWithBadDb = new OpenAccountUseCase(_logger, _bus, _identity, dbContextMock);

        // Act
        var result = await sutWithBadDb.Handle(command, default);
        
        // Assert
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain("Database connection failed");
    }
}