using Spectre.Console;
using System.Text.Json;

namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Handles all visual output formatting using Spectre.Console for beautiful terminal display.
/// This class makes the testing experience visually appealing and informative.
/// </summary>
public class OutputFormatter
{
    #region Public Display Methods

    /// <summary>
    /// Display the header for a test execution
    /// </summary>
    public void DisplayTestHeader(CommandBase command, TestScenario scenario)
    {
        var panel = new Panel($"[bold blue]{command.Name}[/]\n[dim]{command.Description}[/]")
            .Header($"[bold green]🧪 Testing: {scenario.Name}[/]")
            .BorderColor(Color.Blue)
            .Padding(1, 0);

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

        // Show scenario details
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Property[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Component", $"[yellow]{command.ComponentPath}[/]");
        table.AddRow("Scenario", $"[cyan]{scenario.Name}[/]");
        table.AddRow("Description", scenario.Description.EscapeMarkup());

        if (scenario.Input.Any())
        {
            table.AddRow("Input Parameters", $"[dim]{scenario.Input.Count} parameter(s)[/]");
        }

        if (scenario.ExpectedOutput != null && scenario.ExpectedOutput.Any())
        {
            table.AddRow("Expected Outputs", $"[dim]{scenario.ExpectedOutput.Count} expectation(s)[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display batch test header
    /// </summary>
    public void DisplayBatchHeader(CommandBase command, int scenarioCount)
    {
        var rule = new Rule($"[bold blue]🚀 Batch Testing: {command.Name}[/]")
            .LeftJustified()
            .RuleStyle("blue");

        AnsiConsole.Write(rule);
        AnsiConsole.MarkupLine($"[dim]Running {scenarioCount} scenarios...[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display progress message with optional styling
    /// </summary>
    public void ShowProgress(string message, int? percentage = null)
    {
        if (percentage.HasValue)
        {
            AnsiConsole.MarkupLine($"[yellow]⚡ {message} ({percentage}%)[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]⚡ {message}[/]");
        }
    }

    /// <summary>
    /// Display success message with green styling
    /// </summary>
    public void DisplaySuccess(string title, string message)
    {
        var panel = new Panel($"[green]{message.EscapeMarkup()}[/]")
            .Header($"[bold green]✅ {title}[/]")
            .BorderColor(Color.Green);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display error message with red styling
    /// </summary>
    public void DisplayError(string title, string message)
    {
        var panel = new Panel($"[red]{message.EscapeMarkup()}[/]")
            .Header($"[bold red]❌ {title}[/]")
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display error with exception details
    /// </summary>
    public void DisplayError(string title, Exception exception)
    {
        var content = $"[red]{exception.Message.EscapeMarkup()}[/]";
        
        if (exception.InnerException != null)
        {
            content += $"\n[dim red]Inner: {exception.InnerException.Message.EscapeMarkup()}[/]";
        }

        var panel = new Panel(content)
            .Header($"[bold red]❌ {title}[/]")
            .BorderColor(Color.Red);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display warning message with yellow styling
    /// </summary>
    public void DisplayWarning(string title, string message)
    {
        var panel = new Panel($"[yellow]{message.EscapeMarkup()}[/]")
            .Header($"[bold yellow]⚠️  {title}[/]")
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display informational message
    /// </summary>
    public void DisplayInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]ℹ️  {message.EscapeMarkup()}[/]");
    }

    #endregion

    #region Test Result Display

    /// <summary>
    /// Display execution metrics in a formatted table
    /// </summary>
    public void DisplayMetrics(TestResult result)
    {
        var table = new Table()
            .Title("[bold]📊 Execution Metrics[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Metric[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Execution Time", $"[cyan]{result.Duration.TotalMilliseconds:F2} ms[/]");
        table.AddRow("Success", result.Success ? "[green]✅ Yes[/]" : "[red]❌ No[/]");
        table.AddRow("Component", $"[yellow]{result.ComponentName}[/]");

        // Add custom metrics
        foreach (var metric in result.Metrics)
        {
            var value = FormatMetricValue(metric.Value);
            table.AddRow(metric.Key, value);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display warnings if any
    /// </summary>
    public void DisplayWarnings(List<string> warnings)
    {
        if (!warnings.Any()) return;

        var list = new List<string>();
        foreach (var warning in warnings)
        {
            list.Add($"[yellow]• {warning.EscapeMarkup()}[/]");
        }

        var panel = new Panel(string.Join("\n", list))
            .Header("[bold yellow]⚠️  Warnings[/]")
            .BorderColor(Color.Yellow);

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Display result data in a formatted way
    /// </summary>
    public async Task DisplayResultData(object data)
    {
        var panel = new Panel("")
            .Header("[bold]📋 Result Data[/]")
            .BorderColor(Color.Blue);

        try
        {
            string content;
            
            // Handle different data types
            switch (data)
            {
                case string str:
                    content = str.EscapeMarkup();
                    break;
                
                case Dictionary<string, object> dict:
                    content = FormatDictionary(dict);
                    break;
                
                default:
                    // Try to serialize as JSON for structured display
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var json = JsonSerializer.Serialize(data, jsonOptions);
                    content = $"[dim]{json.EscapeMarkup()}[/]";
                    break;
            }

            panel = new Panel(content)
                .Header("[bold]📋 Result Data[/]")
                .BorderColor(Color.Blue)
                .Expand();
            AnsiConsole.Write(panel);
        }
        catch (Exception ex)
        {
            DisplayError("Failed to display result data", ex.Message);
        }
    }

    #endregion

    #region Comparison Display

    /// <summary>
    /// Display comparison results between actual and expected outcomes
    /// </summary>
    public void DisplayComparison(ResultComparison comparison)
    {
        var color = comparison.OverallMatch ? Color.Green : Color.Red;
        var icon = comparison.OverallMatch ? "✅" : "❌";
        
        var panel = new Panel("")
            .Header($"[bold]{icon} Comparison Results ({comparison.MatchPercentage:F1}% match)[/]")
            .BorderColor(color);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Field[/]")
            .AddColumn("[bold]Expected[/]")
            .AddColumn("[bold]Actual[/]")
            .AddColumn("[bold]Match[/]");

        foreach (var field in comparison.FieldComparisons)
        {
            var matchIcon = field.IsMatch ? "[green]✅[/]" : "[red]❌[/]";
            var expectedValue = FormatComparisonValue(field.ExpectedValue);
            var actualValue = FormatComparisonValue(field.ActualValue);

            table.AddRow(
                field.FieldName,
                expectedValue,
                actualValue,
                matchIcon
            );
        }

        AnsiConsole.Write(panel);
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    #endregion

    #region Interactive Prompts

    /// <summary>
    /// Prompt user to select a test scenario
    /// </summary>
    public TestScenario? PromptScenarioSelection(List<TestScenario> scenarios)
    {
        if (!scenarios.Any()) return null;

        var choices = scenarios.Select(s => $"{s.Name} - {s.Description}").ToArray();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold blue]Select a test scenario:[/]")
                .AddChoices(choices)
                .HighlightStyle(new Style(foreground: Color.Blue))
        );

        var selectedIndex = Array.IndexOf(choices, selection);
        return scenarios[selectedIndex];
    }

    /// <summary>
    /// Prompt for execution options
    /// </summary>
    public Dictionary<string, object> PromptExecutionOptions()
    {
        var options = new Dictionary<string, object>();

        var includeVerbose = AnsiConsole.Confirm("Include verbose output?");
        options["verboseOutput"] = includeVerbose;

        var includeMetrics = AnsiConsole.Confirm("Include performance metrics?");
        options["includeMetrics"] = includeMetrics;

        if (AnsiConsole.Confirm("Add custom options?"))
        {
            var customKey = AnsiConsole.Ask<string>("Enter option name:");
            var customValue = AnsiConsole.Ask<string>("Enter option value:");
            options[customKey] = customValue;
        }

        return options;
    }

    #endregion

    #region Test Summary Display

    /// <summary>
    /// Display summary of a single test execution
    /// </summary>
    public void DisplayTestSummary(TestExecutionResult executionResult)
    {
        var color = executionResult.OverallSuccess ? Color.Green : Color.Red;
        var icon = executionResult.OverallSuccess ? "✅" : "❌";

        var rule = new Rule($"[bold]{icon} Test Complete[/]")
            .RuleStyle(color)
            .LeftJustified();

        AnsiConsole.Write(rule);

        var table = new Table()
            .Border(TableBorder.None)
            .AddColumn("")
            .AddColumn("")
            .HideHeaders();

        table.AddRow("Duration:", $"[cyan]{executionResult.TotalDuration.TotalMilliseconds:F2} ms[/]");
        table.AddRow("Success:", executionResult.OverallSuccess ? "[green]Yes[/]" : "[red]No[/]");
        
        if (executionResult.HasComparison)
        {
            table.AddRow("Comparison:", $"[yellow]{executionResult.Comparison!.MatchPercentage:F1}% match[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display summary of batch test execution
    /// </summary>
    public void DisplayBatchSummary(List<TestExecutionResult> results)
    {
        var successCount = results.Count(r => r.OverallSuccess);
        var totalCount = results.Count;
        var successRate = (double)successCount / totalCount * 100;

        var color = successRate >= 80 ? Color.Green : successRate >= 50 ? Color.Yellow : Color.Red;

        var rule = new Rule($"[bold]📊 Batch Summary: {successCount}/{totalCount} Passed ({successRate:F1}%)[/]")
            .RuleStyle(color)
            .LeftJustified();

        AnsiConsole.Write(rule);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Scenario[/]")
            .AddColumn("[bold]Result[/]")
            .AddColumn("[bold]Duration[/]")
            .AddColumn("[bold]Comparison[/]");

        foreach (var result in results)
        {
            var resultIcon = result.OverallSuccess ? "[green]✅[/]" : "[red]❌[/]";
            var duration = $"[cyan]{result.TotalDuration.TotalMilliseconds:F0}ms[/]";
            var comparison = result.HasComparison 
                ? $"[yellow]{result.Comparison!.MatchPercentage:F0}%[/]"
                : "[dim]N/A[/]";

            table.AddRow(
                result.Scenario.Name,
                resultIcon,
                duration,
                comparison
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    #endregion

    #region Navigation and Search Display

    /// <summary>
    /// Prompt for main navigation choice
    /// </summary>
    public string PromptMainNavigation(List<string> categories)
    {
        var choices = new List<string> { "list", "search", "category", "recent", "exit" };
        
        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold blue]What would you like to do?[/]")
                .AddChoices(choices)
                .UseConverter(choice => choice switch
                {
                    "list" => "📋 List all commands",
                    "search" => "🔍 Search commands",  
                    "category" => "📁 Browse by category",
                    "recent" => "🕒 Recent commands",
                    "exit" => "❌ Exit",
                    _ => choice
                })
        );
    }

    /// <summary>
    /// Display search results
    /// </summary>
    public void DisplaySearchResults(string searchTerm, List<CommandBase> results)
    {
        var panel = new Panel("")
            .Header($"[bold green]🔍 Search Results for '{searchTerm.EscapeMarkup()}'[/]")
            .BorderColor(Color.Green);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Category[/]")
            .AddColumn("[bold]Description[/]");

        foreach (var command in results)
        {
            table.AddRow(
                $"[yellow]{command.Name}[/]",
                $"[blue]{command.Category}[/]",
                $"[dim]{command.Description.EscapeMarkup()}[/]"
            );
        }

        AnsiConsole.Write(panel);
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display commands by category
    /// </summary>
    public void DisplayCommandsByCategory(string category, List<CommandBase> commands)
    {
        var panel = new Panel("")
            .Header($"[bold yellow]📁 {category} Commands[/]")
            .BorderColor(Color.Yellow);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Description[/]")
            .AddColumn("[bold]Component Path[/]");

        foreach (var command in commands.OrderBy(c => c.Name))
        {
            table.AddRow(
                $"[yellow]{command.Name}[/]",
                $"[dim]{command.Description.EscapeMarkup()}[/]",
                $"[blue]{command.ComponentPath}[/]"
            );
        }

        AnsiConsole.Write(panel);
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Prompt user to select a command from a list
    /// </summary>
    public CommandBase? PromptCommandSelection(List<CommandBase> commands)
    {
        if (!commands.Any()) return null;

        var choices = commands.Select(c => $"{c.Name} - {c.Description}").ToArray();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold blue]Select a command to test:[/]")
                .AddChoices(choices)
                .HighlightStyle(new Style(foreground: Color.Yellow))
        );

        var selectedIndex = Array.IndexOf(choices, selection);
        return commands[selectedIndex];
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Format a metric value for display
    /// </summary>
    private string FormatMetricValue(object value)
    {
        return value switch
        {
            int i => $"[cyan]{i:N0}[/]",
            long l => $"[cyan]{l:N0}[/]",
            double d => $"[cyan]{d:F2}[/]",
            float f => $"[cyan]{f:F2}[/]",
            bool b => b ? "[green]✅ True[/]" : "[red]❌ False[/]",
            TimeSpan ts => $"[cyan]{ts.TotalMilliseconds:F2} ms[/]",
            _ => $"[yellow]{value?.ToString()?.EscapeMarkup() ?? "null"}[/]"
        };
    }

    /// <summary>
    /// Format a comparison value for display
    /// </summary>
    private string FormatComparisonValue(object? value)
    {
        if (value == null) return "[dim]null[/]";

        return value switch
        {
            string s => $"[yellow]'{s.EscapeMarkup()}'[/]",
            bool b => b ? "[green]true[/]" : "[red]false[/]",
            int or long or double or float => $"[cyan]{value}[/]",
            _ => $"[dim]{value.ToString()?.EscapeMarkup()}[/]"
        };
    }

    /// <summary>
    /// Format a dictionary for display
    /// </summary>
    private string FormatDictionary(Dictionary<string, object> dict)
    {
        var lines = new List<string>();
        
        foreach (var kvp in dict)
        {
            var value = FormatMetricValue(kvp.Value);
            lines.Add($"[bold]{kvp.Key}:[/] {value}");
        }

        return string.Join("\n", lines);
    }

    #endregion
}