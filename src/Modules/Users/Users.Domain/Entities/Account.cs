using Shared.Domain;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public partial class Account : Entity<Guid>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiryTime { get; private set; }
}
