using FluentAssertions;
using NSubstitute;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Application.Users.Queries.GetUser;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Queries.GetUser;

public class GetUserQueryHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new GetUserQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenExistingUserId_ShouldReturnUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("user@example.com"), FullName.Create("John", "Doe"));

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetUserQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("user@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Role.Should().Be("User");
        result.IsActive.Should().BeTrue();
        result.ManagedFacilityIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_GivenNonExistentUserId_ShouldReturnNull()
    {
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var query = new GetUserQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenUserWithManagedFacilities_ShouldReturnThem()
    {
        var userId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var user = User.Register(userId, Email.Create("manager@example.com"), FullName.Create("Manager", "User"));
        user.ChangeRole(UserRole.Manager);
        user.AssignToFacility(facilityId);

        _userRepository.GetByIdAsync(userId, include: null, asNoTracking: true, ct: Arg.Any<CancellationToken>())
            .Returns(user);

        var query = new GetUserQuery(userId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ManagedFacilityIds.Should().ContainSingle().Which.Should().Be(facilityId);
    }
}
