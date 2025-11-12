using myCookbook.Infrastructure.External.MealDb;
using MyCookbook.Application.Recipes;
using MyCookbook.Infrastructure.External.MealDb;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MealDbOptions>(
    builder.Configuration.GetSection("ExternalApis:MealDb"));
builder.Services.AddHttpClient<IExternalRecipeClient, MealDbClient>();
builder.Services.AddCors(p =>
{
    p.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
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
