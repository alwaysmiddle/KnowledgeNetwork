using System.Text;
using Spectre.Console;
using KnowledgeNetwork.AnalysisTester.Models;
using KnowledgeNetwork.Domains.Code.Models;

namespace KnowledgeNetwork.AnalysisTester.Formatters;

/// <summary>
/// Formats analysis results for various output formats
/// </summary>
public class AnalysisResultFormatter
{
    /// <summary>
    /// Format single result as console table
    /// </summary>
    public void FormatAsConsoleTable(TestResult result)
    {
        // Header with file info
        var panel = new Panel($"[bold green]{result.FileName}[/]")
            .Header($"Analysis Results - {result.Language}")
            .BorderColor(result.Success ? Color.Green : Color.Red);

        AnsiConsole.Write(panel);

        // Summary statistics
        var summaryTable = new Table()
            .AddColumn("Metric")
            .AddColumn("Value")
            .BorderColor(Color.Blue);

        summaryTable.AddRow("Status", result.Success ? "[green]✓ Success[/]" : "[red]✗ Failed[/]");
        summaryTable.AddRow("Duration", $"{result.Duration.TotalMilliseconds:F2} ms");
        summaryTable.AddRow("Classes", result.ClassCount.ToString());
        summaryTable.AddRow("Methods", result.MethodCount.ToString());
        summaryTable.AddRow("Properties", result.PropertyCount.ToString());
        summaryTable.AddRow("Using Statements", result.UsingStatementCount.ToString());
        summaryTable.AddRow("CFG Blocks", result.CFGBlockCount.ToString());
        summaryTable.AddRow("CFG Edges", result.CFGEdgeCount.ToString());
        summaryTable.AddRow("Avg Complexity", $"{result.AvgCyclomaticComplexity:F2}");
        summaryTable.AddRow("Decision Points", result.TotalDecisionPoints.ToString());
        summaryTable.AddRow("Loops", result.TotalLoops.ToString());

        AnsiConsole.Write(summaryTable);

        // Classes details
        if (result.AnalysisResult?.Classes.Any() == true)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Classes:[/]");
            
            var classTable = new Table()
                .AddColumn("Name")
                .AddColumn("Namespace")
                .AddColumn("Modifiers")
                .AddColumn("Line");

            foreach (var cls in result.AnalysisResult.Classes)
            {
                classTable.AddRow(
                    cls.Name,
                    cls.Namespace,
                    cls.Modifiers,
                    cls.LineNumber.ToString());
            }

            AnsiConsole.Write(classTable);
        }

        // Methods details
        if (result.AnalysisResult?.Methods.Any() == true)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Methods:[/]");
            
            var methodTable = new Table()
                .AddColumn("Name")
                .AddColumn("Return Type")
                .AddColumn("Class")
                .AddColumn("Parameters")
                .AddColumn("Line");

            foreach (var method in result.AnalysisResult.Methods)
            {
                methodTable.AddRow(
                    method.Name,
                    method.ReturnType,
                    method.ClassName,
                    method.Parameters.Count.ToString(),
                    method.LineNumber.ToString());
            }

            AnsiConsole.Write(methodTable);
        }

        // Control Flow Graphs summary
        if (result.ControlFlowGraphs.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Control Flow Graphs:[/]");
            
            var cfgTable = new Table()
                .AddColumn("Method")
                .AddColumn("Blocks")
                .AddColumn("Edges")
                .AddColumn("Complexity")
                .AddColumn("Loops");

            foreach (var cfg in result.ControlFlowGraphs)
            {
                cfgTable.AddRow(
                    cfg.MethodName,
                    cfg.BasicBlocks.Count.ToString(),
                    cfg.Edges.Count.ToString(),
                    cfg.Metrics.CyclomaticComplexity.ToString(),
                    cfg.Metrics.LoopCount.ToString());
            }

            AnsiConsole.Write(cfgTable);
        }

        // Errors
        if (result.Errors.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold red]Errors:[/]");
            foreach (var error in result.Errors)
            {
                AnsiConsole.MarkupLine($"[red]• {error}[/]");
            }
        }
    }

    /// <summary>
    /// Format batch results as console table
    /// </summary>
    public void FormatBatchResultsAsConsoleTable(List<TestResult> results)
    {
        if (!results.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No results to display.[/]");
            return;
        }

        // Summary panel
        var successCount = results.Count(r => r.Success);
        var totalTime = TimeSpan.FromMilliseconds(results.Sum(r => r.Duration.TotalMilliseconds));
        
        var summaryPanel = new Panel($"[bold]Batch Analysis Results[/]\n" +
                                   $"Files: {results.Count} | " +
                                   $"Success: [green]{successCount}[/] | " +
                                   $"Failed: [red]{results.Count - successCount}[/] | " +
                                   $"Total Time: {totalTime.TotalSeconds:F2}s")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(summaryPanel);
        AnsiConsole.WriteLine();

        // Results table
        var table = new Table()
            .AddColumn("File")
            .AddColumn("Status")
            .AddColumn("Duration (ms)")
            .AddColumn("Classes")
            .AddColumn("Methods")
            .AddColumn("CFG Blocks")
            .AddColumn("Complexity");

        foreach (var result in results.OrderBy(r => r.FileName))
        {
            table.AddRow(
                result.FileName,
                result.Success ? "[green]✓[/]" : "[red]✗[/]",
                $"{result.Duration.TotalMilliseconds:F1}",
                result.ClassCount.ToString(),
                result.MethodCount.ToString(),
                result.CFGBlockCount.ToString(),
                $"{result.AvgCyclomaticComplexity:F1}");
        }

        AnsiConsole.Write(table);

        // Statistics summary
        AnsiConsole.WriteLine();
        var statsTable = new Table()
            .AddColumn("Metric")
            .AddColumn("Total")
            .AddColumn("Average")
            .AddColumn("Min")
            .AddColumn("Max");

        statsTable.AddRow("Classes", 
            results.Sum(r => r.ClassCount).ToString(),
            $"{results.Average(r => r.ClassCount):F1}",
            results.Min(r => r.ClassCount).ToString(),
            results.Max(r => r.ClassCount).ToString());

        statsTable.AddRow("Methods", 
            results.Sum(r => r.MethodCount).ToString(),
            $"{results.Average(r => r.MethodCount):F1}",
            results.Min(r => r.MethodCount).ToString(),
            results.Max(r => r.MethodCount).ToString());

        statsTable.AddRow("Duration (ms)", 
            $"{results.Sum(r => r.Duration.TotalMilliseconds):F1}",
            $"{results.Average(r => r.Duration.TotalMilliseconds):F1}",
            $"{results.Min(r => r.Duration.TotalMilliseconds):F1}",
            $"{results.Max(r => r.Duration.TotalMilliseconds):F1}");

        AnsiConsole.Write(statsTable);
    }

    /// <summary>
    /// Format single result as Markdown
    /// </summary>
    public string FormatAsMarkdown(TestResult result)
    {
        var md = new StringBuilder();
        
        md.AppendLine($"# Analysis Results: {result.FileName}");
        md.AppendLine();
        md.AppendLine($"**File:** {result.FilePath}");
        md.AppendLine($"**Language:** {result.Language}");
        md.AppendLine($"**Status:** {(result.Success ? "✓ Success" : "✗ Failed")}");
        md.AppendLine($"**Duration:** {result.Duration.TotalMilliseconds:F2} ms");
        md.AppendLine($"**Analyzed:** {result.AnalyzedAt:yyyy-MM-dd HH:mm:ss} UTC");
        md.AppendLine();

        // Summary Statistics
        md.AppendLine("## Summary Statistics");
        md.AppendLine();
        md.AppendLine("| Metric | Value |");
        md.AppendLine("|--------|-------|");
        md.AppendLine($"| Classes | {result.ClassCount} |");
        md.AppendLine($"| Methods | {result.MethodCount} |");
        md.AppendLine($"| Properties | {result.PropertyCount} |");
        md.AppendLine($"| Using Statements | {result.UsingStatementCount} |");
        md.AppendLine($"| CFG Blocks | {result.CFGBlockCount} |");
        md.AppendLine($"| CFG Edges | {result.CFGEdgeCount} |");
        md.AppendLine($"| Average Complexity | {result.AvgCyclomaticComplexity:F2} |");
        md.AppendLine($"| Decision Points | {result.TotalDecisionPoints} |");
        md.AppendLine($"| Loops | {result.TotalLoops} |");
        md.AppendLine();

        // Classes
        if (result.AnalysisResult?.Classes.Any() == true)
        {
            md.AppendLine("## Classes");
            md.AppendLine();
            md.AppendLine("| Name | Namespace | Modifiers | Line |");
            md.AppendLine("|------|-----------|-----------|------|");
            
            foreach (var cls in result.AnalysisResult.Classes)
            {
                md.AppendLine($"| {cls.Name} | {cls.Namespace} | {cls.Modifiers} | {cls.LineNumber} |");
            }
            md.AppendLine();
        }

        // Methods
        if (result.AnalysisResult?.Methods.Any() == true)
        {
            md.AppendLine("## Methods");
            md.AppendLine();
            md.AppendLine("| Name | Return Type | Class | Parameters | Line |");
            md.AppendLine("|------|-------------|-------|------------|------|");
            
            foreach (var method in result.AnalysisResult.Methods)
            {
                md.AppendLine($"| {method.Name} | {method.ReturnType} | {method.ClassName} | {method.Parameters.Count} | {method.LineNumber} |");
            }
            md.AppendLine();
        }

        // Control Flow Graphs
        if (result.ControlFlowGraphs.Any())
        {
            md.AppendLine("## Control Flow Graphs");
            md.AppendLine();
            md.AppendLine("| Method | Blocks | Edges | Complexity | Loops |");
            md.AppendLine("|--------|--------|-------|------------|-------|");
            
            foreach (var cfg in result.ControlFlowGraphs)
            {
                md.AppendLine($"| {cfg.MethodName} | {cfg.BasicBlocks.Count} | {cfg.Edges.Count} | {cfg.Metrics.CyclomaticComplexity} | {cfg.Metrics.LoopCount} |");
            }
            md.AppendLine();
        }

        // Errors
        if (result.Errors.Any())
        {
            md.AppendLine("## Errors");
            md.AppendLine();
            foreach (var error in result.Errors)
            {
                md.AppendLine($"- {error}");
            }
            md.AppendLine();
        }

        return md.ToString();
    }

    /// <summary>
    /// Format batch results as Markdown
    /// </summary>
    public string FormatBatchResultsAsMarkdown(List<TestResult> results)
    {
        var md = new StringBuilder();
        
        md.AppendLine("# Batch Analysis Results");
        md.AppendLine();
        
        var successCount = results.Count(r => r.Success);
        var totalTime = TimeSpan.FromMilliseconds(results.Sum(r => r.Duration.TotalMilliseconds));
        
        md.AppendLine("## Summary");
        md.AppendLine();
        md.AppendLine($"- **Total Files:** {results.Count}");
        md.AppendLine($"- **Successful:** {successCount}");
        md.AppendLine($"- **Failed:** {results.Count - successCount}");
        md.AppendLine($"- **Total Time:** {totalTime.TotalSeconds:F2}s");
        md.AppendLine($"- **Average Time:** {results.Average(r => r.Duration.TotalMilliseconds):F2}ms");
        md.AppendLine();

        // Results table
        md.AppendLine("## Detailed Results");
        md.AppendLine();
        md.AppendLine("| File | Status | Duration (ms) | Classes | Methods | CFG Blocks | Avg Complexity |");
        md.AppendLine("|------|--------|---------------|---------|---------|------------|----------------|");

        foreach (var result in results.OrderBy(r => r.FileName))
        {
            var status = result.Success ? "✓" : "✗";
            md.AppendLine($"| {result.FileName} | {status} | {result.Duration.TotalMilliseconds:F1} | {result.ClassCount} | {result.MethodCount} | {result.CFGBlockCount} | {result.AvgCyclomaticComplexity:F2} |");
        }

        return md.ToString();
    }

    /// <summary>
    /// Format CFG as ASCII visualization
    /// </summary>
    public void FormatCFGVisualization(CSharpControlFlowGraph cfg)
    {
        var panel = new Panel($"[bold blue]Control Flow Graph: {cfg.MethodName}[/]")
            .BorderColor(Color.Blue);

        AnsiConsole.Write(panel);

        if (!cfg.BasicBlocks.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No basic blocks found.[/]");
            return;
        }

        // Simple ASCII representation
        foreach (var block in cfg.BasicBlocks.OrderBy(b => b.Ordinal))
        {
            var blockType = block.Kind.ToString();
            var color = block.Kind switch
            {
                var k when k.ToString() == "Entry" => "green",
                var k when k.ToString() == "Exit" => "red",
                _ => "blue"
            };

            AnsiConsole.MarkupLine($"[{color}]┌─ Block {block.Id} ({blockType})[/]");
            
            foreach (var operation in block.Operations.Take(3)) // Show first 3 operations
            {
                var summary = operation.Summary.Length > 50 
                    ? operation.Summary.Substring(0, 47) + "..." 
                    : operation.Summary;
                AnsiConsole.MarkupLine($"[grey]│  {summary}[/]");
            }
            
            if (block.Operations.Count > 3)
            {
                AnsiConsole.MarkupLine($"[grey]│  ... and {block.Operations.Count - 3} more operations[/]");
            }

            // Show outgoing edges
            var outgoingEdges = cfg.Edges.Where(e => e.Source == block.Id).ToList();
            foreach (var edge in outgoingEdges)
            {
                var label = !string.IsNullOrEmpty(edge.Label) ? $" ({edge.Label})" : "";
                AnsiConsole.MarkupLine($"[yellow]│  → Block {edge.Target}{label}[/]");
            }

            AnsiConsole.MarkupLine($"[{color}]└─[/]");
            AnsiConsole.WriteLine();
        }

        // Metrics
        AnsiConsole.MarkupLine($"[bold]Metrics:[/] Complexity: {cfg.Metrics.CyclomaticComplexity}, " +
                              $"Blocks: {cfg.Metrics.BlockCount}, " +
                              $"Edges: {cfg.Metrics.EdgeCount}, " +
                              $"Loops: {cfg.Metrics.LoopCount}");
        AnsiConsole.WriteLine();
    }
}