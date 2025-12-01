using System;
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

    // protected override void OnModelCreating(ModelBuilder builder)
    // {
        // base.OnModelCreating(builder);

        // additional model configuration if needed
        // e.g. builder.Entity<Investor>(b => { ... });
    // }
}
