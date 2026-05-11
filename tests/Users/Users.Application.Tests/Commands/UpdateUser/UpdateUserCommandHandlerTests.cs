using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Users.Commands.UpdateUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.UpdateUser;

public class UpdateUserCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new UpdateUserCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenExistingUser_ShouldUpdateName()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("OldFirst", "OldLast"));

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new UpdateUserCommand(userId, "NewFirst", "NewLast");

        await _handler.Handle(command, CancellationToken.None);

        user.Name.FirstName.Should().Be("NewFirst");
        user.Name.LastName.Should().Be("NewLast");
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new UpdateUserCommand(userId, "First", "Last");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }
}
