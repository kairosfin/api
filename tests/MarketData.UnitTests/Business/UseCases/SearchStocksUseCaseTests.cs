using AutoFixture;
using FluentAssertions;
using Kairos.MarketData.Business.UseCases;
using Kairos.MarketData.Infra.Abstractions;
using Kairos.MarketData.Infra.Dtos;
using Kairos.Shared.Contracts.MarketData;
using Kairos.Shared.Contracts.MarketData.SearchStocks;
using Kairos.Shared.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Internal;

namespace Kairos.MarketData.UnitTests.Business.UseCases;

public sealed class SearchStocksUseCaseTests
{
    readonly Fixture _fixture;
    readonly Mock<IBrapi> _brapi;
    readonly Mock<IStockRepository> _repo;
    readonly Mock<ILogger<SearchStocksUseCase>> _logger;
    readonly SearchStocksUseCase _sut;

    public SearchStocksUseCaseTests()
    {
        _fixture = new Fixture();
        _brapi = new Mock<IBrapi>();
        _repo = new Mock<IStockRepository>();
        _logger = new Mock<ILogger<SearchStocksUseCase>>();
        _sut = new SearchStocksUseCase(_brapi.Object, _repo.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_WhenStocksAreInCache_ShouldReturnStocksFromRepository()
    {
        // Arrange
        var query = new SearchStocksQuery(["PETR"], 1, 10);
        var cachedStocks = _fixture.CreateMany<Stock>(5).ToAsyncEnumerable();

        _repo.Setup(r => r.GetByTickerOrNameOrSector(
                query.Query, query.Page, query.Limit, It.IsAny<CancellationToken>()))
            .Returns(cachedStocks);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stocks = await result.Value.ToListAsync();
        stocks.Should().HaveCount(5);

        _brapi.Verify(b => b.GetStocks(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheIsEmpty_ShouldFetchFromApiAndReturnPaginatedResults()
    {
        // Arrange
        // We want page 2 with a limit of 1, searching for "banco"
        var query = new SearchStocksQuery(["banco"], 2, 1); 
        var apiStocks = new[]
        {
            _fixture.Build<StockSummary>()
                .With(s => s.Name, "Itaú Unibanco")
                .With(s => s.Stock, "ITUB4")
                .With(s => s.Logo, "https://example.com/itub4.png")
                .Create(),
            _fixture.Build<StockSummary>()
                .With(s => s.Name, "Banco Bradesco")
                .With(s => s.Stock, "BBDC4")
                .With(s => s.Logo, "https://example.com/bbdc4.png")
                .Create(),
            _fixture.Build<StockSummary>()
                .With(s => s.Name, "Banco do Brasil")
                .With(s => s.Stock, "BBAS3")
                .With(s => s.Logo, "https://example.com/bbas3.png")
                .Create(),
            _fixture.Build<StockSummary>()
                .With(s => s.Name, "Vale")
                .With(s => s.Stock, "VALE3")
                .With(s => s.Logo, "https://example.com/vale3.png")
                .Create() // Should be filtered out
        };

        var apiResponse = new StockSearchResponse(apiStocks);

        _repo.Setup(r => r.GetByTickerOrNameOrSector(
                query.Query, query.Page, query.Limit, It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Stock>()); // Simulate cache miss
            
        _brapi.Setup(b => b.GetStocks(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stocks = await result.Value.ToListAsync();
        
        // Assert correct filtering and pagination (page 2, limit 1 should return the second bank)
        stocks.Should().HaveCount(1);
        stocks.Single().Ticker.Should().Be("BBDC4");

        _brapi.Verify(b => b.GetStocks(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        
        // Give the background caching task a moment to execute
        await Task.Delay(100); 
        _repo.Verify(r => r.Upsert(It.Is<Stock[]>(s => s.Length == 4), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenApiReturnsNoStocks_ShouldReturnNotFound()
    {
        // Arrange
        var query = new SearchStocksQuery(["INVALID"], 1, 10);
        var apiResponse = new StockSearchResponse([]);

        _repo.Setup(r => r.GetByTickerOrNameOrSector(
                It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Stock>());
        _brapi.Setup(b => b.GetStocks(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(OutputStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnUnexpectedError()
    {
        // Arrange
        var query = new SearchStocksQuery(["error"], 1, 10);
        var exception = new InvalidOperationException("Database connection failed");

        _repo.Setup(r => r.GetByTickerOrNameOrSector(
                It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Throws(exception);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain(exception.Message);
    }
    
    [Fact]
    public async Task Handle_WhenApiThrowsException_ShouldReturnUnexpectedError()
    {
        // Arrange
        var query = new SearchStocksQuery(["error"], 1, 10);
        var exception = new InvalidOperationException("API is down");

        _repo.Setup(r => r.GetByTickerOrNameOrSector(
                It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<Stock>());
        _brapi.Setup(b => b.GetStocks(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(OutputStatus.UnexpectedError);
        result.Messages.Should().Contain(exception.Message);
    }
}