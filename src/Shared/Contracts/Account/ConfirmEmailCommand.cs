using System;
using Kairos.Shared.Abstractions;

namespace Kairos.Shared.Contracts.Account;

public sealed record ConfirmEmailCommand(
    long AccountId,
    string ConfirmationToken,
    Guid CorrelationId) : ICommand;
