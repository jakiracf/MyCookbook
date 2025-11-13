// src/Api/Program.cs
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; // for UseNpgsql extension
using MyCookbook.Infrastructure.Persistence;

using MyCookbook.Application.Recipes;
using MyCookbook.Infrastructure.External.MealDb;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.Configure<MyCookbook.Infrastructure.External.MealDb.MealDbOptions>(
    builder.Configuration.GetSection("ExternalApis:MealDb"));

// Typed HttpClient for TheMealDB
builder.Services.AddHttpClient<IExternalRecipeClient, MealDbClient>();

// DbContext (PostgreSQL)
var connStr = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(connStr));

// CORS for local dev (open for demo)
builder.Services.AddCors(p =>
{
    p.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    if (env.IsDevelopment())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();         // ← auto-run migrations

        // optional dev seed (only if DevSeeder is implemented)
        await DevSeeder.SeedAsync(db);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/ping", () => Results.Ok("pong"));

app.Run();
