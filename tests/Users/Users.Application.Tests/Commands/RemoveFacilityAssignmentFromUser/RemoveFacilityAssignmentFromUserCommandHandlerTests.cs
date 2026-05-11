using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Application.Users.Commands.RemoveFacilityAssignmentFromUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Commands.RemoveFacilityAssignmentFromUser;

public class RemoveFacilityAssignmentFromUserCommandHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly RemoveFacilityAssignmentFromUserCommandHandler _handler;

    public RemoveFacilityAssignmentFromUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new RemoveFacilityAssignmentFromUserCommandHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenAssignedFacility_ShouldRemoveIt()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);
        user.AssignToFacility(facilityId);

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new RemoveFacilityAssignmentFromUserCommand(userId, facilityId);

        await _handler.Handle(command, CancellationToken.None);

        user.ManagedFacilityIds.Should().NotContain(facilityId);
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenNonExistentUser_ShouldThrowException()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new RemoveFacilityAssignmentFromUserCommand(userId, facilityId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }

    [Fact]
    public async Task Handle_GivenFacilityNotAssigned_ShouldRemoveGracefully()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);

        _userRepository.GetByIdAsync(userId, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var command = new RemoveFacilityAssignmentFromUserCommand(userId, facilityId);

        await _handler.Handle(command, CancellationToken.None);

        user.ManagedFacilityIds.Should().BeEmpty();
        _userRepository.Received(1).Update(user);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
