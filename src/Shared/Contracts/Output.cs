using Kairos.Shared.Enums;

namespace Kairos.Shared.Contracts;

public record Output
{
    public OutputStatus Status { get; init; }

    /// <summary>
    /// Success or failure messages
    /// </summary>
    public IEnumerable<string> Messages { get; init; }

    public bool IsSuccess => Status
        is OutputStatus.Ok
        or OutputStatus.Created
        or OutputStatus.Empty;

    public bool IsFailure => !IsSuccess;

    public Output(OutputStatus status, IEnumerable<string> messages)
    {
        Status = status;
        Messages = messages;
    }

    public Output(Output result)
    {
        Status = result.Status;
        Messages = result.Messages;
    }

    #region Success
    
    public static Output Ok(IEnumerable<string>? messages = null) =>
        new(OutputStatus.Ok, messages ?? []);

    public static Output Created(IEnumerable<string>? messages = null) =>
        new(OutputStatus.Created, messages ?? []);

    public readonly static Output Empty = new(OutputStatus.Empty, []);
    #endregion

    #region Failure
    public static Output UnexpectedError(IEnumerable<string> messages) =>
        new(OutputStatus.UnexpectedError, messages);

    public static Output CredentialsRequired(IEnumerable<string> messages) =>
        new(OutputStatus.CredentialsRequired, messages);

    public static Output PolicyViolation(IEnumerable<string> messages) =>
        new(OutputStatus.PolicyViolation, messages);

    public static Output InvalidInput(IEnumerable<string> messages) =>
        new(OutputStatus.InvalidInput, messages);
    #endregion
}

public sealed record Output<TValue> : Output
{
    /// <summary>
    /// Output value
    /// </summary>
    public TValue? Value { get; init; }

    public bool HasValue => IsSuccess && Value is not null;

    Output(TValue? value, OutputStatus status, IEnumerable<string> messages)
        : base(status, messages) => Value = value;

    /// <summary>
    /// Constructs a Result from another Result
    /// </summary>
    public Output(Output other) : base(other.Status, other.Messages) { }

    public static implicit operator TValue?(Output<TValue> result) =>
        result.Value;

    #region Success
    public static Output<TValue> Ok(TValue value, IEnumerable<string>? messages = null) =>
        new(value, OutputStatus.Ok, messages ?? []);

    public static Output<TValue> Created(TValue value, IEnumerable<string>? messages = null) =>
        new(value, OutputStatus.Created, messages ?? []);

    public new readonly static Output<TValue> Empty = new(default, OutputStatus.Empty, []);
    #endregion

    #region Failure
    public static Output<TValue> InvalidInput(IEnumerable<string> messages, TValue? value = default) =>
        new(value, OutputStatus.InvalidInput, messages);

    public static Output<TValue> NotFound(IEnumerable<string> messages, TValue? value = default) =>
        new(value, OutputStatus.NotFound, messages);

    public static Output<TValue> PolicyViolation(IEnumerable<string> messages, TValue? value = default) =>
        new(value, OutputStatus.PolicyViolation, messages);

    public static Output<TValue> UnexpectedError(IEnumerable<string> messages, TValue? value = default) =>
        new(value, OutputStatus.UnexpectedError, messages);
    #endregion
}