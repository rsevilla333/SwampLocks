using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;

Env.Load();  // Load environment variables (e.g., DB_NAME, SERVER_NAME, etc.)

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Register FinancialContext with SQL Server (no need for a connection string, Azure manages this)
builder.Services.AddDbContext<FinancialContext>(options =>
{
    options.UseSqlServer("");  
});

// Register CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:3000")  // Allow requests from your frontend (adjust port if needed)
            .AllowAnyMethod()  // Allow all HTTP methods (GET, POST, etc.)
            .AllowAnyHeader(); // Allow any headers
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowLocalhost"); 
app.UseRouting(); 
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();