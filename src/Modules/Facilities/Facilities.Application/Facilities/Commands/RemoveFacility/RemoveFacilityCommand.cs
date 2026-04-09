using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.RemoveFacility;

public sealed record RemoveFacilityCommand(Guid FacilityId) : IRequest;

public sealed class RemoveFacilityCommandValidator : AbstractValidator<RemoveFacilityCommand>
{
    public RemoveFacilityCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();
    }
}
