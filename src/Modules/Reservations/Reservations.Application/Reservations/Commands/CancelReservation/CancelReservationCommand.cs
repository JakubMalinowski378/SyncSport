using FluentValidation;
using MediatR;

namespace Reservations.Application.Reservations.Commands.CancelReservation;

public record CancelReservationCommand(Guid Id) : IRequest;

internal sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
