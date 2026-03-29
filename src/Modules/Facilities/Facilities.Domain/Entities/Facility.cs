using Facilities.Domain.ValueObjects;
using Shared.Domain;

namespace Facilities.Domain.Entities;

public partial class Facility : AggregateRoot<FacilityId>
{
    private readonly List<Court> _courts = [];

    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public WeeklyOpeningHours WeeklyOpeningHours { get; private set; } = default!;
    public IReadOnlyCollection<Court> Courts => _courts.AsReadOnly();
}
