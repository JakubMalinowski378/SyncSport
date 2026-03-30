using Facilities.Api;
using Users.Api;
using Shared;
using Shared.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedFramework();
builder.Services.AddFacilitiesModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

var app = builder.Build();

app.UseSharedFramework();
await app.ApplyDatabaseMigrationsAsync();
await app.SeedDataAsync();

app.Run();
