using Kairos.Shared.Abstractions;
using Kairos.Shared.Contracts.Account.GetAccountInfo;

namespace Kairos.Shared.Contracts.Account;

public sealed record GetAccountInfoQuery(
    long Id,
    Guid CorrelationId) : IQuery<AccountInfo>
{
    public GetAccountInfoQuery(long id) : this(id, Guid.NewGuid()) { }
}