using Shared.Domain;
using Shared.Domain.Enums;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; }
    public FullName Name { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Guid> _managedFacilityIds = [];
    public IReadOnlyCollection<Guid> ManagedFacilityIds => _managedFacilityIds.AsReadOnly();

    private User() { }

    private User(Guid id, Email email, FullName name, UserRole role) : base(id)
    {
        Email = email;
        Name = name;
        Role = role;
        IsActive = true;
    }
}
