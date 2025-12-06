using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.Account.GetAccountInfo;

namespace Kairos.Shared.Contracts.Account;

public sealed record GetAccountQuery(
    long Id,
    Guid CorrelationId) : IQuery<AccountInfo>
{
    public GetAccountQuery(long id) : this(id, Guid.NewGuid()) { }
}