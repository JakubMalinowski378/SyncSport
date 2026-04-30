using Facilities.Shared;
using Microsoft.EntityFrameworkCore;
using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Domain.Entities;
using Reservations.Infrastructure.Persistence;
using Shared.Pagination;
using Shared.Persistence;

namespace Reservations.Infrastructure.Services;

internal sealed class ReservationRepository(
    ReservationsDbContext dbContext,
    IFacilitiesModuleApi facilitiesModuleApi)
    : Repository<Reservation, Guid>(dbContext), IReservationRepository
{
    public async Task<PagedResult<ReservationWithDetailsDto>> GetMyReservationsWithDetailsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.Time.Start);

        var totalCount = await query.CountAsync(cancellationToken);

        var reservations = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var courtIds = reservations.Select(r => r.CourtId).Distinct();
        var courtDetails = await facilitiesModuleApi.GetCourtsWithFacilityByIdsAsync(courtIds, cancellationToken);

        var items = reservations.ConvertAll(r =>
        {
            courtDetails.TryGetValue(r.CourtId, out var details);
            return new ReservationWithDetailsDto(
                r.Id,
                r.CourtId,
                details?.CourtName!,
                details?.FacilityName!,
                r.Time.Start,
                r.Time.End,
                r.Price,
                r.Status);
        });

        return new PagedResult<ReservationWithDetailsDto>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }
}
