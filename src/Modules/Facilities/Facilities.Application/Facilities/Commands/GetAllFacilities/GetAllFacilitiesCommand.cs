using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Commands.GetAllFacilities;

public sealed record GetAllFacilitiesCommand(
    string? SearchTerm = null,
    string? SortColumn = null,
    string? SortOrder = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<GetAllFacilitiesResult>>;

public sealed class GetAllFacilitiesCommandValidator : AbstractValidator<GetAllFacilitiesCommand>
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];   
    private static readonly string[] AllowedSortColumns = ["name", "address"];
    private static readonly string[] AllowedSortOrders = ["asc", "desc"];

    public GetAllFacilitiesCommandValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");    

        RuleFor(x => x.SortColumn)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortColumns.Contains(x.ToLower()))
            .WithMessage("SortColumn must be 'name' or 'address'.");

        RuleFor(x => x.SortOrder)
            .Must(x => string.IsNullOrWhiteSpace(x) || AllowedSortOrders.Contains(x.ToLower()))
            .WithMessage("SortOrder must be 'asc' or 'desc'.");
    }
}

public sealed record GetAllFacilitiesResult(
    Guid Id,
    string Name,
    string Address,
    int ReservationDuration,
    List<DailyOpeningHoursDto> OpeningHours,
    List<string> Images);
