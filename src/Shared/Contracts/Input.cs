namespace Kairos.Shared.Contracts;

/// <summary>
/// A wrapper for input data
/// </summary>
/// <remarks>
/// Inspired by the <see href="https://refactoring.guru/introduce-parameter-object">Parameter Object Pattern</see>
/// </remarks>
public record Input<TValue>
{
    /// <summary>
    /// Main input value
    /// </summary>
    public TValue Value { get; init; } = default!;
    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// To store custom additional data
    /// </summary>
    public IReadOnlyDictionary<string, object>? AdditionalData { get; init; }
}