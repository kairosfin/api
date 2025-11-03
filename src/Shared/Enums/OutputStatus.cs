namespace Kairos.Shared.Enums;

public enum OutputStatus
{
    #region Success
    /// <summary>
    /// Happy path
    /// </summary>
    Ok,

    /// <summary>
    /// State changed
    /// </summary>
    Created,

    /// <summary>
    /// Empty result of a query
    /// </summary>
    Empty,
    #endregion

    #region Failure
    /// <summary>
    /// Incorrect or incomplete input data
    /// </summary>
    InvalidInput,

    /// <summary>
    /// Non-existent identifier
    /// </summary>
    UnexistentId,

    /// <summary>
    /// Business logic violation
    /// </summary>
    BusinessLogicViolation,

    /// <summary>
    /// Unexpected internal error
    /// </summary>
    UnexpectedError,
    #endregion
}
