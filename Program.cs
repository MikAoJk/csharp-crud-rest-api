var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
app.MapControllers();

app.Run();
