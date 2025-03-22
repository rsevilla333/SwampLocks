using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SwampLocksDb.Data;

void CreateBuilder(WebApplicationBuilder builder)
{
    // Replace with your Azure SQL Database connection string
    string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

    // Add services to the container
    builder.Services.AddControllers()
        .AddNewtonsoftJson();

    // Register dependencies
    builder.Services.AddDbContext<FinancialsContext>((options) =>
    {
        options.UseSqlServer(connectionString);
    });

    builder.Services.AddDbContext<LocalContext>(opt =>
        opt.UseInMemoryDatabase("LocalFinancials"));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

void SetupApp(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

}

Env.Load();

var builder = WebApplication.CreateBuilder(args);

CreateBuilder(builder);

var app = builder.Build();

SetupApp(app);

app.Run();