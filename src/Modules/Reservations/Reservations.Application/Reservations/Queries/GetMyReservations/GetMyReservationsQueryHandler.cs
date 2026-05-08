using Facilities.Shared;
using MediatR;
using Reservations.Application.Common.Interfaces;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetMyReservations;

internal sealed class GetMyReservationsQueryHandler(
    IReservationRepository reservationRepository,
    IFacilitiesModuleApi facilitiesModuleApi)
    : IRequestHandler<GetMyReservationsQuery, PagedResult<ReservationWithDetailsDto>>
{
    public async Task<PagedResult<ReservationWithDetailsDto>> Handle(
        GetMyReservationsQuery request,
        CancellationToken cancellationToken)
    {
        var filters = new ReservationFilters(request.Status);

        var reservations = await reservationRepository.GetMyReservationsAsync(
            request.UserId,
            filters,
            cancellationToken);

        if (reservations.Count == 0)
            return new PagedResult<ReservationWithDetailsDto>([], 0, request.PageNumber, request.PageSize);

        var courtIds = reservations.Select(r => r.CourtId).Distinct();
        var courtDetails = await facilitiesModuleApi.GetCourtsWithFacilityByIdsAsync(courtIds, cancellationToken);

        var enriched = reservations
            .Select(r =>
            {
                courtDetails.TryGetValue(r.CourtId, out var details);
                return new ReservationWithDetailsDto(
                    r.Id,
                    r.CourtId,
                    details?.CourtName ?? "Unknown",
                    details?.FacilityName ?? "Unknown",
                    r.Time.Start,
                    r.Time.End,
                    r.Price,
                    r.Status);
            })
            .AsEnumerable();

        // Filter by court/facility name (in memory, since names come from Facilities API)
        if (!string.IsNullOrWhiteSpace(request.CourtName))
            enriched = enriched.Where(r => r.CourtName.Contains(request.CourtName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(request.FacilityName))
            enriched = enriched.Where(r => r.FacilityName.Contains(request.FacilityName, StringComparison.OrdinalIgnoreCase));

        // Sort
        enriched = ApplySorting(enriched, request.SortBy, request.SortDirection);

        var items = enriched.ToList();
        var totalCount = items.Count;

        var pagedItems = items
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<ReservationWithDetailsDto>(
            pagedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static IEnumerable<ReservationWithDetailsDto> ApplySorting(
        IEnumerable<ReservationWithDetailsDto> source,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, SortDirection.Desc, StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLowerInvariant()) switch
        {
            GetMyReservationsSortFields.Date => descending
                ? source.OrderByDescending(r => r.StartTime)
                : source.OrderBy(r => r.StartTime),
            GetMyReservationsSortFields.Status => descending
                ? source.OrderByDescending(r => r.Status)
                : source.OrderBy(r => r.Status),
            GetMyReservationsSortFields.Price => descending
                ? source.OrderByDescending(r => r.Price)
                : source.OrderBy(r => r.Price),
            _ => source.OrderByDescending(r => r.StartTime) // default: date desc
        };
    }
}
