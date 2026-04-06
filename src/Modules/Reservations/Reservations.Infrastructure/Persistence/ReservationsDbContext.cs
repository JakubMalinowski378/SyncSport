using Microsoft.EntityFrameworkCore;
using Reservations.Domain.Entities;

namespace Reservations.Infrastructure.Persistence;

public sealed class ReservationsDbContext(DbContextOptions<ReservationsDbContext> options) 
    : DbContext(options)
{
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reservations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationsDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
