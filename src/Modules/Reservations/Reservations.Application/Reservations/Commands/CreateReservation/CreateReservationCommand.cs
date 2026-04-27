using FluentValidation;
using Facilities.Shared;
using System.Linq;
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
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;

    public CreateReservationCommandValidator(IFacilitiesModuleApi facilitiesModuleApi)
    {
        _facilitiesModuleApi = facilitiesModuleApi;
        RuleFor(x => x.CourtId)
            .NotEmpty().WithMessage("CourtId is required.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("StartTime is required.")
            .LessThan(x => x.EndTime).WithMessage("StartTime must be before EndTime.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("EndTime is required.")
            .Must((command, endTime) => (endTime - command.StartTime).TotalMinutes is >= 60 and <= 120)
            .WithMessage("Reservation duration must be between 60 and 120 minutes.");

        RuleFor(x => x)
            .MustAsync(async (command, ct) => await IsAlignedWithFacilityAsync(command, ct))
            .WithMessage("Reservation must align with facility opening hours and court reservation duration.");
    }

    private async Task<bool> IsAlignedWithFacilityAsync(CreateReservationCommand command, CancellationToken ct)
    {
        var facilityId = await _facilitiesModuleApi.GetFacilityIdByCourtIdAsync(command.CourtId, ct);
        if (facilityId is null) return false;

        var info = await _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId.Value, ct);
        if (info is null) return false;

        var court = info.Courts.FirstOrDefault(c => c.CourtId == command.CourtId);
        if (court is null) return false;

        var duration = court.ReservationDurationMinutes;

        var start = command.StartTime;
        var end = command.EndTime;

        if ((end - start).TotalMinutes != duration) return false;

        var opening = info.OpeningHours.FirstOrDefault(o => o.DayOfWeek == start.DayOfWeek);
        if (opening is null) return false;

        var open = opening.OpenTime;
        var close = opening.CloseTime;

        var startTime = start.TimeOfDay;
        var endTime = end.TimeOfDay;

        if (startTime < open) return false;
        if (endTime > close) return false;

        var minutesFromOpen = (int)(startTime - open).TotalMinutes;
        return minutesFromOpen % duration == 0;
    }
}

