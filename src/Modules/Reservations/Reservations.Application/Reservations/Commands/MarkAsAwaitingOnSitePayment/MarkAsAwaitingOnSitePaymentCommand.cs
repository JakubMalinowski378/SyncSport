using FluentValidation;
using MediatR;

namespace Reservations.Application.Reservations.Commands.MarkAsAwaitingOnSitePayment;

public record MarkAsAwaitingOnSitePaymentCommand(Guid Id) : IRequest;

internal sealed class MarkAsAwaitingOnSitePaymentCommandValidator : AbstractValidator<MarkAsAwaitingOnSitePaymentCommand>
{
    public MarkAsAwaitingOnSitePaymentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}
