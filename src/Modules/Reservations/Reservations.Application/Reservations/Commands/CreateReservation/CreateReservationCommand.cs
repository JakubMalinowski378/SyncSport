using FluentValidation;
using MediatR;
using Shared.Behaviors;
using System.Text.Json.Serialization;

namespace Reservations.Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid CourtId,
    DateTime StartTime,
    DateTime EndTime,
    bool PayOnSite = false)
    : IRequest<Guid>, IAuditable
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}

internal sealed class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.CourtId)
            .NotEmpty().WithMessage("CourtId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.")
            .LessThan(x => x.EndTime).WithMessage("StartTime must be before EndTime.")
            .Must(x => x > DateTime.UtcNow).WithMessage("Reservation cannot start in the past.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.");
    }
}

