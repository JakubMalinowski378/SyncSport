using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Domain.Enums;
using Shared.Persistence;
using Shared.Seeding;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Seeding;

internal sealed class UsersSeeder(
    IRepository<User, Guid> userRepository,
    IRepository<Account, Guid> accountRepository,
    IPasswordHasher passwordHasher,
    ILogger<UsersSeeder> logger,
    IWebHostEnvironment environment)
    : IDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        if (await userRepository.AnyAsync(ct: cancellationToken))
        {
            logger.LogInformation("Users already seeded.");
            return;
        }

        var usersToSeed = new List<(Email Email, FullName Name, UserRole Role)>
        {
            (Email.Create("user@syncsport.com"), FullName.Create("Client", "User"), UserRole.User),
            (Email.Create("manager@syncsport.com"), FullName.Create("Manager", "User"), UserRole.Manager),
            (Email.Create("admin@syncsport.com"), FullName.Create("Admin", "User"), UserRole.Admin)
        };

        var hashedPassword = passwordHasher.Hash("Password123!");

        foreach (var (email, name, role) in usersToSeed)
        {
            var id = Guid.NewGuid();
            var account = Account.Create(id, email, hashedPassword);

            var user = User.Register(id, email, name);
            if (role != UserRole.User)
            {
                user.ChangeRole(role);
            }

            await accountRepository.AddAsync(account, cancellationToken);
            await userRepository.AddAsync(user, cancellationToken);
        }

        await userRepository.SaveChangesAsync(cancellationToken);
        await accountRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully seeded default users.");
    }
}