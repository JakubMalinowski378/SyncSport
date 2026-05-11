using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Application.Users.Commands.AssignFacilityToUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.AssignFacilityToUser;

public class AssignFacilityToUserCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly AssignFacilityToUserCommandHandler _handler;

    public AssignFacilityToUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new AssignFacilityToUserCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenManagerUser_ShouldAssignFacility()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new AssignFacilityToUserCommand(userId, facilityId);

        await _handler.Handle(command, CancellationToken.None);

        user.ManagedFacilityIds.Should().Contain(facilityId);
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonManagerUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("Regular", "User"));

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new AssignFacilityToUserCommand(userId, facilityId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Only managers can be assigned to a facility.");
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new AssignFacilityToUserCommand(userId, facilityId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }

    [Fact]
    public async Task Handle_GivenManagerAlreadyAssignedToFacility_ShouldNotDuplicate()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);

        user.AssignToFacility(facilityId);

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new AssignFacilityToUserCommand(userId, facilityId);

        await _handler.Handle(command, CancellationToken.None);

        user.ManagedFacilityIds.Should().HaveCount(1);
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
