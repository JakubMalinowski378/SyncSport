using FluentValidation;
using MediatR;

namespace Reservations.Application.Reservations.Commands.AdminCreateReservation;

public record AdminCreateReservationCommand(
    Guid UserId,
    Guid FacilityId,
    Guid CourtId,
    DateTime StartTime,
    DateTime EndTime)
    : IRequest<Guid>;

internal sealed class AdminCreateReservationCommandValidator
    : AbstractValidator<AdminCreateReservationCommand>
{
    public AdminCreateReservationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.FacilityId)
            .NotEmpty().WithMessage("FacilityId is required.");

        RuleFor(x => x.CourtId)
            .NotEmpty().WithMessage("CourtId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.")
            .LessThan(x => x.EndTime).WithMessage("StartTime must be before EndTime.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.")
            .Must((command, endTime) => (endTime - command.StartTime).TotalMinutes is >= 60 and <= 120)
            .WithMessage("Reservation duration must be between 60 and 120 minutes.");
    }
}
