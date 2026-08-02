
using EventManagerService.Application;
using EventManagerService.Domain;
using EventManagerService.Infrastructure;
using EventManagerService.Infrastructure.DataAssets;
using Microsoft.EntityFrameworkCore;
using EventManagerService;

var builder = WebApplication.CreateBuilder(args);

// Регистрация DbContext с провайдером PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сервисы в контейнер.
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddDomain();
builder.Services.AddPresentation();
// Регистрация сервисов локализации, чтобы IStringLocalizerFactory был доступен в DI
builder.Services.AddLocalization();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

}

// Настраиваем конвейер обработки HTTP-запросов.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
