using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;
using Shared.FluentValidation;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Queries.GetAllFacilities;

public sealed record GetAllFacilitiesCommand(
    string? SearchTerm = null,
    string? SortColumn = null,
    string? SortOrder = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? ManagedFacilityIds = null) : IRequest<PagedResult<GetAllFacilitiesResult>>, IPaginatedRequest;

public sealed class GetAllFacilitiesCommandValidator : AbstractValidator<GetAllFacilitiesCommand>
{
    private static readonly string[] AllowedSortColumns = ["name", "slug", "address"];
    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetAllFacilitiesCommandValidator()
    {
        this.AddPaginationRules();

        RuleFor(x => x.SortColumn)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortColumns.Contains(x.ToLower()))
            .WithMessage("SortColumn must be 'name', 'slug' or 'address'.");

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortOrders.Contains(x.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}

public sealed record GetAllFacilitiesResult(
    Guid Id,
    string Name,
    string Slug,
    string Address,
    int ReservationDuration,
    List<DailyOpeningHoursDto> OpeningHours,
    List<ImageDto> Images);
