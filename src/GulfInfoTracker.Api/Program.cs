using GulfInfoTracker.Api.Data;
using GulfInfoTracker.Api.Data.Repositories;
using GulfInfoTracker.Api.Extensions;
using GulfInfoTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// EF Core via Aspire integration
builder.AddNpgsqlDbContext<AppDbContext>("GulfInfoTracker");

// Redis via Aspire integration
builder.AddRedisClient("redis");

// Repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ISourcePollLogRepository, SourcePollLogRepository>();

// Plugins – sources.json is copied to output directory by the csproj Content item
var sourcesJsonPath = Path.Combine(AppContext.BaseDirectory, "sources.json");
Console.WriteLine($"[Plugins] Loading sources from: {sourcesJsonPath} (exists={File.Exists(sourcesJsonPath)})");
builder.Services.AddPlugins(sourcesJsonPath, builder.Configuration);

// Ingestion
builder.Services.AddScoped<IIngestionProcessor, IngestionProcessor>();
builder.Services.AddHostedService<IngestionService>();

// AI pipeline
builder.Services.AddAiServices(builder.Configuration);
builder.Services.AddHostedService<ScoringBackgroundService>();
// builder.Services.AddHostedService<TranslationBackgroundService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

// Apply pending EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
