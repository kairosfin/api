using Kairos.Account.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kairos.Account.Infra;

internal sealed class AccountContext : IdentityDbContext<Investor, IdentityRole, string>
{
    public AccountContext(DbContextOptions<AccountContext> options)
        : base(options)
    {
    }

    public DbSet<Investor> Investors { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Investor>(b => b.ToTable("Account"));
        builder.Entity<IdentityRole>(b => b.ToTable("Role"));
        builder.Entity<IdentityUserRole<string>>(b => b.ToTable("AccountRole"));
        builder.Entity<IdentityUserClaim<string>>(b => b.ToTable("AccountClaim"));
        builder.Entity<IdentityUserLogin<string>>(b => b.ToTable("AccountLogin"));
        builder.Entity<IdentityRoleClaim<string>>(b => b.ToTable("RoleClaim"));
        builder.Entity<IdentityUserToken<string>>(b => b.ToTable("AccountToken"));
    }
}
