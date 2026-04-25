using Facilities.Domain.ValueObjects;

namespace Facilities.Domain.Entities;

public partial class Court
{
    private Court() { }

    public static Court Create(string name, string slug, string surfaceType, int? overrideReservationDuration = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Court name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Court slug cannot be empty.");
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
            Slug = slug,
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

    public void ChangeSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentException("Court slug cannot be empty.");
        }

        Slug = slug;
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

        if (imageUrl.IsMain)
        {
            for (var i = 0; i < _images.Count; i++)
            {
                if (_images[i].Value != imageUrl.Value && _images[i].IsMain)
                {
                    _images[i] = ImageUrl.Create(_images[i].Value, false);
                }
            }
        }
    }

    public void RemoveImage(ImageUrl imageUrl)
    {
        _images.Remove(imageUrl);
    }
}
