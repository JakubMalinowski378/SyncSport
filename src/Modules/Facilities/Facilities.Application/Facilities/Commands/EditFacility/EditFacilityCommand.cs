using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed record EditFacilityCommand(
    Guid FacilityId,
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime) : IRequest<bool>;

public sealed class EditFacilityCommandValidator : AbstractValidator<EditFacilityCommand>
{
    public EditFacilityCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.OpenTime)
            .LessThan(x => x.CloseTime)
            .WithMessage("OpenTime must be before CloseTime.");

        RuleFor(x => x.CloseTime)
            .GreaterThan(x => x.OpenTime)
            .WithMessage("CloseTime must be after OpenTime.");
    }
}
