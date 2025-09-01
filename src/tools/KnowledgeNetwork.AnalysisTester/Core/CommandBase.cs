using System.Text.Json;
using Spectre.Console;

namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Abstract base class for all interactive test commands.
/// Provides the contract and common functionality for testing individual components.
/// </summary>
public abstract class CommandBase
{
    #region Abstract Properties - Must be implemented by each command
    
    /// <summary>
    /// Display name for this test command (e.g., "Method Block Analyzer")
    /// </summary>
    public abstract string Name { get; }
    
    /// <summary>
    /// Brief description of what this command tests
    /// </summary>
    public abstract string Description { get; }
    
    /// <summary>
    /// Path to the component being tested (e.g., "Domains.Code/Analyzers/Blocks/CSharpMethodBlockAnalyzer")
    /// </summary>
    public abstract string ComponentPath { get; }
    
    /// <summary>
    /// Category for grouping commands (e.g., "Analyzers", "Controllers", "Models")
    /// </summary>
    public abstract string Category { get; }
    
    #endregion

    #region Abstract Methods - Core functionality each command must implement
    
    /// <summary>
    /// Execute the test command with the given scenario
    /// This is where the actual component testing happens
    /// </summary>
    /// <param name="scenario">Test scenario containing inputs and configuration</param>
    /// <returns>Results of the test execution</returns>
    public abstract Task<TestResult> ExecuteAsync(TestScenario scenario);
    
    /// <summary>
    /// Get all available test scenarios for this command
    /// </summary>
    /// <returns>List of available test scenarios</returns>
    public abstract Task<List<TestScenario>> GetAvailableScenariosAsync();
    
    #endregion

    #region Protected Virtual Methods - Can be overridden for customization
    
    /// <summary>
    /// Validate the input scenario before execution
    /// Override to add component-specific validation
    /// </summary>
    /// <param name="scenario">Scenario to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    protected virtual bool ValidateScenario(TestScenario scenario)
    {
        if (scenario == null)
        {
            AnsiConsole.MarkupLine("[red]Error: Scenario cannot be null[/]");
            return false;
        }

        if (string.IsNullOrWhiteSpace(scenario.Name))
        {
            AnsiConsole.MarkupLine("[red]Error: Scenario must have a name[/]");
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// Display execution progress - override for custom progress display
    /// </summary>
    /// <param name="message">Progress message to display</param>
    /// <param name="percentage">Optional percentage complete (0-100)</param>
    protected virtual void ShowProgress(string message, int? percentage = null)
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
    /// Display error messages in a consistent format
    /// </summary>
    /// <param name="error">Error message to display</param>
    /// <param name="exception">Optional exception for additional context</param>
    protected virtual void ShowError(string error, Exception? exception = null)
    {
        AnsiConsole.MarkupLine($"[red]❌ Error: {error}[/]");
        
        if (exception != null)
        {
            AnsiConsole.MarkupLine($"[dim red]   Details: {exception.Message}[/]");
        }
    }
    
    /// <summary>
    /// Display success messages
    /// </summary>
    /// <param name="message">Success message to display</param>
    protected virtual void ShowSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✅ {message}[/]");
    }
    
    #endregion

    #region Public Helper Methods
    
    /// <summary>
    /// Load a test scenario from a JSON file
    /// </summary>
    /// <param name="scenarioPath">Path to the scenario file</param>
    /// <returns>Loaded test scenario or null if failed</returns>
    public async Task<TestScenario?> LoadScenarioFromFileAsync(string scenarioPath)
    {
        try
        {
            if (!File.Exists(scenarioPath))
            {
                ShowError($"Scenario file not found: {scenarioPath}");
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(scenarioPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var scenario = JsonSerializer.Deserialize<TestScenario>(jsonContent, options);
            
            if (scenario != null && ValidateScenario(scenario))
            {
                return scenario;
            }
            
            ShowError($"Failed to load or validate scenario from: {scenarioPath}");
            return null;
        }
        catch (Exception ex)
        {
            ShowError($"Exception loading scenario from {scenarioPath}", ex);
            return null;
        }
    }
    
    /// <summary>
    /// Get the base directory for this command's test scenarios
    /// </summary>
    /// <returns>Path to scenarios directory</returns>
    public string GetScenariosDirectory()
    {
        // Try relative path first (works when files are copied to output directory)
        var relativePath = Path.Combine(
            "TestHarnesses",
            ComponentPath,
            "Scenarios"
        );
        
        // Check if relative path exists from current directory
        if (Directory.Exists(relativePath))
        {
            return Path.GetFullPath(relativePath);
        }
        
        // Fall back to AppContext.BaseDirectory (for runtime execution)
        var runtimePath = Path.Combine(
            AppContext.BaseDirectory,
            "TestHarnesses",
            ComponentPath,
            "Scenarios"
        );
        
        if (Directory.Exists(runtimePath))
        {
            return runtimePath;
        }
        
        // If neither exists, return the runtime path for error reporting
        return runtimePath;
    }
    
    /// <summary>
    /// Get basic information about this command for display purposes
    /// </summary>
    /// <returns>Command information</returns>
    public CommandInfo GetInfo()
    {
        return new CommandInfo
        {
            Name = Name,
            Description = Description,
            ComponentPath = ComponentPath,
            Category = Category,
            ScenariosPath = GetScenariosDirectory()
        };
    }
    
    #endregion

    #region Utility Methods for subclasses
    
    /// <summary>
    /// Helper method to create a basic test result
    /// </summary>
    /// <param name="success">Whether the test succeeded</param>
    /// <param name="message">Result message</param>
    /// <param name="data">Optional result data</param>
    /// <returns>Test result object</returns>
    protected TestResult CreateResult(bool success, string message, object? data = null)
    {
        return new TestResult
        {
            Success = success,
            Message = message,
            Data = data,
            ExecutionTime = DateTime.UtcNow,
            ComponentName = Name
        };
    }
    
    /// <summary>
    /// Helper method to measure execution time of an operation
    /// </summary>
    /// <param name="operation">Operation to measure</param>
    /// <returns>Tuple of result and execution time</returns>
    protected async Task<(T result, TimeSpan duration)> MeasureExecutionAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        
        return (result, stopwatch.Elapsed);
    }
    
    #endregion
}