using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.CreateCourt;

public sealed record CreateCourtCommand(
    Guid FacilityId,
    string Name,
    string SurfaceType) : IRequest<Guid>;

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
    }
}
