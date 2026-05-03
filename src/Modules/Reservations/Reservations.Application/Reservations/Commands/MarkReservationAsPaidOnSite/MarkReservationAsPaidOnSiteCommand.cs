using FluentValidation;
using MediatR;

namespace Reservations.Application.Reservations.Commands.MarkReservationAsPaidOnSite;

public record MarkReservationAsPaidOnSiteCommand(Guid Id) : IRequest;

internal sealed class MarkReservationAsPaidOnSiteCommandValidator
    : AbstractValidator<MarkReservationAsPaidOnSiteCommand>
{
    public MarkReservationAsPaidOnSiteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Reservation id is required.");
    }
}
