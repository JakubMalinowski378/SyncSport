using Facilities.Domain.ValueObjects;
namespace Facilities.Domain.Entities;

public partial class Facility
{
    private Facility() { }

    public static Facility Create(string name, string address, int reservationDuration, WeeklyOpeningHours weeklyOpeningHours, IEnumerable<DateSpecificOpeningHours>? customDateHours = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Facility address cannot be empty.");
        }

        if (reservationDuration <= 0)
        {
            throw new ArgumentException("Reservation duration must be greater than zero.");
        }

        var facility = new Facility
        {
            Id = FacilityId.New(),
            Name = name,
            Address = address,
            ReservationDuration = reservationDuration,
            WeeklyOpeningHours = weeklyOpeningHours
        };

        if (customDateHours is not null)
        {
            facility._customDateHours.AddRange(customDateHours);
        }

        return facility;
    }

    public Court AddCourt(string name, string surfaceType, int? overrideReservationDuration = null)
    {
        if (_courts.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A court with this name already exists in this facility.");
        }

        var court = Court.Create(name, surfaceType, overrideReservationDuration);
        _courts.Add(court);

        return court;
    }

    public void EditCourt(CourtId courtId, string name, bool isActive, int? overrideReservationDuration = null)
    {
        var court = _courts.FirstOrDefault(c => c.Id == courtId);
        if (court is null)
        {
            throw new InvalidOperationException("Court not found.");
        }

        if (_courts.Any(c => c.Id != courtId && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Another court with this name already exists in this facility.");
        }

        court.Rename(name);
        
        court.ChangeReservationDuration(overrideReservationDuration);

        if (isActive && !court.IsActive)
            court.Activate();
        else if (!isActive && court.IsActive)
            court.Deactivate();
    }

    public void RemoveCourt(CourtId courtId)
    {
        var court = _courts.FirstOrDefault(c => c.Id == courtId);
        if (court is null)
        {
            throw new InvalidOperationException("Court not found for removal.");
        }

        _courts.Remove(court);
    }

    public void ChangeOpeningHours(WeeklyOpeningHours weeklyOpeningHours)
    {
        WeeklyOpeningHours = weeklyOpeningHours;
    }

    public void ChangeCustomDateHours(IEnumerable<DateSpecificOpeningHours> customDateHours)
    {
        _customDateHours.Clear();
        _customDateHours.AddRange(customDateHours);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name cannot be empty.");
        }

        Name = name;
    }

    public void ChangeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Facility address cannot be empty.");
        }

        Address = address;
    }

    public void ChangeReservationDuration(int duration)
    {
        if (duration <= 0)
            throw new ArgumentException("Reservation duration must be greater than zero.");
        
        ReservationDuration = duration;
    }

    public void AddImage(ImageUrl imageUrl)
    {
        if (!_images.Contains(imageUrl))
        {
            _images.Add(imageUrl);
        }
    }

    public void RemoveImage(ImageUrl imageUrl)
    {
        _images.Remove(imageUrl);
    }
}
