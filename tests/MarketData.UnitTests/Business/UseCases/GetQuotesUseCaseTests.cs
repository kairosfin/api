using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kairos.MarketData.Business.UseCases;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData.GetStockQuotes;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Output = Kairos.Shared.Contracts.Output<System.Collections.Generic.IAsyncEnumerable<Kairos.Shared.Contracts.MarketData.GetStockQuotes.Quote>>;

namespace Kairos.MarketData.UnitTests.Business.UseCases;

public sealed class GetQuotesUseCaseTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IBrapi> _brapi;
    private readonly Mock<IStockRepository> _repo;
    private readonly Mock<ILogger<GetQuotesUseCase>> _logger;
    private readonly GetQuotesUseCase _sut;

    public GetQuotesUseCaseTests()
    {
        _fixture = new Fixture();
        _brapi = new Mock<IBrapi>();
        _repo = new Mock<IStockRepository>();
        _logger = new Mock<ILogger<GetQuotesUseCase>>();
        _sut = new GetQuotesUseCase(_brapi.Object, _logger.Object, _repo.Object);
    }

    [Fact]
    public async Task Handle_WhenDatabaseHasUpToDatePrices_ShouldReturnPricesFromDatabase()
    {
        // Arrange
        var query = new GetQuotesQuery("PETR4", QuoteRange.FiveDays);
        var dbPrices = new List<Price>
        {
            new("PETR4", DateTime.Today.ToUnixTimeSeconds(), 30, 30)
        };

        _repo.Setup(r => r.GetPrices(query.Ticker, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(dbPrices.ToAsyncEnumerable());

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var quotes = await result.Value.ToListAsync();
        quotes.Should().HaveCount(1);
        quotes[0].Close.Should().Be(30);

        _brapi.Verify(b => b.GetQuote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDatabaseHasOutdatedPrices_ShouldFetchFromApiAndReturnFilteredPrices()
    {
        // Arrange
        var query = new GetQuotesQuery("PETR4", QuoteRange.Day);
        var outdatedDbPrices = new List<Price>
        {
            new("PETR4", DateTime.Today.AddDays(-10).ToUnixTimeSeconds(), 25, 25)
        };
        var apiQuotes = new List<StockQuote>
        {
            new() { Date = DateTime.Today.ToUnixTimeSeconds(), Close = 30, AdjustedClose = 30 },
            new() { Date = DateTime.Today.AddDays(-10).ToUnixTimeSeconds(), Close = 25, AdjustedClose = 25 }
        };
        var quoteResponse = _fixture.Build<QuoteResponse>()
            .With(r => r.Results, [_fixture.Build<StockDetail>().With(qr => qr.HistoricalDataPrice, apiQuotes).Create()])
            .Create();

        _repo.Setup(r => r.GetPrices(query.Ticker, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(outdatedDbPrices.ToAsyncEnumerable());
        _brapi.Setup(b => b.GetQuote(query.Ticker, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(quoteResponse);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var quotes = await result.Payload.ToListAsync();
        
        // Should only return the quote within the requested range (1 day)
        quotes.Should().HaveCount(1);
        quotes[0].Date.Should().Be(DateTime.Today);
        quotes[0].Close.Should().Be(30);

        _brapi.Verify(b => b.GetQuote(query.Ticker, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        
        // Give the fire-and-forget task a moment to run
        await Task.Delay(100); 
        _repo.Verify(r => r.AddPrices(It.IsAny<IEnumerable<Price>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnUnexpectedError()
    {
        // Arrange
        var query = new GetQuotesQuery("PETR4", QuoteRange.Day);
        var exception = new InvalidOperationException("DB error");

        _repo.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Messages.Should().Contain(exception.Message);
        
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldReturnUnexpectedError()
    {
        // Arrange
        var query = new GetQuotesQuery("PETR4", QuoteRange.Day);
        var exception = new InvalidOperationException("API error");

        // Force fallback to API by returning empty list from DB
        _repo.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Price>());

        _brapi.Setup(b => b.GetQuote(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Messages.Should().Contain(exception.Message);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}

// Helper extension for converting Unix time
public static class DateTimeExtensions
{
    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
    }
}