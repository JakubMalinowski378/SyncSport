using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using MediatR;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourtById;

public sealed record GetFacilityCourtByIdQuery(string FacilitySlug, string CourtSlug) : IRequest<CourtDto?>;

