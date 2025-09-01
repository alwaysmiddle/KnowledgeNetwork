using Spectre.Console;

namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Orchestrates test execution with visual feedback and result comparison.
/// This is the engine that runs test scenarios and displays results beautifully.
/// </summary>
public class TestRunner
{
    private readonly OutputFormatter _formatter = new();

    /// <summary>
    /// Execute a single test scenario with full visual feedback
    /// </summary>
    /// <param name="command">The test command to execute</param>
    /// <param name="scenario">The scenario to run</param>
    /// <returns>Test execution result with comparison data</returns>
    public async Task<TestExecutionResult> RunTestAsync(CommandBase command, TestScenario scenario)
    {
        var executionResult = new TestExecutionResult
        {
            Command = command.GetInfo(),
            Scenario = scenario,
            StartTime = DateTime.UtcNow
        };

        try
        {
            // Display test header
            _formatter.DisplayTestHeader(command, scenario);

            // Validate scenario
            if (!ValidateScenario(command, scenario))
            {
                executionResult.Result = CreateFailureResult("Scenario validation failed", command);
                executionResult.EndTime = DateTime.UtcNow;
                return executionResult;
            }

            // Execute the test with progress indication
            _formatter.ShowProgress("Initializing test execution...");
            
            var (result, duration) = await MeasureExecutionAsync(async () =>
            {
                _formatter.ShowProgress("Running test scenario...");
                return await command.ExecuteAsync(scenario);
            });

            result.Duration = duration;
            executionResult.Result = result;
            executionResult.EndTime = DateTime.UtcNow;

            // Display results
            await DisplayResults(executionResult);

            // Compare with expected results if available
            if (scenario.ExpectedOutput != null && scenario.ExpectedOutput.Count > 0)
            {
                _formatter.ShowProgress("Comparing results with expected outcomes...");
                await CompareWithExpected(executionResult);
            }

            _formatter.DisplayTestSummary(executionResult);

        }
        catch (Exception ex)
        {
            executionResult.Result = CreateFailureResult($"Test execution failed: {ex.Message}", command);
            executionResult.EndTime = DateTime.UtcNow;
            executionResult.Exception = ex;
            
            _formatter.DisplayError("Test Execution Failed", ex);
        }

        return executionResult;
    }

    /// <summary>
    /// Run multiple scenarios in batch mode
    /// </summary>
    /// <param name="command">The test command to execute</param>
    /// <param name="scenarios">List of scenarios to run</param>
    /// <returns>List of execution results</returns>
    public async Task<List<TestExecutionResult>> RunBatchTestsAsync(CommandBase command, List<TestScenario> scenarios)
    {
        var results = new List<TestExecutionResult>();
        
        _formatter.DisplayBatchHeader(command, scenarios.Count);

        for (int i = 0; i < scenarios.Count; i++)
        {
            var scenario = scenarios[i];
            
            _formatter.ShowProgress($"Running scenario {i + 1} of {scenarios.Count}: {scenario.Name}");
            
            var result = await RunTestAsync(command, scenario);
            results.Add(result);

            // Brief pause between tests for readability
            await Task.Delay(500);
        }

        _formatter.DisplayBatchSummary(results);
        return results;
    }

    /// <summary>
    /// Interactive test execution with user prompts
    /// </summary>
    /// <param name="command">The test command to execute</param>
    /// <returns>Test execution result</returns>
    public async Task<TestExecutionResult?> RunInteractiveTestAsync(CommandBase command)
    {
        try
        {
            // Get available scenarios
            _formatter.ShowProgress("Loading available test scenarios...");
            var scenarios = await command.GetAvailableScenariosAsync();

            if (!scenarios.Any())
            {
                _formatter.DisplayWarning("No test scenarios found", 
                    $"No scenarios available for {command.Name}. Please create scenario files in the Scenarios folder.");
                return null;
            }

            // Let user choose scenario
            var selectedScenario = _formatter.PromptScenarioSelection(scenarios);
            if (selectedScenario == null)
            {
                _formatter.DisplayInfo("Test cancelled by user");
                return null;
            }

            // Ask for execution options
            var options = _formatter.PromptExecutionOptions();
            
            // Merge options into scenario
            foreach (var option in options)
            {
                selectedScenario.Options[option.Key] = option.Value;
            }

            // Execute the selected scenario
            return await RunTestAsync(command, selectedScenario);
        }
        catch (Exception ex)
        {
            _formatter.DisplayError("Interactive test failed", ex);
            return null;
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Validate that a scenario is properly configured
    /// </summary>
    private bool ValidateScenario(CommandBase command, TestScenario scenario)
    {
        if (scenario == null)
        {
            _formatter.DisplayError("Invalid scenario", new ArgumentNullException(nameof(scenario)));
            return false;
        }

        if (string.IsNullOrWhiteSpace(scenario.Name))
        {
            _formatter.DisplayError("Scenario validation failed", 
                new ArgumentException("Scenario must have a name"));
            return false;
        }

        if (scenario.Input == null || !scenario.Input.Any())
        {
            _formatter.DisplayWarning("Empty input", 
                "Scenario has no input data. This may be intentional for some tests.");
        }

        return true;
    }

    /// <summary>
    /// Display test results with appropriate formatting
    /// </summary>
    private async Task DisplayResults(TestExecutionResult executionResult)
    {
        var result = executionResult.Result;
        
        if (result.Success)
        {
            _formatter.DisplaySuccess("Test Completed Successfully", result.Message);
        }
        else
        {
            _formatter.DisplayError("Test Failed", result.Message);
        }

        // Display execution metrics
        _formatter.DisplayMetrics(result);

        // Display warnings if any
        if (result.Warnings.Any())
        {
            _formatter.DisplayWarnings(result.Warnings);
        }

        // Display result data if available
        if (result.Data != null)
        {
            await _formatter.DisplayResultData(result.Data);
        }
    }

    /// <summary>
    /// Compare actual results with expected outcomes
    /// </summary>
    private async Task CompareWithExpected(TestExecutionResult executionResult)
    {
        try
        {
            var comparison = await CompareResults(
                executionResult.Result, 
                executionResult.Scenario.ExpectedOutput!
            );

            executionResult.Comparison = comparison;
            _formatter.DisplayComparison(comparison);
        }
        catch (Exception ex)
        {
            _formatter.DisplayError("Result comparison failed", ex);
        }
    }

    /// <summary>
    /// Compare actual vs expected results
    /// </summary>
    private async Task<ResultComparison> CompareResults(TestResult actual, Dictionary<string, object> expected)
    {
        var comparison = new ResultComparison
        {
            ActualResult = actual,
            ExpectedValues = expected,
            ComparisonTime = DateTime.UtcNow
        };

        // Compare each expected value
        foreach (var expectedKvp in expected)
        {
            var fieldName = expectedKvp.Key;
            var expectedValue = expectedKvp.Value;

            // Extract actual value (this will depend on the result structure)
            var actualValue = ExtractActualValue(actual, fieldName);

            var fieldComparison = new FieldComparison
            {
                FieldName = fieldName,
                ExpectedValue = expectedValue,
                ActualValue = actualValue,
                IsMatch = CompareValues(expectedValue, actualValue)
            };

            comparison.FieldComparisons.Add(fieldComparison);
        }

        comparison.OverallMatch = comparison.FieldComparisons.All(fc => fc.IsMatch);
        return comparison;
    }

    /// <summary>
    /// Extract a field value from the test result
    /// </summary>
    private object? ExtractActualValue(TestResult result, string fieldName)
    {
        // Try to get from Metrics first
        if (result.Metrics.ContainsKey(fieldName))
        {
            return result.Metrics[fieldName];
        }

        // Try to extract from Data if it's a dictionary
        if (result.Data is Dictionary<string, object> dataDict && dataDict.ContainsKey(fieldName))
        {
            return dataDict[fieldName];
        }

        // Try reflection on the Data object
        if (result.Data != null)
        {
            var property = result.Data.GetType().GetProperty(fieldName);
            if (property != null)
            {
                return property.GetValue(result.Data);
            }
        }

        return null;
    }

    /// <summary>
    /// Compare two values for equality
    /// </summary>
    private bool CompareValues(object expected, object? actual)
    {
        if (actual == null && expected == null) return true;
        if (actual == null || expected == null) return false;

        // Handle numeric comparisons
        if (IsNumeric(expected) && IsNumeric(actual))
        {
            return Convert.ToDouble(expected).Equals(Convert.ToDouble(actual));
        }

        // Handle string comparisons
        if (expected is string && actual is string)
        {
            return string.Equals(expected.ToString(), actual.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        // Default comparison
        return expected.Equals(actual);
    }

    /// <summary>
    /// Check if a value is numeric
    /// </summary>
    private bool IsNumeric(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    /// <summary>
    /// Measure execution time of an async operation
    /// </summary>
    private async Task<(T result, TimeSpan duration)> MeasureExecutionAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Create a failure test result
    /// </summary>
    private TestResult CreateFailureResult(string message, CommandBase command)
    {
        return new TestResult
        {
            Success = false,
            Message = message,
            ExecutionTime = DateTime.UtcNow,
            ComponentName = command.Name,
            Duration = TimeSpan.Zero
        };
    }

    #endregion
}