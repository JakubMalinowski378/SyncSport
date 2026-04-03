using FluentValidation;
using MediatR;
using Shared.Behaviors;
using System.Text.Json.Serialization;

namespace Users.Application.Users.Commands.UpdateCurrentUser;

public sealed record UpdateCurrentUserCommand(
    string FirstName,
    string LastName)
    : IRequest, IAuditable
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}

public sealed class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
