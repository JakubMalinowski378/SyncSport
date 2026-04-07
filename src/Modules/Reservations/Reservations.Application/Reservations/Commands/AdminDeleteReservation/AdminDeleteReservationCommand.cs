using FluentValidation;
using MediatR;

namespace Reservations.Application.Reservations.Commands.AdminDeleteReservation;

public record AdminDeleteReservationCommand(Guid Id, Guid FacilityId) : IRequest;

internal sealed class AdminDeleteReservationCommandValidator : AbstractValidator<AdminDeleteReservationCommand>
{
    public AdminDeleteReservationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.FacilityId).NotEmpty().WithMessage("FacilityId is required.");
    }
}
