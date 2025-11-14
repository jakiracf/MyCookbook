// src/Api/Program.cs
using Microsoft.EntityFrameworkCore;
using MyCookbook.Application;
using MyCookbook.Application.Recipes;
using MyCookbook.Infrastructure.ApplicationServices;
using MyCookbook.Infrastructure.External.MealDb;
using MyCookbook.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TheMealDB options + typed HttpClient
builder.Services.Configure<MealDbOptions>(builder.Configuration.GetSection("ExternalApis:MealDb"));
builder.Services.AddHttpClient<IExternalRecipeClient, MealDbClient>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));
builder.Services.AddMemoryCache();

// DbContext (PostgreSQL) with transient fault handling
var connStr = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(
        connStr,
        npg =>
            npg.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(2),
                errorCodesToAdd: null
            )
    )
);

// CORS for local dev (open for demo)
builder.Services.AddCors(p =>
{
    p.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ---- Dev-only: auto-migrate (with retry) + seed (enabled by default) ----
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbStartup");

    if (env.IsDevelopment() && (cfg.GetValue<bool?>("MigrateOnStartup") ?? true))
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var attempts = 0;
        while (true)
        {
            try
            {
                await db.Database.MigrateAsync();

                // Seed is ON by default in Development; set "SeedOnStartup": false to skip
                var seed = cfg.GetValue<bool?>("SeedOnStartup");
                if (seed is null || seed == true)
                {
                    await DevSeeder.SeedAsync(db);
                }

                log.LogInformation(
                    "Database migrated{Seed} successfully.",
                    (seed is null || seed == true) ? " and seeded" : ""
                );
                break;
            }
            catch (Exception ex) when (attempts++ < 6)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(30, Math.Pow(2, attempts))); // 2,4,8,16,30,30
                log.LogWarning(
                    ex,
                    "DB not ready (attempt {Attempt}). Retrying in {Delay}s…",
                    attempts,
                    delay.TotalSeconds
                );
                await Task.Delay(delay);
            }
        }
    }
    else
    {
        // In non-Development, just warn if there are pending migrations
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var pending = await db.Database.GetPendingMigrationsAsync();
        if (pending.Any())
            log.LogWarning(
                "There are {Count} pending migrations. Apply them before production start.",
                pending.Count()
            );
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
