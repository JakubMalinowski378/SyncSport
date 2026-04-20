using Shared.Domain.Enums;
using Shared.Domain.Exceptions;
using Users.Domain.ValueObjects;
using Users.Shared.Events;

namespace Users.Domain.Entities;

public partial class User
{
    private User() { }

    private User(Guid id, Email email, FullName name, UserRole role) : base(id)
    {
        Email = email;
        Name = name;
        Role = role;
        IsActive = true;
    }
    public static User Register(Guid id, Email email, FullName name)
    {
        var user = new User(id, email, name, UserRole.User);

        user.AddDomainEvent(new UserRegisteredEvent(user.Id, user.Email.Value));

        return user;
    }

    public void AssignToFacility(Guid facilityId)
    {
        if (Role != UserRole.Manager)
            throw new DomainException("Only managers can be assigned to a facility.");

        if (!_managedFacilityIds.Contains(facilityId))
        {
            _managedFacilityIds.Add(facilityId);
        }
    }

    public void RemoveFacilityAssignment(Guid facilityId)
    {
        _managedFacilityIds.Remove(facilityId);
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new UserDeactivatedEvent(Id));
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new UserActivatedEvent(Id));
    }

    public void UpdateName(FullName name)
    {
        Name = name;
    }

    public void RequestPasswordReset(string resetToken)
    {
        AddDomainEvent(new PasswordResetRequestedEvent(Id, Email.Value, Name.FirstName, resetToken));
    }

    public void ChangeRole(UserRole role)
    {
        if (Role == role)
        {
            return;
        }

        Role = role;
    }
}
