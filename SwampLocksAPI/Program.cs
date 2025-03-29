using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;

Env.Load();  // Load environment variables (e.g., DB_NAME, SERVER_NAME, etc.)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson();

string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");


if (connectionString is null)
{
    // using netra ID
    builder.Services.AddDbContext<FinancialContext>(options =>
    {
        options.UseSqlServer("");  
    });
}
else
{
    // using connection string
    builder.Services.AddDbContext<FinancialContext>((options) =>
    {
        options.UseSqlServer(connectionString);
    });
}

// Register CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000")  // Allow requests from your frontend 
            .AllowAnyMethod()  
            .AllowAnyHeader();
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowLocalhost"); 
app.UseRouting(); 
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();