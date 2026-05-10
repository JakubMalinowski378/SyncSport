using Facilities.Domain.ValueObjects;
using Facilities.Shared.Events;
namespace Facilities.Domain.Entities;

public partial class Facility
{
    private Facility() { }

    public static Facility Create(string name, string slug, string address, int reservationDuration, WeeklyOpeningHours weeklyOpeningHours, IEnumerable<DateSpecificOpeningHours>? customDateHours = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Facility slug cannot be empty.");
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
            Slug = slug,
            Address = address,
            ReservationDuration = reservationDuration,
            WeeklyOpeningHours = weeklyOpeningHours
        };

        if (customDateHours is not null)
        {
            facility._customDateHours.AddRange(customDateHours);
        }

        facility.AddDomainEvent(new FacilityCreatedEvent(facility.Id.Value));

        return facility;
    }

    public Court AddCourt(string name, string slug, string surfaceType, int? overrideReservationDuration = null)
    {
        if (_courts.Any(c => c.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A court with this slug already exists in this facility.");
        }

        var court = Court.Create(name, slug, surfaceType, overrideReservationDuration);
        _courts.Add(court);

        return court;
    }

    public void EditCourt(CourtId courtId, string name, string slug, bool isActive, int? overrideReservationDuration = null, List<ImageUrl>? newImages = null)
    {
        var court = _courts.FirstOrDefault(c => c.Id == courtId);
        if (court is null)
        {
            throw new InvalidOperationException("Court not found.");
        }

        court.ChangeSlug(slug);
        court.Rename(name);

        court.ChangeReservationDuration(overrideReservationDuration);

        if (isActive && !court.IsActive)
            court.Activate();
        else if (!isActive && court.IsActive)
            court.Deactivate();

        if (newImages is null) return;

        var currentImages = court.Images.ToList();
        foreach (var img in currentImages)
        {
            court.RemoveImage(img);
        }

        foreach (var newImg in newImages)
        {
            court.AddImage(newImg);
        }
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

    public void ChangeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Facility slug cannot be empty.");
        }

        Slug = slug;
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
        var existingIndex = _images.FindIndex(x => x.Value == imageUrl.Value);

        if (existingIndex >= 0)
        {
            _images[existingIndex] = imageUrl;
        }
        else
        {
            _images.Add(imageUrl);
        }
    }

    public void RemoveImage(ImageUrl imageUrl)
    {
        _images.Remove(imageUrl);
    }
}
