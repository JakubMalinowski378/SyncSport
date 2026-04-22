using MediatR;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed record EditCourtCommand(
    Guid FacilityId,
    Guid CourtId,
    string Name,
    bool IsActive,
    int? OverrideReservationDuration = null,
    List<string>? Images = null) : IRequest;