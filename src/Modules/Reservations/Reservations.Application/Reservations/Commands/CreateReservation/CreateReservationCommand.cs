using FluentValidation;
using MediatR;
using Shared.Behaviors;
using System.Text.Json.Serialization;

namespace Reservations.Application.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    Guid CourtId,
    DateTime StartTime,
    DateTime EndTime)
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
            .LessThan(x => x.EndTime).WithMessage("StartTime must be before EndTime.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.")
            .Must((command, endTime) => (endTime - command.StartTime).TotalMinutes is >= 60 and <= 120)
            .WithMessage("Reservation duration must be between 60 and 120 minutes.");
    }
}

