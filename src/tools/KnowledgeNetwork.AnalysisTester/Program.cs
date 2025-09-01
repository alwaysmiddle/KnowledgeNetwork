
using KnowledgeNetwork.AnalysisTester.Core;
using KnowledgeNetwork.AnalysisTester.TestHarnesses.Domains.Code.Analyzers.Blocks.CSharpMethodBlockAnalyzer;
using Spectre.Console;

namespace KnowledgeNetwork.AnalysisTester;

/// <summary>
/// Interactive testing environment for Knowledge Network components
/// </summary>
class Program
{
    private static NavigationEngine _navigation = null!;
    private static Core.TestRunner _testRunner = null!;
    private static OutputFormatter _formatter = null!;

    static async Task Main(string[] args)
    {
        try
        {
            // Initialize the testing environment
            await InitializeAsync();

            // Show welcome message
            ShowWelcome();

            // Start interactive session
            await RunInteractiveSessionAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }

        AnsiConsole.MarkupLine("\n[dim]Press any key to exit...[/]");
        Console.ReadKey();
    }

    /// <summary>
    /// Initialize the testing environment
    /// </summary>
    private static async Task InitializeAsync()
    {
        _formatter = new OutputFormatter();
        _testRunner = new Core.TestRunner();
        _navigation = new NavigationEngine();

        AnsiConsole.MarkupLine("[bold blue]🚀 Initializing Interactive Testing Environment...[/]");

        // Register our example command
        var methodAnalyzerCommand = new MethodBlockAnalyzerCommand();
        _navigation.RegisterCommand(methodAnalyzerCommand);

        // Discover any other commands
        await _navigation.DiscoverCommandsAsync();

        AnsiConsole.MarkupLine("[green]✅ Environment initialized successfully![/]\n");
    }

    /// <summary>
    /// Show welcome message and summary
    /// </summary>
    private static void ShowWelcome()
    {
        var panel = new Panel(@"
[bold blue]🧪 Knowledge Network Interactive Testing Environment[/]

This tool lets you test individual components with real inputs and see the results.
Perfect for understanding how each part of the system works!

[bold yellow]What you can do:[/]
• Test components with real C# code
• See actual execution results
• Compare expected vs actual outcomes  
• Browse and search available tests
• Get detailed performance metrics

[bold green]Let's start testing![/]")
            .Header("[bold]Welcome to Interactive Testing[/]")
            .BorderColor(Color.Blue)
            .Padding(1, 0);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Show summary of available commands
        _navigation.DisplaySummary();
    }

    /// <summary>
    /// Run the main interactive session
    /// </summary>
    private static async Task RunInteractiveSessionAsync()
    {
        while (true)
        {
            try
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold blue]What would you like to do?[/]")
                        .AddChoices(new[] {
                            "🧪 Run a test",
                            "📋 List all commands", 
                            "🔍 Search commands",
                            "📊 View test results",
                            "❓ Help",
                            "🚪 Exit"
                        })
                );

                switch (choice)
                {
                    case "🧪 Run a test":
                        await RunTestInteractiveAsync();
                        break;

                    case "📋 List all commands":
                        _navigation.DisplayAllCommands();
                        break;

                    case "🔍 Search commands":
                        await SearchCommandsAsync();
                        break;

                    case "📊 View test results":
                        ShowTestResultsInfo();
                        break;

                    case "❓ Help":
                        ShowHelp();
                        break;

                    case "🚪 Exit":
                        AnsiConsole.MarkupLine("[yellow]👋 Thanks for testing! Goodbye![/]");
                        return;
                }

                AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                _formatter.DisplayError("Session Error", ex);
                AnsiConsole.MarkupLine("[yellow]Press any key to continue...[/]");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// Run a test interactively
    /// </summary>
    private static async Task RunTestInteractiveAsync()
    {
        var command = await _navigation.BrowseCommandsInteractivelyAsync();
        if (command == null)
        {
            _formatter.DisplayInfo("No command selected");
            return;
        }

        AnsiConsole.MarkupLine($"[green]Selected: {command.Name}[/]");
        
        // Run the test interactively
        var result = await _testRunner.RunInteractiveTestAsync(command);
        
        if (result != null)
        {
            _formatter.DisplayInfo($"Test completed! Overall success: {(result.OverallSuccess ? "✅ Yes" : "❌ No")}");
        }
    }

    /// <summary>
    /// Search for commands interactively
    /// </summary>
    private static async Task SearchCommandsAsync()
    {
        var searchTerm = AnsiConsole.Ask<string>("Enter search term:");
        var results = _navigation.SearchCommands(searchTerm);

        if (results.Any())
        {
            _formatter.DisplaySearchResults(searchTerm, results);
            
            if (AnsiConsole.Confirm("Would you like to run one of these tests?"))
            {
                var selectedCommand = _formatter.PromptCommandSelection(results);
                if (selectedCommand != null)
                {
                    await _testRunner.RunInteractiveTestAsync(selectedCommand);
                }
            }
        }
        else
        {
            _formatter.DisplayWarning("No Results", $"No commands found matching '{searchTerm}'");
        }
    }

    /// <summary>
    /// Show information about test results
    /// </summary>
    private static void ShowTestResultsInfo()
    {
        var info = @"
[bold]Test Results Features:[/]

• [green]✅ Success Indicators[/] - Clear visual feedback on test outcomes
• [blue]📊 Performance Metrics[/] - Execution time and resource usage
• [yellow]⚖️  Expected vs Actual[/] - Side-by-side comparison of results
• [red]🔍 Detailed Analysis[/] - In-depth breakdown of what happened
• [cyan]📋 Structured Data[/] - JSON formatted output for complex results

[dim]Results are displayed immediately after each test execution.[/]";

        var panel = new Panel(info)
            .Header("[bold]📊 About Test Results[/]")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Show help information
    /// </summary>
    private static void ShowHelp()
    {
        var help = @"
[bold blue]🎯 How to Use This Tool[/]

[bold yellow]1. Run a Test:[/]
   • Select a component to test (like Method Block Analyzer)
   • Choose a test scenario (Simple Method, Conditional, etc.)
   • Watch the test execute with real-time feedback
   • See the results compared with expected outcomes

[bold yellow]2. Understanding Results:[/]
   • [green]Green ✅[/] = Test passed
   • [red]Red ❌[/] = Test failed  
   • [yellow]Yellow numbers[/] = Performance metrics
   • [blue]Blue text[/] = Component information

[bold yellow]3. What Gets Tested:[/]
   • Real C# code analysis using your actual components
   • Control Flow Graph (CFG) generation
   • Method complexity calculations
   • Performance timing

[bold yellow]4. Example Flow:[/]
   1. Choose 'Method Block Analyzer' 
   2. Pick 'Simple Linear Method' scenario
   3. Watch it analyze the Add() method
   4. See: nodes=3, edges=2, complexity=1
   5. Compare with expected results

[dim]This tool helps you understand exactly what each component does by seeing it work![/]";

        var panel = new Panel(help)
            .Header("[bold]❓ Help & Usage Guide[/]")
            .BorderColor(Color.Green)
            .Expand();

        AnsiConsole.Write(panel);
    }
}