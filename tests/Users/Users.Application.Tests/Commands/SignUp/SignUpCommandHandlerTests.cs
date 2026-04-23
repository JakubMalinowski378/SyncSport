using FluentAssertions;
using NSubstitute;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Application.Accounts.Commands.SignUp;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;
using System.Linq.Expressions;
using Users.Application.Accounts.Common;

namespace Users.Application.Tests.Commands.SignUp;

public class SignUpCommandHandlerTests
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRepository<User, Guid> _userRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IJwtService _jwtService;
    private readonly SignUpCommandHandler _handler;

    public SignUpCommandHandlerTests()
    {
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _userRepository = Substitute.For<IRepository<User, Guid>>();
        _accountRepository = Substitute.For<IRepository<Account, Guid>>();
        _jwtService = Substitute.For<IJwtService>();

        _handler = new SignUpCommandHandler(
            _passwordHasher,
            _userRepository,
            _accountRepository,
            _jwtService);
    }

    [Fact]
    public async Task Handle_GivenValidUserData_ShouldReturn200OkEquivalent_TC001()
    {
        // Arrange
        var command = new SignUpCommand("newuser@example.com", "ValidP@ss123", "John", "Doe");

        _accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.Hash(command.Password).Returns("hashed-password");

        var tokenResponse = new AuthenticationResponse("jwt-token", "refresh-token");
        
        _jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-token");
        _jwtService.GenerateRefreshToken().Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JwtToken.Should().Be("jwt-token");
        result.RefreshToken.Should().Be("refresh-token");

        await _accountRepository.Received(1).AddAsync(Arg.Is<Account>(a => a.Email.Value == command.Email), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => u.Email.Value == command.Email), Arg.Any<CancellationToken>());
        await _accountRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GivenMinimumRequiredFields_ShouldReturn200OkEquivalent_TC002()
    {
        var command = new SignUpCommand("minimal@example.com", "ValidP@ss123", "First", "Last");

        _accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        _passwordHasher.Hash(command.Password).Returns("hashed-password");

        _jwtService.GenerateAccessToken(Arg.Any<User>()).Returns("jwt-token-minimal");
        _jwtService.GenerateRefreshToken().Returns(new RefreshTokenResult("refresh-token-minimal", DateTime.UtcNow.AddDays(7)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JwtToken.Should().Be("jwt-token-minimal");
        result.RefreshToken.Should().Be("refresh-token-minimal");
    }

    [Fact]
    public async Task Handle_GivenExistingEmail_ShouldThrowEmailAlreadyTakenException_WhichMpsTo400BadRequest_TC003()
    {
        // Arrange
        var command = new SignUpCommand("newuser@example.com", "ValidP@ss123", "John", "Doe");

        _accountRepository.AnyAsync(Arg.Any<Expression<Func<Account, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true); // Email exists

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EmailAlreadyTakenException>();
    }
}