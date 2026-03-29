namespace Users.Domain.Entities;

public partial class Account
{
    public static Account Create(ValueObjects.Email email, string passwordHash)
    {
        return new Account(Guid.NewGuid(), email, passwordHash);
    }
}
