using Kairos.Account.Domain;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kairos.Account.Infra;

internal sealed class AccountContext : IdentityDbContext<Investor, IdentityRole<long>, long>
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
        builder.Entity<IdentityRole<long>>(b => b.ToTable("Role"));
        builder.Entity<IdentityUserRole<long>>(b => b.ToTable("AccountRole"));
        builder.Entity<IdentityUserClaim<long>>(b => b.ToTable("AccountClaim"));
        builder.Entity<IdentityUserLogin<long>>(b => b.ToTable("AccountLogin"));
        builder.Entity<IdentityRoleClaim<long>>(b => b.ToTable("RoleClaim"));
        builder.Entity<IdentityUserToken<long>>(b => b.ToTable("AccountToken"));

        // Outbox pattern
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
}
