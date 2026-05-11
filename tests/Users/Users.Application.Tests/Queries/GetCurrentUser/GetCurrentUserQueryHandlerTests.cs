using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Application.Users.Queries.GetCurrentUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new GetCurrentUserQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenAuthenticatedUser_ShouldReturnCurrentUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("john@example.com"), FullName.Create("John", "Doe"));

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetCurrentUserQuery { UserId = userId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be("john@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be("User");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_GivenUserNotFound_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetCurrentUserQuery { UserId = userId };

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"User with id {userId} not found");
    }

    [Fact]
    public async Task Handle_GivenUserWithManagedFacilities_ShouldIncludeThem()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);
        user.AssignToFacility(facilityId);

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetCurrentUserQuery { UserId = userId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.ManagedFacilityIds.Should().ContainSingle().Which.Should().Be(facilityId);
    }
}
