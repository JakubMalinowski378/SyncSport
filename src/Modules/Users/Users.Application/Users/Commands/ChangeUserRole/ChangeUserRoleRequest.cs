using Shared.Domain.Enums;

namespace Users.Application.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleRequest(UserRole Role);
