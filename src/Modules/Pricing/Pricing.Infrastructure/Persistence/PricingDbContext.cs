using Microsoft.EntityFrameworkCore;
using Pricing.Domain.Entities;

namespace Pricing.Infrastructure.Persistence;

public sealed class PricingDbContext(DbContextOptions<PricingDbContext> options) : DbContext(options)
{
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<PriceRule> PriceRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("pricing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PricingDbContext).Assembly);
    }
}