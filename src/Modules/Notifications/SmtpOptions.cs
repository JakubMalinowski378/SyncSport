namespace Notifications;

public sealed record SmtpOptions
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool EnableSsl { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
}
