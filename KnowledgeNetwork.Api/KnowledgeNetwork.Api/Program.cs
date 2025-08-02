using KnowledgeNetwork.Api.Services;
using KnowledgeNetwork.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register our core service for code analysis
builder.Services.AddScoped<IRoslynAnalysisService, RoslynAnalysisService>();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Knowledge Network API",
        Version = "v1",
        Description = "A comprehensive API for analyzing C# code structure using Roslyn compiler APIs. " +
                     "This API provides detailed analysis of code elements, relationships, and structural information " +
                     "to support knowledge graph construction and code visualization.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@knowledgenetwork.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Enable XML comments for enhanced documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add response examples and better error descriptions
    options.EnableAnnotations();
    
    // Configure schema generation for better model documentation
    options.SchemaFilter<RequiredNotNullableSchemaFilter>();
});

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Network API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Knowledge Network API Documentation";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableFilter();
        options.ShowExtensions();
    });
}

app.UseCors("AllowReactApp");
app.UseRouting();
app.MapControllers();

// Log custom startup messages
var logger = app.Services.GetRequiredService<ILogger<Program>>();
app.Lifetime.ApplicationStarted.Register(() =>
{
    if (!app.Environment.IsDevelopment()) return;
    logger.LogInformation("Local Development Swagger UI available at: http://localhost:5000/swagger");
    logger.LogInformation("Local Development Health check endpoint: http://localhost:5000/api/CodeAnalysis/health");
});

app.Run();