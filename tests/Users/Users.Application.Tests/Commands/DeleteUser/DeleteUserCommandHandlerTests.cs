using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Users.Commands.DeleteUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.DeleteUser;

public class DeleteUserCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new DeleteUserCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenExistingUser_ShouldDeleteUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("Test", "User"));

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new DeleteUserCommand(userId);

        await _handler.Handle(command, CancellationToken.None);

        _userRepository.Received(1).Remove(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new DeleteUserCommand(userId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }
}
