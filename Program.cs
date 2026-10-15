using Microsoft.AspNetCore.Mvc;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = _ => ApiResponse.InternalError();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.OperationFilter<UserRequestOperationFilter>());
builder.Services.AddCors();

var connectionString = $"Host={Environment.GetEnvironmentVariable("DATABASE_HOST_URL") ?? "localhost"};Port=5432;Database=postgres;Username=postgres;Password=postgres";

builder.Services.AddSingleton<IDatabase>(new Database(connectionString));
builder.Services.AddScoped<UserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
    db.RunMigrations();
}

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (NpgsqlException exception) when (!context.Response.HasStarted)
    {
        app.Logger.LogError(exception, "Database request failed");
        context.Response.Clear();
        await ApiResponse.InternalError().ExecuteResultAsync(new ActionContext
        {
            HttpContext = context
        });
    }
});
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode is 404 or 405)
    {
        await ApiResponse.NotFound("404 not found").ExecuteResultAsync(new ActionContext
        {
            HttpContext = context.HttpContext
        });
    }
});
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
app.MapControllers();

app.Run();

public partial class Program;
