using KnowledgeNetwork.Domains.Code.Services;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register our analysis services
builder.Services.AddScoped<CSharpMethodBlockAnalyzer>();
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
