using Facilities.Api;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedFramework();
builder.Services.AddFacilitiesModule(builder.Configuration);

var app = builder.Build();

app.UseSharedFramework();
await app.SeedDataAsync();

app.Run();
