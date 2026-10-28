using Microsoft.EntityFrameworkCore;
using RunAnalyzer.Api.Data;
using RunAnalyzer.Api.Services;

const string FrontendCorsPolicy = "AllowViteFrontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<RunParser>();

builder.Services.AddDbContext<RunDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Create the SQLite database and apply any pending migrations on startup,
// so a fresh clone runs without a separate `dotnet ef database update` step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RunDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();
