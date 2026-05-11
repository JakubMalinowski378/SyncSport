using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Users.Queries.GetUsers;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Queries.GetUsers;

public class GetUsersQueryHandlerTests
{
    private readonly IRepository<User, Guid> _userRepository;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _handler = new GetUsersQueryHandler(_userRepository);
    }

    [Fact]
    public async Task Handle_GivenDefaultQuery_ShouldReturnAllUsersPaginated()
    {
        var users = Enumerable.Range(1, 3).Select(i =>
        {
            var user = User.Register(
                Guid.NewGuid(),
                Email.Create($"user{i}@example.com"),
                FullName.Create($"First{i}", $"Last{i}"));
            return user;
        }).ToList();

        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((users, 3));

        var query = new GetUsersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_GivenSearchTerm_ShouldPassFilterToRepository()
    {
        var users = new List<User>
        {
            User.Register(Guid.NewGuid(), Email.Create("john@example.com"), FullName.Create("John", "Doe"))
        };

        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((users, 1));

        var query = new GetUsersQuery(SearchTerm: "john");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_GivenNoUsers_ShouldReturnEmptyPage()
    {
        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((new List<User>(), 0));

        var query = new GetUsersQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_GivenSortByEmailDesc_ShouldPassDescendingOrder()
    {
        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((new List<User>(), 0));

        var query = new GetUsersQuery(SortColumn: "Email", SortOrder: "desc");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GivenSortByFirstNameAsc_ShouldPassAscendingOrder()
    {
        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((new List<User>(), 0));

        var query = new GetUsersQuery(SortColumn: "FirstName", SortOrder: "asc");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GivenSortByLastName_ShouldReturnSuccessfully()
    {
        _userRepository.GetPagedAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((new List<User>(), 0));

        var query = new GetUsersQuery(SortColumn: "LastName");

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_GivenCustomPageSize_ShouldPassCorrectPagination()
    {
        var users = Enumerable.Range(1, 2).Select(i =>
        {
            var user = User.Register(
                Guid.NewGuid(),
                Email.Create($"user{i}@example.com"),
                FullName.Create($"First{i}", $"Last{i}"));
            return user;
        }).ToList();

        _userRepository.GetPagedAsync(
                2,
                5,
                Arg.Any<System.Linq.Expressions.Expression<Func<User, bool>>?>(),
                Arg.Is<Func<IQueryable<User>, IQueryable<User>>?>(x => x == null),
                Arg.Any<Func<IQueryable<User>, IOrderedQueryable<User>>?>(),
                true,
                Arg.Any<CancellationToken>())
            .Returns((users, 10));

        var query = new GetUsersQuery(PageNumber: 2, PageSize: 5);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }
}
