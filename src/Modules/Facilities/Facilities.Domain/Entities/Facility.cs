using Facilities.Domain.ValueObjects;
using Shared.Domain;

namespace Facilities.Domain.Entities;

public partial class Facility : AggregateRoot<FacilityId>
{
    private readonly List<Court> _courts = [];
    private readonly List<DateSpecificOpeningHours> _customDateHours = [];
    private readonly List<ImageUrl> _images = [];

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public int ReservationDuration { get; private set; }
    public WeeklyOpeningHours WeeklyOpeningHours { get; private set; } = default!;
    public IReadOnlyCollection<Court> Courts => _courts.AsReadOnly();
    public IReadOnlyCollection<DateSpecificOpeningHours> CustomDateHours => _customDateHours.AsReadOnly();
    public IReadOnlyCollection<ImageUrl> Images => _images.AsReadOnly();
}
