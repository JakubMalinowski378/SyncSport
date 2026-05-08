using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reservations.Application.Common.Interfaces;
using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Infrastructure.Persistence;
using Shared.Persistence;

namespace Reservations.Infrastructure.Services;

internal sealed class ReservationRepository(ReservationsDbContext dbContext)
    : Repository<Reservation, Guid>(dbContext), IReservationRepository
{
    public async Task<IReadOnlyList<Reservation>> GetMyReservationsAsync(
        Guid userId,
        ReservationFilters filters,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(r => r.UserId == userId);

        if (filters.Status.HasValue)
            query = query.Where(r => r.Status == filters.Status.Value);

        return await query
            .OrderByDescending(r => r.Time.Start)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
