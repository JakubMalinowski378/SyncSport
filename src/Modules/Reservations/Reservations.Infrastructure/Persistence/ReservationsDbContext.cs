using Microsoft.EntityFrameworkCore;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Infrastructure.Persistence;

public sealed class ReservationsDbContext(DbContextOptions<ReservationsDbContext> options)
    : DbContext(options)
{
    public DbSet<Reservation> Reservations { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationsDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
