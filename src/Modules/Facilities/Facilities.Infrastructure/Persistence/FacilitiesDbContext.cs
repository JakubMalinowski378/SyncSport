using Facilities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Facilities.Infrastructure.Persistence;

public sealed class FacilitiesDbContext(DbContextOptions<FacilitiesDbContext> options) : DbContext(options)
{
    public DbSet<Facility> Facilities {get; set; }
    public DbSet<Court> Courts {get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("facilities");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FacilitiesDbContext).Assembly);
    }
}
