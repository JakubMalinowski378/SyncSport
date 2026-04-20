using Facilities.Domain.ValueObjects;

namespace Facilities.Domain.Entities;

public partial class Court
{
    private Court() { }

    public static Court Create(string name, string surfaceType, int? overrideReservationDuration = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Court name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(surfaceType))
        {
            throw new ArgumentException("Surface type cannot be empty.");
        }

        if (overrideReservationDuration.HasValue && overrideReservationDuration.Value <= 0)
        {
            throw new ArgumentException("Reservation duration must be greater than zero.");
        }

        return new Court
        {
            Id = CourtId.New(),
            Name = name,
            SurfaceType = surfaceType,
            IsActive = true,
            OverrideReservationDuration = overrideReservationDuration
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

    public void ChangeReservationDuration(int? overrideReservationDuration)
    {
        if (overrideReservationDuration.HasValue && overrideReservationDuration.Value <= 0)
        {
            throw new ArgumentException("Reservation duration must be greater than zero.");
        }
        
        OverrideReservationDuration = overrideReservationDuration;
    }
    public WeeklyOpeningHours? GetEffectiveOpeningHours(Facility facility)
    {
        return OverrideWeeklyOpeningHours ?? facility.WeeklyOpeningHours;
    }

    public int GetEffectiveReservationDuration(Facility facility)
    {
        return OverrideReservationDuration ?? facility.ReservationDuration;
    }
}
