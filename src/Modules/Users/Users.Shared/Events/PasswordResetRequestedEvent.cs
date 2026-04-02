using Shared.Domain;

namespace Users.Shared.Events;

public record PasswordResetRequestedEvent(Guid UserId, string Email, string FirstName, string ResetToken) : IDomainEvent;
