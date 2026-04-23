using FluentAssertions;
using Users.Application.Accounts.Commands.SignUp;

namespace Users.Application.Tests.Commands.SignUp;

public class SignUpCommandValidatorTests
{
    private readonly SignUpCommandValidator _validator;

    public SignUpCommandValidatorTests()
    {
        _validator = new SignUpCommandValidator();
    }

    [Fact]
    public void Validate_GivenInvalidEmailFormat_ShouldHaveValidationError_TC004()
    {
        // Arrange
        var command = new SignUpCommand("invalid-email", "ValidP@ss123", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SignUpCommand.Email));
    }

    [Fact]
    public void Validate_GivenWeakPassword_ShouldHaveValidationError_TC005()
    {
        // Arrange
        var command = new SignUpCommand("test@example.com", "weakpassword", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SignUpCommand.Password));
    }

    [Fact]
    public void Validate_GivenMissingRequiredFields_ShouldHaveValidationError_TC006()
    {
        // Arrange
        var command = new SignUpCommand("test@example.com", "", "", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SignUpCommand.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SignUpCommand.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SignUpCommand.LastName));
    }
}