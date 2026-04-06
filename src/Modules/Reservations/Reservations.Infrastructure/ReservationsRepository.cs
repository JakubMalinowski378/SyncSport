using Reservations.Infrastructure.Persistence;
using Shared.Domain;
using Shared.Persistence;

namespace Reservations.Infrastructure;

internal sealed class ReservationsRepository<TEntity, TId>(ReservationsDbContext context)
    : Repository<TEntity, TId>(context)
    where TEntity : Entity<TId>;