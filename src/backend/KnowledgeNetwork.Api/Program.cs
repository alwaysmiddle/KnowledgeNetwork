using KnowledgeNetwork.Domains.Code.Services;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;
using ICSharpMethodBlockAnalyzer = KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions.ICSharpMethodBlockAnalyzer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register our analysis services with dependency injection

// Core block analyzer services (refined 2-step pipeline)
builder.Services.AddScoped<IRoslynCfgExtractor, RoslynCfgExtractor>();
builder.Services.AddScoped<IDomainModelConverter, DomainModelConverter>();

// Main block analyzer (composed from the two services above)
builder.Services.AddScoped<ICSharpMethodBlockAnalyzer, CSharpMethodBlockAnalyzer>();
builder.Services.AddScoped<CSharpMethodBlockAnalyzer>(); // Keep concrete registration for backward compatibility

// Class relationship analysis utilities
builder.Services.AddScoped<IFileNameResolver, FileNameResolver>();
builder.Services.AddScoped<IClassNodeFactory, ClassNodeFactory>();
builder.Services.AddScoped<IComplexityCalculator, ComplexityCalculator>();
builder.Services.AddScoped<ISyntaxUtilities, SyntaxUtilities>();

// Specialized relationship analyzers
builder.Services.AddScoped<IInheritanceAnalyzer, InheritanceAnalyzer>();
builder.Services.AddScoped<IInterfaceImplementationAnalyzer, InterfaceImplementationAnalyzer>();
builder.Services.AddScoped<ICompositionAnalyzer, CompositionAnalyzer>();
builder.Services.AddScoped<IDependencyAnalyzer, DependencyAnalyzer>();
builder.Services.AddScoped<INestedClassAnalyzer, NestedClassAnalyzer>();

// Main class relationship analyzer (orchestrates the specialized analyzers above)
builder.Services.AddScoped<ICSharpClassRelationshipAnalyzer, CSharpClassRelationshipAnalyzer>();
builder.Services.AddScoped<CSharpClassRelationshipAnalyzer>(); // Keep concrete registration for backward compatibility

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
