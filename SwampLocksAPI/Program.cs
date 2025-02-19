using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksAPI.Data;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Replace with your Azure SQL Database connection string
string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

// Register dependencies
builder.Services.AddDbContext<FinancialContext>((options) =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddDbContext<LocalContext>(opt =>
    opt.UseInMemoryDatabase("LocalFinancials"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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