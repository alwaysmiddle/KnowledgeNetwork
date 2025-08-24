using KnowledgeNetwork.Domains.Code.Services;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using ICSharpMethodBlockAnalyzer = KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions.ICSharpMethodBlockAnalyzer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register our analysis services with dependency injection
// Core analyzer services (focused, testable components)
builder.Services.AddScoped<IRoslynOperationExtractor, RoslynOperationExtractor>();
builder.Services.AddScoped<ICfgStructureBuilder, CfgStructureBuilder>();
builder.Services.AddScoped<IDomainModelConverter, DomainModelConverter>();

// Main analyzer (composed from the three services above)
builder.Services.AddScoped<ICSharpMethodBlockAnalyzer, CSharpMethodBlockAnalyzer>();
builder.Services.AddScoped<CSharpMethodBlockAnalyzer>(); // Keep concrete registration for backward compatibility

// Higher-level analysis service
builder.Services.AddScoped<CSharpAnalysisService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map our controllers
app.MapControllers();


app.Run();
