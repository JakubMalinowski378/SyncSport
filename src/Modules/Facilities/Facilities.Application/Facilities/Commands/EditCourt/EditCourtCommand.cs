using Facilities.Application.Facilities.Common;
using FluentValidation;
using MediatR;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed record EditCourtCommand(
    Guid FacilityId,
    Guid CourtId,
    string Name,
    bool IsActive,
    int? OverrideReservationDuration = null,
    List<ImageDto>? Images = null) : IRequest;

public sealed class EditCourtCommandValidator : AbstractValidator<EditCourtCommand>
{
    public EditCourtCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .NotEmpty();

        RuleFor(x => x.CourtId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.OverrideReservationDuration)
            .GreaterThan(0)
            .When(x => x.OverrideReservationDuration.HasValue)
            .WithMessage("Override reservation duration must be greater than zero if provided.");

        RuleFor(x => x.Images)
            .Must(images => images is null || images.Count(i => i.IsMain) <= 1)
            .WithMessage("Only one image can be marked as main.");
    }
}