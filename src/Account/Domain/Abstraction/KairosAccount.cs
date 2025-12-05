using Microsoft.AspNetCore.Identity;

namespace Kairos.Account.Domain.Abstraction;

public abstract class KairosAccount : IdentityUser<long>;
