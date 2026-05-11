using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Users.Commands.UpdateCurrentUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;
using Users.Shared;

namespace Users.Application.Tests.Commands.UpdateCurrentUser;

public class UpdateCurrentUserCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly ICurrentUser _currentUser;
    private readonly UpdateCurrentUserCommandHandler _handler;

    public UpdateCurrentUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _currentUser = Substitute.For<ICurrentUser>();
        _handler = new UpdateCurrentUserCommandHandler(_userRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_GivenAuthenticatedUser_ShouldUpdateName()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("OldFirst", "OldLast"));

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@example.com", "User", [], true));
        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new UpdateCurrentUserCommand("NewFirst", "NewLast");

        await _handler.Handle(command, CancellationToken.None);

        user.Name.FirstName.Should().Be("NewFirst");
        user.Name.LastName.Should().Be("NewLast");
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenUnauthenticatedUser_ShouldThrowException()
    {
        _currentUser.GetState().Returns((CurrentUserState?)null);

        var command = new UpdateCurrentUserCommand("First", "Last");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User is not authenticated");
    }

    [Fact]
    public async Task Handle_GivenAuthenticatedButUserNotFound_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@example.com", "User", [], true));
        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateCurrentUserCommand("First", "Last");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }

    [Fact]
    public async Task Handle_GivenNotAuthenticatedFlag_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _currentUser.GetState().Returns(new CurrentUserState(userId, "test@example.com", "User", [], false));

        var command = new UpdateCurrentUserCommand("First", "Last");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("User is not authenticated");
    }
}
