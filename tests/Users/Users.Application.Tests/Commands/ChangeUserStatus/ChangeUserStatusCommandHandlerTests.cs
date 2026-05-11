using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Users.Commands.ChangeUserStatus;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly ChangeUserStatusCommandHandler _handler;

    public ChangeUserStatusCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new ChangeUserStatusCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenExistingUserAndSetActive_ShouldActivateUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("Test", "User"));
        user.Deactivate();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new ChangeUserStatusCommand(userId, true);

        await _handler.Handle(command, CancellationToken.None);

        user.IsActive.Should().BeTrue();
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenExistingUserAndSetInactive_ShouldDeactivateUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("Test", "User"));

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new ChangeUserStatusCommand(userId, false);

        await _handler.Handle(command, CancellationToken.None);

        user.IsActive.Should().BeFalse();
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new ChangeUserStatusCommand(userId, true);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }
}
