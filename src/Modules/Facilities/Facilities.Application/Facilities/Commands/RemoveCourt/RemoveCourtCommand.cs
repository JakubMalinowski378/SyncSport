using MediatR;

namespace Facilities.Application.Facilities.Commands.RemoveCourt;

public sealed record RemoveCourtCommand(Guid FacilityId, Guid CourtId) : IRequest;
