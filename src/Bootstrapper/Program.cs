using Facilities.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFacilitiesModule(builder.Configuration);

var app = builder.Build();

app.Run();
