namespace Facilities.Domain.Entities;

public partial class Court : Entity<CourtId>
{
    public string Name { get; private set; } = string.Empty;
    public string SurfaceType { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
}
