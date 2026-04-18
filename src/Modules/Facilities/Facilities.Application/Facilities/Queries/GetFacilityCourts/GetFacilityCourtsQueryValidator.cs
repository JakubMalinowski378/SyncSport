using FluentValidation;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQueryValidator : AbstractValidator<GetFacilityCourtsQuery>
{
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public GetFacilityCourtsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .Must(x => AllowedPageSizes.Contains(x))
            .WithMessage("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
    }
}
