using Kairos.Shared.Contracts;
using MediatR;

namespace Kairos.Shared.Abstractions;

/// <summary>
/// Represents a state change request
/// </summary>
public interface ICommand : IRequest<Output>
{
    public Guid CorrelationId { get; }
}

/// <summary>
/// Represents a command that returns a result
/// </summary>
public interface ICommand<TOutput> : IRequest<Output<TOutput>>;