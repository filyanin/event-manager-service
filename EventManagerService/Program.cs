
using EventManagerService.Application;
using EventManagerService.Domain;
using EventManagerService.Infrastructure;
using EventManagerService.Infrastructure.DataAssets;
using EventManagerService.Infrastructure.Security.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using EventManagerService;

var builder = WebApplication.CreateBuilder(args);

// Получаем JWT настройки
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

// Регистрация DbContext с провайдером PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем сервисы в контейнер.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddDomain();
builder.Services.AddPresentation();
// Регистрация сервисов локализации, чтобы IStringLocalizerFactory был доступен в DI
builder.Services.AddLocalization();

// Конфигурация JWT аутентификации
if (jwtSettings != null)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
}

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

// Добавляем аутентификацию и авторизацию в pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
