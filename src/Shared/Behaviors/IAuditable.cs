namespace Shared.Behaviors;

public interface IAuditable
{
    Guid UserId { get; set; }
}