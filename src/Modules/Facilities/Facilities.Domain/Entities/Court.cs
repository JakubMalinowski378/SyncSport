using Facilities.Domain.ValueObjects;
using Shared.Domain;

namespace Facilities.Domain.Entities;

public partial class Court : Entity<CourtId>
{
    private readonly List<ImageUrl> _images = [];

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string SurfaceType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public int? OverrideReservationDuration { get; private set; }
    public WeeklyOpeningHours? OverrideWeeklyOpeningHours { get; private set; }
    public IReadOnlyCollection<ImageUrl> Images => _images.AsReadOnly();
}
