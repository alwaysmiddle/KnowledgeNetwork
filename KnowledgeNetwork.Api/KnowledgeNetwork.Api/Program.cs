using KnowledgeNetwork.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register our core service for code analysis
builder.Services.AddScoped<IRoslynAnalysisService, RoslynAnalysisService>();

// Add CORS for React development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:1420")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();

app.Run();