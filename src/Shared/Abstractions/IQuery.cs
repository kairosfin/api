using Kairos.Shared.Contracts;
using MediatR;

namespace Kairos.Shared.Abstractions;

/// <summary>
/// Represents a data retrieval request
/// </summary>
public interface IQuery<TOutput> : IRequest<Output<TOutput>>
{
    public Guid CorrelationId { get; }
}
