using Spectre.Console;
using System.Text.Json;
using KnowledgeNetwork.Core.Models.Core;

namespace KnowledgeNetwork.AnalysisTester.Formatters;

/// <summary>
/// Formatter for displaying KnowledgeNode data in various formats
/// </summary>
public class KnowledgeNodeFormatter
{
    /// <summary>
    /// Display nodes in a hierarchical tree format with color coding
    /// </summary>
    /// <param name="nodes">List of nodes to display</param>
    public void DisplayNodesAsTree(List<KnowledgeNode> nodes)
    {
        if (!nodes.Any())
        {
            AnsiConsole.WriteLine("[yellow]No nodes to display[/]");
            return;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[green]Knowledge Graph Nodes[/]"));
        AnsiConsole.WriteLine();

        // Group nodes by type for better organization
        var methodNodes = nodes.Where(n => n.Type.Primary == "method").ToList();
        var blockNodes = nodes.Where(n => n.Type.Primary == "basic-block").ToList();
        var operationNodes = nodes.Where(n => n.Type.Primary == "operation").ToList();
        var viewNodes = nodes.Where(n => n.Type.Primary == "view").ToList();

        // Display methods first
        foreach (var methodNode in methodNodes)
        {
            DisplayMethodNode(methodNode, nodes);
        }

        // Display standalone views
        foreach (var viewNode in viewNodes)
        {
            DisplayViewNode(viewNode, nodes);
        }

        // Display summary
        DisplaySummary(nodes);
    }

    /// <summary>
    /// Display relationships as an edge list
    /// </summary>
    /// <param name="nodes">List of nodes to extract relationships from</param>
    public void DisplayRelationships(List<KnowledgeNode> nodes)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[blue]Relationships[/]"));
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("Source Node");
        table.AddColumn("Relationship");
        table.AddColumn("Target Node");
        table.AddColumn("Category");

        foreach (var node in nodes)
        {
            // Show edge references (edges would need to be passed separately to show details)
            foreach (var edgeId in node.OutgoingEdgeIds)
            {
                var sourceColor = GetNodeColor(node.Type.Primary);

                table.AddRow(
                    $"[{sourceColor}]{node.Label}[/]",
                    $"[cyan]edge-reference[/]",
                    $"[gray]{edgeId}[/]",
                    $"[yellow]reference[/]"
                );
            }
        }

        if (table.Rows.Count == 0)
        {
            AnsiConsole.WriteLine("[yellow]No relationships found[/]");
        }
        else
        {
            AnsiConsole.Write(table);
        }
    }

    /// <summary>
    /// Export nodes to JSON format
    /// </summary>
    /// <param name="nodes">Nodes to export</param>
    /// <param name="filePath">Path to save the JSON file</param>
    public async Task ExportToJsonAsync(List<KnowledgeNode> nodes, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(nodes, options);
        await File.WriteAllTextAsync(filePath, json);

        AnsiConsole.WriteLine($"[green]Exported {nodes.Count} nodes to {filePath}[/]");
    }

    /// <summary>
    /// Display node metrics in a summary table
    /// </summary>
    /// <param name="nodes">Nodes to analyze</param>
    public void DisplayMetrics(List<KnowledgeNode> nodes)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[yellow]Metrics Summary[/]"));
        AnsiConsole.WriteLine();

        var methodNodes = nodes.Where(n => n.Type.Primary == "method").ToList();
        
        var table = new Table();
        table.AddColumn("Method");
        table.AddColumn("Complexity");
        table.AddColumn("Blocks");
        table.AddColumn("Operations");
        table.AddColumn("Relationships");

        foreach (var method in methodNodes)
        {
            var blockCount = method.OutgoingEdgeIds.Count;
            var totalOperations = nodes
                .Where(n => n.Type.Primary == "operation" && 
                           n.Id.Contains(method.Id))
                .Count();

            var complexityColor = GetComplexityColor(method.Metrics.Complexity);
            
            table.AddRow(
                $"[{GetNodeColor(method.Type.Primary)}]{method.Label}[/]",
                $"[{complexityColor}]{method.Metrics.Complexity ?? 0}[/]",
                $"[blue]{blockCount}[/]",
                $"[cyan]{totalOperations}[/]",
                $"[magenta]{method.IncomingEdgeIds.Count + method.OutgoingEdgeIds.Count}[/]"
            );
        }

        if (table.Rows.Count == 0)
        {
            AnsiConsole.WriteLine("[yellow]No method metrics to display[/]");
        }
        else
        {
            AnsiConsole.Write(table);
        }
    }

    private void DisplayMethodNode(KnowledgeNode methodNode, List<KnowledgeNode> allNodes)
    {
        var methodColor = GetNodeColor(methodNode.Type.Primary);
        var complexityColor = GetComplexityColor(methodNode.Metrics.Complexity);
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write($"[{methodColor}]🔧 {methodNode.Label}[/] ");
        AnsiConsole.Write($"[{complexityColor}](Complexity: {methodNode.Metrics.Complexity ?? 0})[/]");
        AnsiConsole.WriteLine();

        // Display basic blocks (note: edges would need to be passed to show actual child nodes)
        // For now, find related nodes by ID pattern
        var relatedBlocks = allNodes.Where(n => 
            n.Type.Primary == "basic-block" && 
            n.Id.Contains(methodNode.Id)).ToList();
        
        foreach (var blockNode in relatedBlocks)
        {
            DisplayBasicBlockNode(blockNode, allNodes, "  ");
        }
    }

    private void DisplayBasicBlockNode(KnowledgeNode blockNode, List<KnowledgeNode> allNodes, string indent)
    {
        var blockColor = GetNodeColor(blockNode.Type.Primary);
        var icon = GetBlockIcon(blockNode.Properties);
        
        AnsiConsole.Write($"{indent}[{blockColor}]{icon} {blockNode.Label}[/]");
        
        // Show reachability
        if (blockNode.Properties.TryGetValue("isReachable", out var reachable) && reachable is bool isReachable)
        {
            var reachableColor = isReachable ? "green" : "red";
            AnsiConsole.Write($" [{reachableColor}]({(isReachable ? "reachable" : "unreachable")})[/]");
        }
        
        AnsiConsole.WriteLine();

        // Display operations if present and not too many
        var operations = allNodes.Where(n => 
            n.Type.Primary == "operation" && 
            n.Id.StartsWith($"op-{blockNode.Id}")).ToList();

        if (operations.Count > 0 && operations.Count <= 10) // Limit for readability
        {
            foreach (var operation in operations.Take(5)) // Show first 5
            {
                DisplayOperationNode(operation, indent + "    ");
            }
            
            if (operations.Count > 5)
            {
                AnsiConsole.WriteLine($"{indent}    [gray]... and {operations.Count - 5} more operations[/]");
            }
        }
        else if (operations.Count > 10)
        {
            AnsiConsole.WriteLine($"{indent}    [gray]({operations.Count} operations - too many to display)[/]");
        }
    }

    private void DisplayOperationNode(KnowledgeNode operationNode, string indent)
    {
        var operationColor = "gray";
        var summary = operationNode.Properties.TryGetValue("summary", out var summaryObj) 
            ? summaryObj?.ToString() ?? operationNode.Label 
            : operationNode.Label;

        AnsiConsole.WriteLine($"{indent}[{operationColor}]• {summary}[/]");
    }

    private void DisplayViewNode(KnowledgeNode viewNode, List<KnowledgeNode> allNodes)
    {
        var viewColor = GetNodeColor(viewNode.Type.Primary);
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write($"[{viewColor}]👁️ View: {viewNode.Label}[/]");
        AnsiConsole.Write($" [gray]({viewNode.OutgoingEdgeIds.Count} edge references)[/]");
        AnsiConsole.WriteLine();

        // Show edge IDs (in a real implementation, edges would be resolved to show actual target nodes)
        foreach (var edgeId in viewNode.OutgoingEdgeIds.Take(10)) // Show first 10
        {
            AnsiConsole.WriteLine($"  [gray]• Edge Reference: {edgeId}[/]");
        }

        if (viewNode.OutgoingEdgeIds.Count > 10)
        {
            AnsiConsole.WriteLine($"  [gray]... and {viewNode.OutgoingEdgeIds.Count - 10} more edge references[/]");
        }
    }

    private void DisplaySummary(List<KnowledgeNode> nodes)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[cyan]Summary[/]"));
        AnsiConsole.WriteLine();

        var summary = new Dictionary<string, int>();
        foreach (var node in nodes)
        {
            var key = $"{node.Type.Primary}";
            summary[key] = summary.GetValueOrDefault(key, 0) + 1;
        }

        var table = new Table();
        table.AddColumn("Node Type");
        table.AddColumn("Count");

        foreach (var kvp in summary.OrderByDescending(x => x.Value))
        {
            var color = GetNodeColor(kvp.Key);
            table.AddRow($"[{color}]{kvp.Key}[/]", $"[white]{kvp.Value}[/]");
        }

        AnsiConsole.Write(table);

        // Total edge references
        var totalEdgeReferences = nodes.Sum(n => n.IncomingEdgeIds.Count + n.OutgoingEdgeIds.Count);
        AnsiConsole.WriteLine($"\n[cyan]Total Nodes:[/] {nodes.Count}");
        AnsiConsole.WriteLine($"[cyan]Total Edge References:[/] {totalEdgeReferences}");
    }

    private string GetNodeColor(string nodeType)
    {
        return nodeType switch
        {
            "method" => "green",
            "basic-block" => "blue",
            "operation" => "gray",
            "view" => "purple",
            "class" => "yellow",
            "namespace" => "cyan",
            _ => "white"
        };
    }

    private string GetComplexityColor(int? complexity)
    {
        if (!complexity.HasValue) return "gray";
        
        return complexity.Value switch
        {
            <= 5 => "green",
            <= 10 => "yellow",
            _ => "red"
        };
    }

    private string GetBlockIcon(Dictionary<string, object?> properties)
    {
        if (properties.TryGetValue("kind", out var kindObj) && kindObj != null)
        {
            return kindObj.ToString() switch
            {
                "Entry" => "▶️",
                "Exit" => "⏹️",
                "Block" => "⬜",
                "ExceptionHandler" => "⚠️",
                _ => "📦"
            };
        }
        return "📦";
    }
}