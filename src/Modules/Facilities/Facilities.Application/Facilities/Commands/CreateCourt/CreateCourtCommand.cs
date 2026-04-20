using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.CreateCourt;

public sealed record CreateCourtCommand(
    Guid FacilityId,
    string Name,
    string SurfaceType,
    int? OverrideReservationDuration = null) : IRequest<Guid>;

public sealed class CreateCourtCommandValidator : AbstractValidator<CreateCourtCommand>
{
    public CreateCourtCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty()
            .WithMessage("Facility ID cannot be empty.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Court name cannot be empty and must not exceed 100 characters.");

        RuleFor(x => x.SurfaceType)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Surface type cannot be empty and must not exceed 50 characters.");

        RuleFor(x => x.OverrideReservationDuration)
            .GreaterThan(0)
            .When(x => x.OverrideReservationDuration.HasValue)
            .WithMessage("Override reservation duration must be greater than zero if provided.");
    }
}
