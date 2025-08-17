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
                        "📊 Standard Analysis",
                        "🔧 CFG Analysis (Unified Format)",
                        "📈 Visualization & Export",
                        "⚡ Performance Testing",
                        "⚙️ Settings",
                        "Exit"
                    }));

            switch (choice)
            {
                case "📊 Standard Analysis":
                    await HandleStandardAnalysisMenuAsync(testRunner);
                    break;
                case "🔧 CFG Analysis (Unified Format)":
                    await HandleCfgAnalysisMenuAsync(testRunner);
                    break;
                case "📈 Visualization & Export":
                    await HandleVisualizationMenuAsync(testRunner);
                    break;
                case "⚡ Performance Testing":
                    await HandlePerformanceMenuAsync(testRunner);
                    break;
                case "⚙️ Settings":
                    await HandleSettingsMenuAsync(testRunner);
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
                .Title("[cyan]Choose output format:[/]")
                .AddChoices(new[] { "console", "json", "markdown" }));

        var saveToFile = AnsiConsole.Confirm("[cyan]Save results to file?[/]", false);
        string? outputPath = null;
        
        if (saveToFile)
        {
            var defaultName = $"analysis-{Path.GetFileNameWithoutExtension(filePath)}-{DateTime.Now:yyyyMMdd-HHmmss}";
            var extension = export == "json" ? ".json" : export == "markdown" ? ".md" : ".txt";
            outputPath = AnsiConsole.Ask("[cyan]Output file name:[/]", defaultName + extension);
        }

        AnsiConsole.Status()
            .Start("📊 Running standard analysis...", async ctx =>
            {
                await testRunner.RunFileAnalysisAsync(filePath, export == "console" ? null : export, outputPath);
            });

        AnsiConsole.MarkupLine("[green]✅ Standard analysis completed![/]");
    }

    static async Task HandleDirectoryTestAsync(AnalysisTestRunner testRunner)
    {
        var directoryPath = AnsiConsole.Ask<string>("[green]Enter directory path:[/]");
        
        if (!Directory.Exists(directoryPath))
        {
            AnsiConsole.MarkupLine($"[red]Directory not found: {directoryPath}[/]");
            return;
        }

        var pattern = AnsiConsole.Ask("[green]File pattern:[/]", "*.cs");
        
        var export = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Choose output format:[/]")
                .AddChoices(new[] { "console", "json", "markdown" }));

        var saveToFile = AnsiConsole.Confirm("[cyan]Save results to file?[/]", false);
        string? outputPath = null;
        
        if (saveToFile)
        {
            var defaultName = $"batch-analysis-{DateTime.Now:yyyyMMdd-HHmmss}";
            var extension = export == "json" ? ".json" : export == "markdown" ? ".md" : ".txt";
            outputPath = AnsiConsole.Ask("[cyan]Output file name:[/]", defaultName + extension);
        }

        AnsiConsole.Status()
            .Start("📁 Running directory analysis...", async ctx =>
            {
                await testRunner.RunDirectoryAnalysisAsync(directoryPath, pattern, export == "console" ? null : export, outputPath);
            });

        AnsiConsole.MarkupLine("[green]✅ Directory analysis completed![/]");
    }

    static async Task HandleCFGVisualizationAsync(AnalysisTestRunner testRunner)
    {
        var filePath = AnsiConsole.Ask<string>("[green]Enter C# file path for CFG visualization:[/]");
        
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        AnsiConsole.Status()
            .Start("📊 Generating CFG visualization (legacy format)...", async ctx =>
            {
                await testRunner.VisualizeCFGAsync(filePath);
            });

        AnsiConsole.MarkupLine("[green]✅ CFG visualization completed![/]");
        AnsiConsole.MarkupLine("[yellow]💡 Tip: Use '🔧 CFG Analysis (Unified Format)' for the enhanced experience![/]");
    }

    static async Task HandleStandardAnalysisMenuAsync(AnalysisTestRunner testRunner)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Standard Analysis Options:[/]")
                .AddChoices(new[]
                {
                    "📄 Analyze Single File",
                    "📁 Analyze Directory",
                    "🔙 Back to Main Menu"
                }));

        switch (choice)
        {
            case "📄 Analyze Single File":
                await HandleSingleFileTestAsync(testRunner);
                break;
            case "📁 Analyze Directory":
                await HandleDirectoryTestAsync(testRunner);
                break;
        }
    }

    static async Task HandleCfgAnalysisMenuAsync(AnalysisTestRunner testRunner)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]CFG Analysis (Unified Format) Options:[/]")
                .AddChoices(new[]
                {
                    "📄 Analyze Single File CFG",
                    "📁 Analyze Directory CFG",
                    "⚡ Quick CFG Analysis (Paste Code)",
                    "📋 Recent Files",
                    "🔙 Back to Main Menu"
                }));

        switch (choice)
        {
            case "📄 Analyze Single File CFG":
                await HandleCfgSingleFileAsync(testRunner);
                break;
            case "📁 Analyze Directory CFG":
                await HandleCfgDirectoryAsync(testRunner);
                break;
            case "⚡ Quick CFG Analysis (Paste Code)":
                await HandleQuickCfgAnalysisAsync(testRunner);
                break;
            case "📋 Recent Files":
                await HandleRecentFilesAsync(testRunner);
                break;
        }
    }

    static async Task HandleVisualizationMenuAsync(AnalysisTestRunner testRunner)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Visualization & Export Options:[/]")
                .AddChoices(new[]
                {
                    "📊 View CFG Graph (Legacy Format)",
                    "🔍 Interactive Node Explorer",
                    "📤 Export Last Results",
                    "📋 Compare Analyses",
                    "🔙 Back to Main Menu"
                }));

        switch (choice)
        {
            case "📊 View CFG Graph (Legacy Format)":
                await HandleCFGVisualizationAsync(testRunner);
                break;
            case "🔍 Interactive Node Explorer":
                await HandleInteractiveExplorerAsync(testRunner);
                break;
            case "📤 Export Last Results":
                await HandleExportResultsAsync(testRunner);
                break;
            case "📋 Compare Analyses":
                await HandleCompareAnalysesAsync(testRunner);
                break;
        }
    }

    static async Task HandlePerformanceMenuAsync(AnalysisTestRunner testRunner)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Performance Testing Options:[/]")
                .AddChoices(new[]
                {
                    "🏃 Run Benchmark Suite",
                    "📊 CFG Performance Test",
                    "📈 Memory Usage Analysis",
                    "🔙 Back to Main Menu"
                }));

        switch (choice)
        {
            case "🏃 Run Benchmark Suite":
                await testRunner.RunBenchmarkSuiteAsync();
                break;
            case "📊 CFG Performance Test":
                await HandleCfgPerformanceTestAsync(testRunner);
                break;
            case "📈 Memory Usage Analysis":
                await HandleMemoryAnalysisAsync(testRunner);
                break;
        }
    }

    static async Task HandleSettingsMenuAsync(AnalysisTestRunner testRunner)
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Settings Options:[/]")
                .AddChoices(new[]
                {
                    "🎨 Color Theme",
                    "📋 Default Export Format",
                    "📁 Working Directory",
                    "🧹 Clear History",
                    "🔙 Back to Main Menu"
                }));

        switch (choice)
        {
            case "🎨 Color Theme":
                await HandleColorThemeAsync();
                break;
            case "📋 Default Export Format":
                await HandleDefaultExportFormatAsync();
                break;
            case "📁 Working Directory":
                await HandleWorkingDirectoryAsync();
                break;
            case "🧹 Clear History":
                await HandleClearHistoryAsync();
                break;
        }
    }

    static async Task HandleCfgSingleFileAsync(AnalysisTestRunner testRunner)
    {
        var filePath = AnsiConsole.Ask<string>("[green]Enter file path:[/]");
        
        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
            return;
        }

        // Interactive options
        var includeOperations = AnsiConsole.Confirm("[cyan]Include operations in analysis?[/]", true);
        
        var depthChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Choose analysis depth:[/]")
                .AddChoices(new[] { "Unlimited", "Methods only", "Methods + Blocks", "Custom limit" }));

        int? depth = depthChoice switch
        {
            "Methods only" => 1,
            "Methods + Blocks" => 2,
            "Custom limit" => AnsiConsole.Ask<int>("[cyan]Enter depth limit:[/]"),
            _ => null
        };

        var outputFormat = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Choose output format:[/]")
                .AddChoices(new[] { "tree", "json", "markdown" }));

        var saveToFile = AnsiConsole.Confirm("[cyan]Save results to file?[/]", false);
        string? outputPath = null;
        
        if (saveToFile)
        {
            var defaultName = $"cfg-analysis-{Path.GetFileNameWithoutExtension(filePath)}-{DateTime.Now:yyyyMMdd-HHmmss}";
            var extension = outputFormat == "json" ? ".json" : outputFormat == "markdown" ? ".md" : ".txt";
            outputPath = AnsiConsole.Ask("[cyan]Output file name:[/]", defaultName + extension);
        }

        AnsiConsole.Status()
            .Start("🔧 Analyzing CFG with unified format...", async ctx =>
            {
                await testRunner.RunCfgAnalysisAsync(filePath, outputFormat == "tree" ? null : outputFormat, outputPath);
            });

        AnsiConsole.MarkupLine("[green]✅ CFG analysis completed![/]");
        
        // Interactive post-analysis options
        var postChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]What would you like to do next?[/]")
                .AddChoices(new[] { "View relationships", "View metrics", "Export in different format", "Continue" }));

        // TODO: Implement post-analysis actions based on choice
    }

    static async Task HandleCfgDirectoryAsync(AnalysisTestRunner testRunner)
    {
        var directoryPath = AnsiConsole.Ask<string>("[green]Enter directory path:[/]");
        
        if (!Directory.Exists(directoryPath))
        {
            AnsiConsole.MarkupLine($"[red]Directory not found: {directoryPath}[/]");
            return;
        }

        var pattern = AnsiConsole.Ask("[green]File pattern:[/]", "*.cs");
        var includeOperations = AnsiConsole.Confirm("[cyan]Include operations in analysis?[/]", true);
        
        var outputFormat = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[cyan]Choose output format:[/]")
                .AddChoices(new[] { "tree", "json", "markdown" }));

        var saveToFile = AnsiConsole.Confirm("[cyan]Save results to file?[/]", false);
        string? outputPath = null;
        
        if (saveToFile)
        {
            var defaultName = $"cfg-batch-analysis-{DateTime.Now:yyyyMMdd-HHmmss}";
            var extension = outputFormat == "json" ? ".json" : outputFormat == "markdown" ? ".md" : ".txt";
            outputPath = AnsiConsole.Ask("[cyan]Output file name:[/]", defaultName + extension);
        }

        await testRunner.RunDirectoryCfgAnalysisAsync(directoryPath, pattern, outputFormat == "tree" ? null : outputFormat, outputPath);
        
        AnsiConsole.MarkupLine("[green]✅ Batch CFG analysis completed![/]");
    }

    static async Task HandleQuickCfgAnalysisAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[cyan]📝 Quick CFG Analysis - Paste your C# code:[/]");
        AnsiConsole.MarkupLine("[gray]Press Enter twice when finished, or type 'DONE' on a new line[/]");
        
        var codeLines = new List<string>();
        string line;
        
        while ((line = Console.ReadLine()) != null)
        {
            if (line.Trim().ToUpper() == "DONE" || (string.IsNullOrWhiteSpace(line) && codeLines.Count > 0 && string.IsNullOrWhiteSpace(codeLines.LastOrDefault())))
            {
                break;
            }
            codeLines.Add(line);
        }

        if (!codeLines.Any() || codeLines.All(string.IsNullOrWhiteSpace))
        {
            AnsiConsole.MarkupLine("[red]No code provided![/]");
            return;
        }

        var code = string.Join(Environment.NewLine, codeLines);
        var includeOperations = AnsiConsole.Confirm("[cyan]Include operations in analysis?[/]", true);
        
        // Create temporary file for analysis
        var tempFile = Path.GetTempFileName() + ".cs";
        await File.WriteAllTextAsync(tempFile, code);
        
        try
        {
            AnsiConsole.Status()
                .Start("🔧 Analyzing pasted code...", async ctx =>
                {
                    await testRunner.RunCfgAnalysisAsync(tempFile, null, null);
                });
                
            AnsiConsole.MarkupLine("[green]✅ Quick CFG analysis completed![/]");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // Placeholder implementations for remaining handlers
    static async Task HandleRecentFilesAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]📋 Recent files feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleInteractiveExplorerAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]🔍 Interactive explorer feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleExportResultsAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]📤 Export results feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleCompareAnalysesAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]📋 Compare analyses feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleCfgPerformanceTestAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]📊 CFG performance test feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleMemoryAnalysisAsync(AnalysisTestRunner testRunner)
    {
        AnsiConsole.MarkupLine("[yellow]📈 Memory analysis feature coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleColorThemeAsync()
    {
        AnsiConsole.MarkupLine("[yellow]🎨 Color theme settings coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleDefaultExportFormatAsync()
    {
        AnsiConsole.MarkupLine("[yellow]📋 Default export format settings coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleWorkingDirectoryAsync()
    {
        AnsiConsole.MarkupLine("[yellow]📁 Working directory settings coming soon![/]");
        await Task.Delay(1000);
    }

    static async Task HandleClearHistoryAsync()
    {
        AnsiConsole.MarkupLine("[yellow]🧹 Clear history feature coming soon![/]");
        await Task.Delay(1000);
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