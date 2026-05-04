using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class Account
{
    private Account() { }

    private Account(Guid id, Email email, string passwordHash) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
    }

    public static Account Create(Guid id, Email email, string passwordHash)
    {
        return new Account(id, email, passwordHash);
    }

    public void SetRefreshToken(string refreshToken, DateTimeOffset expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void SetPasswordResetToken(string resetToken, DateTimeOffset expiryTime)
    {
        PasswordResetToken = resetToken;
        PasswordResetTokenExpiryTime = expiryTime;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiryTime = null;
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
