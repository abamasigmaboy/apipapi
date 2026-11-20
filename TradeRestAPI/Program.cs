using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TradeRestAPI.Models;
using TradeRestAPI.Models.Abstractions;
using TradeRestAPI.Models.Data;
using TradeRestAPI.Models.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Авторизация и аутентификация JWT Bearer
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

// 2. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Контроллеры
builder.Services.AddControllers();

// 4. Кэширование в оперативной памяти (MemoryCache)
builder.Services.AddSingleton<IMemoryCache>(new MemoryCache(new MemoryCacheOptions
{
    TrackStatistics = true,
    SizeLimit = 100,
}));

// 5. Подключение к PostgreSQL через Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=trade_db;Username=postgres;Password=postgres";

builder.Services.AddDbContext<TradeDbContext>(opt =>
    opt.UseNpgsql(connectionString));

// 6. Регистрация сервисов бизнес-логики
builder.Services.AddScoped<ICommonService<Department, int>, DepartmentsService>();
builder.Services.AddScoped<DepartmentsService>();

builder.Services.AddScoped<ICommonService<Product, int>, ProductsService>();
builder.Services.AddScoped<ProductsService>();

builder.Services.AddScoped<ICommonService<Sale, int>, SalesService>();
builder.Services.AddScoped<SalesService>();

builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<AuthService>();

// 7. CORS и Response Caching
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());
});
builder.Services.AddResponseCaching();

var app = builder.Build();

// Инициализация базы данных при старте (если не была создана)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<TradeDbContext>();
        db.Database.EnsureCreated();
        logger.LogInformation("Проверка подключения к базе данных прошла успешно.");
    }
    catch (Exception ex)
    {
        logger.LogWarning($"База данных еще не готова или недоступна: {ex.Message}");
    }
}

// Swagger доступен всегда (и на корневом URL http://localhost:5000/, и на /swagger)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trade REST API v1");
    c.RoutePrefix = string.Empty; // Swagger открывается сразу на главной странице!
});

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCaching();

app.MapControllers();

app.Run();
