using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;

/// <summary>
/// Calculates complexity metrics for classes and methods
/// </summary>
public class ComplexityCalculator(
    ILogger<ComplexityCalculator> logger,
    ISyntaxUtilities syntaxUtilities) : IComplexityCalculator
{
    private readonly ILogger<ComplexityCalculator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Calculates complexity metrics for a class
    /// </summary>
    public async Task<ClassComplexityMetrics> CalculateClassComplexityAsync(
        BaseTypeDeclarationSyntax typeDeclaration, 
        SemanticModel semanticModel)
    {
        var metrics = new ClassComplexityMetrics();

        try
        {
            // Calculate line count
            var span = typeDeclaration.Span;
            var text = typeDeclaration.SyntaxTree.GetText();
            var startLine = text.Lines.GetLineFromPosition(span.Start).LineNumber;
            var endLine = text.Lines.GetLineFromPosition(span.End).LineNumber;
            metrics.TotalLineCount = endLine - startLine + 1;

            // Get members for analysis
            var methods = _syntaxUtilities.GetMethods(typeDeclaration);
            var allMembers = _syntaxUtilities.GetAllMembers(typeDeclaration);

            // Count public members
            metrics.PublicMemberCount = allMembers
                .Count(m => m.Modifiers.Any(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword));

            // Calculate WMC (sum of method complexities) - simplified version
            foreach (var method in methods)
            {
                // Simple complexity based on control flow statements
                var complexity = CalculateMethodComplexity(method);
                metrics.WeightedMethodsPerClass += complexity;
            }

            // Calculate RFC (response for class) - methods + methods called
            metrics.ResponseForClass = methods.Count();
            foreach (var method in methods)
            {
                var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>().Count();
                metrics.ResponseForClass += invocations;
            }

            _logger.LogDebug("Calculated complexity metrics for type: WMC={WMC}, RFC={RFC}, Lines={Lines}, PublicMembers={PublicMembers}",
                metrics.WeightedMethodsPerClass, metrics.ResponseForClass, metrics.TotalLineCount, metrics.PublicMemberCount);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate complexity metrics for type declaration");
            return metrics; // Return empty metrics on error
        }
    }

    /// <summary>
    /// Simple method complexity calculation based on control flow statements
    /// </summary>
    private int CalculateMethodComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1; // Base complexity

        try
        {
            // Add complexity for control flow statements
            complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
            complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
            complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
            complexity += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
            complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
            complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();

            return complexity;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate complexity for method: {MethodName}", method.Identifier.ValueText);
            return 1; // Return base complexity on error
        }
    }
}