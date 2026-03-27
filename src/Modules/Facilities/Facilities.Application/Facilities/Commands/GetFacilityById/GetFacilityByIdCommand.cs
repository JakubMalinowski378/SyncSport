using MediatR;

namespace Facilities.Application.Facilities.Commands.GetFacilityById;

public sealed record GetFacilityByIdCommand(Guid FacilityId) : IRequest<GetFacilityByIdResult?>;

public sealed record GetFacilityByIdResult(
    Guid Id,
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime);
