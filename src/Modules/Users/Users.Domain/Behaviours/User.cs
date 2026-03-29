using Shared.Domain.Exceptions;
using Users.Domain.Enums;
using Users.Shared.Events;

namespace Users.Domain.Entities;

public partial class User
{
    public static User Register(Guid id, ValueObjects.Email email, ValueObjects.FullName name, UserRole role)
    {
        var user = new User(id, email, name, role);

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value));

        return user;
    }

    public void AssignToFacility(Guid facilityId)
    {
        if (Role != UserRole.FacilityManager)
            throw new DomainException("Only managers can be assigned to a facility.");

        if (!_managedFacilityIds.Contains(facilityId))
        {
            _managedFacilityIds.Add(facilityId);
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
    }
}
