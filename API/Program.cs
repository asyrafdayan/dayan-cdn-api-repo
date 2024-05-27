using API.Entities.Contexts;
using API.Interfaces;
using API.Middleware;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Allow config from docker compose
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json")
    .AddJsonFile($"serilog-config.{builder.Environment.EnvironmentName}.json")
    .AddEnvironmentVariables()
    .Build();

// Add Serilog config
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    builder.Services.Configure<AppSettingsModel>(builder.Configuration.GetSection("AppSettings"));

    builder.Services.AddControllers();

    // Inject SQLite context
    builder.Services.AddDbContext<DevContext>(options => options.UseSqlite(builder.Configuration.GetSection("AppSettings:ConnectionStrings:SQLite").ToString()));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Register services
    builder.Services.AddScoped<IRedisService, RedisService>();
    builder.Services.AddScoped<IFreelance, FreelanceService>();

    // Remove default Logger and replace with Serilog
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(logger);

    // Exception Handler
    builder.Services.AddExceptionHandler<ExceptionMiddleware>();
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Fatal(ex, $"Server terminated: {ex.Message}");
}
finally
{
    logger.Dispose();
}
