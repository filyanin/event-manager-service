using EventManagerService.Application;
using EventManagerService.Domain;
using EventManagerService.Infrastructure;
using EventManagerService.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAplication();
builder.Services.AddDomain();
builder.Services.AddPresentation();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
