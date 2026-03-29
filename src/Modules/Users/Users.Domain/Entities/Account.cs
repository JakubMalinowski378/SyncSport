using Shared.Domain;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class Account : Entity<Guid>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    private Account() { }

    private Account(Guid id, Email email, string passwordHash) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
    }
}
