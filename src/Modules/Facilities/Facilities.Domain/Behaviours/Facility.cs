using Facilities.Domain.ValueObjects;
namespace Facilities.Domain.Entities;

public partial class Facility
{
    private Facility() { }

    public static Facility Create(string name, string address, OpeningHours openingHours)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Facility name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Facility address cannot be empty.");
        }

        return new Facility
        {
            Id = FacilityId.New(),
            Name = name,
            Address = address,
            OpeningHours = openingHours
        };
    }

    public Court AddCourt(string name, string surfaceType)
    {
        if (_courts.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("A court with this name already exists in this facility.");
        }

        var court = Court.Create(name, surfaceType);
        _courts.Add(court);

        return court;
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

    public void ChangeOpeningHours(OpeningHours openingHours)
    {
        OpeningHours = openingHours;
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
}
