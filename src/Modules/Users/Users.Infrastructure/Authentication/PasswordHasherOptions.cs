namespace Users.Infrastructure.Authentication;

public sealed class PasswordHasherOptions
{
    public int WorkFactor { get; init; } = 11;
}