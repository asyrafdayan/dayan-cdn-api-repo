using API.Entities.Contexts;
using API.Interfaces;
using API.Middleware;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog config
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile($"serilog-config.{builder.Environment.EnvironmentName}.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Services.Configure<AppSettingsModel>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllers();

// Inject SQLite context
builder.Services.AddDbContext<DevContext>(options => options.UseSqlite(builder.Configuration.GetSection("AppSettings:ConnectionStrings").ToString()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
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
