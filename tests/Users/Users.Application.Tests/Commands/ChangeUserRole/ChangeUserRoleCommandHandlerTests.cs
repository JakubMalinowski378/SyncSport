using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Application.Users.Commands.ChangeUserRole;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.ChangeUserRole;

public class ChangeUserRoleCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly ChangeUserRoleCommandHandler _handler;

    public ChangeUserRoleCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new ChangeUserRoleCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenExistingUser_ShouldChangeRole()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("Regular", "User"));

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new ChangeUserRoleCommand(userId, UserRole.Manager);

        await _handler.Handle(command, CancellationToken.None);

        user.Role.Should().Be(UserRole.Manager);
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new ChangeUserRoleCommand(userId, UserRole.Admin);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }

    [Fact]
    public async Task Handle_GivenSameRole_ShouldDoNothing()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("admin@example.com"), FullName.Create("Admin", "User"));
        user.ChangeRole(UserRole.Admin);

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new ChangeUserRoleCommand(userId, UserRole.Admin);

        await _handler.Handle(command, CancellationToken.None);

        user.Role.Should().Be(UserRole.Admin);
        _userRepository.DidNotReceive().Update(Arg.Any<User>());
        await _userRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
