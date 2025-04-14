using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;
using SwampLocks.EmailSevice;

Env.Load();  // Load environment variables (e.g., DB_NAME, SERVER_NAME, etc.)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container
builder.Services.AddControllers()
    .AddNewtonsoftJson();

string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
string? emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
string? emailServer = Environment.GetEnvironmentVariable("EMAIL_SERVER");
string? emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

Console.WriteLine(connectionString);
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

builder.Services.AddScoped<EmailNotificationService>(provider =>
    new EmailNotificationService(
        emailServer,  
        emailUsername,  
        emailPassword
    ));

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