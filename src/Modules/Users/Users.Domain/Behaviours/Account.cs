namespace Users.Domain.Entities;

public partial class Account
{
    public static Account Create(ValueObjects.Email email, string passwordHash)
    {
        return new Account(Guid.NewGuid(), email, passwordHash);
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
    }

    public void SetPasswordResetToken(string resetToken, DateTime expiryTime)
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
}
