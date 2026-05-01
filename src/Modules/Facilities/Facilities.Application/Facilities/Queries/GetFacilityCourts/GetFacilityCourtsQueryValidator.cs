using FluentValidation;
using Shared.FluentValidation;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQueryValidator : AbstractValidator<GetFacilityCourtsQuery>
{
    public GetFacilityCourtsQueryValidator()
    {
        this.AddPaginationRules();

        RuleFor(x => x.FacilitySlug)
            .NotEmpty()
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Facility slug is invalid.");
    }
}
