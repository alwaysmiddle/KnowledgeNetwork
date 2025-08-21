using System.Diagnostics;
using Spectre.Console;
using KnowledgeNetwork.Domains.Code.Services;
using KnowledgeNetwork.AnalysisTester.Formatters;
using KnowledgeNetwork.AnalysisTester.Models;
using System.Text.Json;

namespace KnowledgeNetwork.AnalysisTester.TestRunner;

/// <summary>
/// Core test runner for analysis services
/// </summary>
public class AnalysisTestRunner
{
    private readonly CSharpAnalysisService _csharpAnalysisService;
    private readonly AnalysisResultFormatter _resultFormatter;
    private readonly TestFileManager _fileManager;

    public AnalysisTestRunner()
    {
        _csharpAnalysisService = new CSharpAnalysisService();
        _resultFormatter = new AnalysisResultFormatter();
        _fileManager = new TestFileManager();
    }

    /// <summary>
    /// Analyze a single file
    /// </summary>
    public async Task RunFileAnalysisAsync(string filePath, string? exportFormat = null, string? outputPath = null)
    {
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        AnsiConsole.Status()
            .Start("Analyzing file...", async ctx =>
            {
                ctx.Status($"Reading file: {Path.GetFileName(filePath)}");
                var content = await File.ReadAllTextAsync(filePath);
                
                ctx.Status("Running analysis...");
                var stopwatch = Stopwatch.StartNew();
                
                var analysisResult = await _csharpAnalysisService.AnalyzeAsync(content);
                var cfgResults = await _csharpAnalysisService.ExtractControlFlowAsync(content);
                
                stopwatch.Stop();

                var testResult = new TestResult
                {
                    FilePath = filePath,
                    FileName = Path.GetFileName(filePath),
                    Language = "csharp",
                    AnalysisResult = analysisResult,
                    ControlFlowGraphs = cfgResults,
                    Duration = stopwatch.Elapsed,
                    Success = analysisResult.Success,
                    AnalyzedAt = DateTime.UtcNow
                };

                ctx.Status("Formatting results...");
                await DisplayOrExportResult(testResult, exportFormat, outputPath);
            });
    }

    /// <summary>
    /// Analyze all files in a directory
    /// </summary>
    public async Task RunDirectoryAnalysisAsync(string directoryPath, string pattern = "*.cs", string? exportFormat = null, string? outputPath = null)
    {
        if (!Directory.Exists(directoryPath))
        {
            AnsiConsole.MarkupLine($"[red]Directory not found: {directoryPath}[/]");
            return;
        }

        var files = _fileManager.DiscoverTestFiles(directoryPath, new[] { pattern });
        
        if (!files.Any())
        {
            AnsiConsole.MarkupLine($"[yellow]No files found matching pattern '{pattern}' in {directoryPath}[/]");
            return;
        }

        var results = new List<TestResult>();

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Analyzing files...[/]");
                task.MaxValue = files.Count;

                foreach (var file in files)
                {
                    task.Description = $"Analyzing {Path.GetFileName(file)}";
                    
                    try
                    {
                        var content = await File.ReadAllTextAsync(file);
                        var stopwatch = Stopwatch.StartNew();
                        
                        var analysisResult = await _csharpAnalysisService.AnalyzeAsync(content);
                        var cfgResults = await _csharpAnalysisService.ExtractControlFlowAsync(content);
                        
                        stopwatch.Stop();

                        var testResult = new TestResult
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file),
                            Language = "csharp",
                            AnalysisResult = analysisResult,
                            ControlFlowGraphs = cfgResults,
                            Duration = stopwatch.Elapsed,
                            Success = analysisResult.Success,
                            AnalyzedAt = DateTime.UtcNow
                        };

                        results.Add(testResult);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error analyzing {Path.GetFileName(file)}: {ex.Message}[/]");
                    }

                    task.Increment(1);
                }
            });

        await DisplayOrExportBatchResults(results, exportFormat, outputPath);
    }

    /// <summary>
    /// Run performance benchmarks
    /// </summary>
    public async Task RunBenchmarkSuiteAsync()
    {
        AnsiConsole.MarkupLine("[yellow]Running performance benchmarks...[/]");

        var testCases = new[]
        {
            ("Simple Class", "class Test { public void Method() { } }"),
            ("Complex Method", GenerateComplexMethod()),
            ("Large Class", GenerateLargeClass()),
            ("Nested Conditions", GenerateNestedConditions()),
            ("Multiple Loops", GenerateMultipleLoops())
        };

        var table = new Table()
            .AddColumn("Test Case")
            .AddColumn("Duration (ms)")
            .AddColumn("Classes")
            .AddColumn("Methods")
            .AddColumn("CFG Blocks")
            .AddColumn("Memory (KB)");

        foreach (var (name, code) in testCases)
        {
            var initialMemory = GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            
            var analysisResult = await _csharpAnalysisService.AnalyzeAsync(code);
            var cfgResults = await _csharpAnalysisService.ExtractControlFlowAsync(code);
            
            stopwatch.Stop();
            var finalMemory = GC.GetTotalMemory(true);
            var memoryUsed = (finalMemory - initialMemory) / 1024;

            table.AddRow(
                name,
                stopwatch.ElapsedMilliseconds.ToString(),
                analysisResult.Classes.Count.ToString(),
                analysisResult.Methods.Count.ToString(),
                cfgResults.Sum(cfg => cfg.BasicBlocks.Count).ToString(),
                memoryUsed.ToString());
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Visualize control flow graph for a file
    /// </summary>
    public async Task VisualizeCFGAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        var content = await File.ReadAllTextAsync(filePath);
        var cfgResults = await _csharpAnalysisService.ExtractControlFlowAsync(content);

        if (!cfgResults.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No control flow graphs found in the file.[/]");
            return;
        }

        foreach (var cfg in cfgResults)
        {
            _resultFormatter.FormatCFGVisualization(cfg);
        }
    }

    private async Task DisplayOrExportResult(TestResult result, string? exportFormat, string? outputPath)
    {
        switch (exportFormat?.ToLower())
        {
            case "json":
                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                if (outputPath != null)
                {
                    await File.WriteAllTextAsync(outputPath, json);
                    AnsiConsole.MarkupLine($"[green]Results exported to: {outputPath}[/]");
                }
                else
                {
                    AnsiConsole.WriteLine(json);
                }
                break;
            
            case "markdown":
                var markdown = _resultFormatter.FormatAsMarkdown(result);
                if (outputPath != null)
                {
                    await File.WriteAllTextAsync(outputPath, markdown);
                    AnsiConsole.MarkupLine($"[green]Results exported to: {outputPath}[/]");
                }
                else
                {
                    AnsiConsole.WriteLine(markdown);
                }
                break;
            
            default:
                _resultFormatter.FormatAsConsoleTable(result);
                break;
        }
    }

    private async Task DisplayOrExportBatchResults(List<TestResult> results, string? exportFormat, string? outputPath)
    {
        switch (exportFormat?.ToLower())
        {
            case "json":
                var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
                if (outputPath != null)
                {
                    await File.WriteAllTextAsync(outputPath, json);
                    AnsiConsole.MarkupLine($"[green]Results exported to: {outputPath}[/]");
                }
                else
                {
                    AnsiConsole.WriteLine(json);
                }
                break;
            
            case "markdown":
                var markdown = _resultFormatter.FormatBatchResultsAsMarkdown(results);
                if (outputPath != null)
                {
                    await File.WriteAllTextAsync(outputPath, markdown);
                    AnsiConsole.MarkupLine($"[green]Results exported to: {outputPath}[/]");
                }
                else
                {
                    AnsiConsole.WriteLine(markdown);
                }
                break;
            
            default:
                _resultFormatter.FormatBatchResultsAsConsoleTable(results);
                break;
        }
    }

    private string GenerateComplexMethod()
    {
        return @"
class ComplexTest 
{
    public async Task<string> ComplexMethod(int input, string text)
    {
        if (input > 0)
        {
            for (int i = 0; i < input; i++)
            {
                if (i % 2 == 0)
                {
                    text += i.ToString();
                }
                else
                {
                    text = text.ToUpper();
                }
            }
        }
        else if (input < 0)
        {
            while (input < 0)
            {
                input++;
                text = text.Substring(1);
            }
        }
        
        try
        {
            return await ProcessTextAsync(text);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    
    private async Task<string> ProcessTextAsync(string text) => await Task.FromResult(text);
}";
    }

    private string GenerateLargeClass()
    {
        var methods = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            methods.Add($"public void Method{i}() {{ var x = {i}; }}");
        }

        return $@"
class LargeTest
{{
    {string.Join("\n    ", methods)}
}}";
    }

    private string GenerateNestedConditions()
    {
        return @"
class NestedTest
{
    public void NestedMethod(int a, int b, int c)
    {
        if (a > 0)
        {
            if (b > 0)
            {
                if (c > 0)
                {
                    Console.WriteLine(""All positive"");
                }
                else
                {
                    Console.WriteLine(""C is not positive"");
                }
            }
            else
            {
                if (c > 0)
                {
                    Console.WriteLine(""B negative, C positive"");
                }
                else
                {
                    Console.WriteLine(""B and C negative"");
                }
            }
        }
        else
        {
            Console.WriteLine(""A is not positive"");
        }
    }
}";
    }

    private string GenerateMultipleLoops()
    {
        return @"
class LoopTest
{
    public void MultipleLoops()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
        }
        
        int j = 0;
        while (j < 5)
        {
            j++;
        }
        
        do
        {
            j--;
        } while (j > 0);
        
        foreach (var item in new[] { 1, 2, 3 })
        {
            Console.WriteLine(item);
        }
    }
}";
    }
}