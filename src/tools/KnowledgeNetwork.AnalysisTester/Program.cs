using System.CommandLine;
using Spectre.Console;
using KnowledgeNetwork.AnalysisTester.TestRunner;

namespace KnowledgeNetwork.AnalysisTester;

/// <summary>
/// Console application for testing Knowledge Network analysis services
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Knowledge Network Analysis Tester");

        // File option
        var fileOption = new Option<FileInfo?>(
            aliases: new[] { "--file", "-f" },
            description: "Analyze a single file");

        // Directory option
        var directoryOption = new Option<DirectoryInfo?>(
            aliases: new[] { "--dir", "-d" },
            description: "Analyze all files in a directory");

        // Pattern option
        var patternOption = new Option<string>(
            aliases: new[] { "--pattern", "-p" },
            getDefaultValue: () => "*.cs",
            description: "File pattern to match (default: *.cs)");

        // Export option
        var exportOption = new Option<string?>(
            aliases: new[] { "--export", "-e" },
            description: "Export format: json, markdown, or console (default)");

        // Output option
        var outputOption = new Option<FileInfo?>(
            aliases: new[] { "--output", "-o" },
            description: "Output file path for exported results");

        // Benchmark option
        var benchmarkOption = new Option<bool>(
            aliases: new[] { "--benchmark", "-b" },
            description: "Run performance benchmarks");

        // Interactive option
        var interactiveOption = new Option<bool>(
            aliases: new[] { "--interactive", "-i" },
            getDefaultValue: () => true,
            description: "Run in interactive mode (default)");

        // CFG option
        var cfgOption = new Option<bool>(
            aliases: new[] { "--cfg", "-c" },
            description: "Analyze control flow graphs with unified node format");

        rootCommand.AddOption(fileOption);
        rootCommand.AddOption(directoryOption);
        rootCommand.AddOption(patternOption);
        rootCommand.AddOption(exportOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(benchmarkOption);
        rootCommand.AddOption(interactiveOption);
        rootCommand.AddOption(cfgOption);

        rootCommand.SetHandler(async (file, directory, pattern, export, output, benchmark, interactive, cfg) =>
        {
            var testRunner = new AnalysisTestRunner();

            try
            {
                // Command line mode
                if (file != null)
                {
                    if (cfg)
                    {
                        await testRunner.RunCfgAnalysisAsync(file.FullName, export, output?.FullName);
                    }
                    else
                    {
                        await testRunner.RunFileAnalysisAsync(file.FullName, export, output?.FullName);
                    }
                }
                else if (directory != null)
                {
                    if (cfg)
                    {
                        await testRunner.RunDirectoryCfgAnalysisAsync(directory.FullName, pattern, export, output?.FullName);
                    }
                    else
                    {
                        await testRunner.RunDirectoryAnalysisAsync(directory.FullName, pattern, export, output?.FullName);
                    }
                }
                else if (benchmark)
                {
                    await testRunner.RunBenchmarkSuiteAsync();
                }
                else if (interactive && args.Length == 0)
                {
                    // Interactive mode - default when no arguments provided
                    await RunInteractiveModeAsync(testRunner);
                }
                else
                {
                    ShowUsage();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                Environment.Exit(1);
            }
        }, fileOption, directoryOption, patternOption, exportOption, outputOption, benchmarkOption, interactiveOption, cfgOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task RunInteractiveModeAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.Write(
            new FigletText("Knowledge Network")
                .Centered()
                .Color(Color.Blue));

        AnsiConsole.Write(
            new Panel("[bold]Analysis Tester[/]")
                .Header("Welcome")
                .BorderColor(Color.Green));

        while (true)
        {
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[green]What would you like to do?[/]")
                    .AddChoices(new[]
                    {
                        "Test Single File",
                        "Test Directory",
                        "Run Benchmark Suite",
                        "View CFG Visualization",
                        "Exit"
                    }));

            switch (choice)
            {
                case "Test Single File":
                    await HandleSingleFileTestAsync(testRunner);
                    break;
                case "Test Directory":
                    await HandleDirectoryTestAsync(testRunner);
                    break;
                case "Run Benchmark Suite":
                    await testRunner.RunBenchmarkSuiteAsync();
                    break;
                case "View CFG Visualization":
                    await HandleCFGVisualizationAsync(testRunner);
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[yellow]Thanks for using Knowledge Network Analysis Tester![/]");
                    return;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static async Task HandleSingleFileTestAsync(AnalysisTestRunner testRunner)
    {
        var filePath = AnsiConsole.Ask<string>("[green]Enter file path:[/]");
        
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        var export = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose output format:")
                .AddChoices(new[] { "console", "json", "markdown" }));

        await testRunner.RunFileAnalysisAsync(filePath, export == "console" ? null : export);
    }

    static async Task HandleDirectoryTestAsync(AnalysisTestRunner testRunner)
    {
        var directoryPath = AnsiConsole.Ask<string>("[green]Enter directory path:[/]");
        
        if (!Directory.Exists(directoryPath))
        {
            AnsiConsole.MarkupLine($"[red]Directory not found: {directoryPath}[/]");
            return;
        }

        var pattern = AnsiConsole.Ask("[green]Enter file pattern:[/]", "*.cs");
        
        var export = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Choose output format:")
                .AddChoices(new[] { "console", "json", "markdown" }));

        await testRunner.RunDirectoryAnalysisAsync(directoryPath, pattern, export == "console" ? null : export);
    }

    static async Task HandleCFGVisualizationAsync(AnalysisTestRunner testRunner)
    {
        var filePath = AnsiConsole.Ask<string>("[green]Enter C# file path for CFG visualization:[/]");
        
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        await testRunner.VisualizeCFGAsync(filePath);
    }

    static void ShowUsage()
    {
        AnsiConsole.MarkupLine("[yellow]Knowledge Network Analysis Tester[/]");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[green]Usage:[/]");
        AnsiConsole.MarkupLine("  dotnet run                           # Interactive mode");
        AnsiConsole.MarkupLine("  dotnet run --file MyClass.cs         # Test single file");
        AnsiConsole.MarkupLine("  dotnet run --dir src --pattern *.cs  # Test directory");
        AnsiConsole.MarkupLine("  dotnet run --benchmark               # Run benchmarks");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("[green]Options:[/]");
        AnsiConsole.MarkupLine("  --file, -f        Test a single file");
        AnsiConsole.MarkupLine("  --dir, -d         Test all files in directory");
        AnsiConsole.MarkupLine("  --pattern, -p     File pattern (default: *.cs)");
        AnsiConsole.MarkupLine("  --export, -e      Export format: json, markdown");
        AnsiConsole.MarkupLine("  --output, -o      Output file path");
        AnsiConsole.MarkupLine("  --benchmark, -b   Run performance benchmarks");
        AnsiConsole.MarkupLine("  --interactive, -i Run interactive mode");
    }
}