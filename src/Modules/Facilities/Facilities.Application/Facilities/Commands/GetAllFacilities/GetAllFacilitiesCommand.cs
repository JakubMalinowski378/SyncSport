using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Commands.GetAllFacilities;

public sealed record GetAllFacilitiesCommand(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<GetAllFacilitiesResult>>;

public sealed class GetAllFacilitiesCommandValidator : AbstractValidator<GetAllFacilitiesCommand>
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public GetAllFacilitiesCommandValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
    }
}

public sealed record GetAllFacilitiesResult(
    Guid Id,
    string Name,
    string Address,
    List<DailyOpeningHoursDto> OpeningHours);
