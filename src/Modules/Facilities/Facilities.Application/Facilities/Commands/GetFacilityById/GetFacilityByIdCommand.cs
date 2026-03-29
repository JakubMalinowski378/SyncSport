using Facilities.Application.Facilities.Common;
using MediatR;

namespace Facilities.Application.Facilities.Commands.GetFacilityById;

public sealed record GetFacilityByIdCommand(Guid FacilityId) : IRequest<GetFacilityByIdResult?>;

public sealed record GetFacilityByIdResult(
    Guid Id,
    string Name,
    string Address,
    List<DailyOpeningHoursDto> OpeningHours);
