using Facilities.Api;
using Users.Api;
using Notifications;
using Shared;
using Shared.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedFramework(builder.Configuration);
builder.Services.AddFacilitiesModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);

var app = builder.Build();

app.UseSharedFramework();
await app.ApplyDatabaseMigrationsAsync();
await app.SeedDataAsync();

app.Run();
