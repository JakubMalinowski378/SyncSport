using Facilities.Api;
using Notifications;
using Pricing.Api;
using Reservations.Api;
using Shared;
using Shared.Persistence;
using Users.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedFramework(builder.Configuration);
builder.Services.AddFacilitiesModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddNotificationsModule(builder.Configuration);
builder.Services.AddReservationsModule(builder.Configuration);
builder.Services.AddPricingModule(builder.Configuration);

var app = builder.Build();

app.UseSharedFramework();
await app.ApplyDatabaseMigrationsAsync();
await app.SeedDataAsync();

app.Run();
