using Facilities.Domain.ValueObjects;

namespace Facilities.Domain.Entities;

public partial class Court
{
    private Court() { }

    public static Court Create(string name, string surfaceType)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Court name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(surfaceType))
        {
            throw new ArgumentException("Surface type cannot be empty.");
        }

        return new Court
        {
            Id = CourtId.New(),
            Name = name,
            SurfaceType = surfaceType,
            IsActive = true
        };
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Court name cannot be empty.");
        }

        Name = name;
    }
}
