using Kairos.MarketData.Infra.Dtos;

namespace Kairos.MarketData.Infra.Abstractions;

internal interface IPriceRepository
{
    IAsyncEnumerable<Price> Get(
        string ticker, 
        DateTime from,
        CancellationToken ct);

    /// <summary>
    /// Append a collection of prices into the database
    /// </summary>
    /// <remarks>The existent prices (Ticker, Date) are gonna be ignored</remarks>
    /// <param name="ticker"></param>
    /// <param name="prices"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task Append(
        string ticker,
        Price[] prices, 
        CancellationToken ct);
}
