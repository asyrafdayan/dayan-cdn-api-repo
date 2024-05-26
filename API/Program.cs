using API.Entities.Contexts;
using API.Interfaces;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettingsModel>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllers();

// Inject SQLite context
builder.Services.AddDbContext<DevContext>(options => options.UseSqlite(builder.Configuration.GetSection("AppSettings:ConnectionStrings").ToString()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IFreelance, FreelanceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
