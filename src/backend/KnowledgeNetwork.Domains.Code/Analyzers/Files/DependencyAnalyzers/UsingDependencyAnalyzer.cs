using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.DependencyAnalyzers;

/// <summary>
/// Analyzes using directive dependencies between files
/// </summary>
public class UsingDependencyAnalyzer(ILogger<UsingDependencyAnalyzer> logger, IFileSyntaxUtilities syntaxUtilities) : IUsingDependencyAnalyzer
{
    /// <summary>
    /// Analyzes using directive dependencies between files in the graph
    /// </summary>
    public async Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph)
    {
        foreach (var sourceFile in graph.Files)
        {
            var syntaxTree = compilation.SyntaxTrees.FirstOrDefault(st => st.FilePath == sourceFile.FilePath);
            if (syntaxTree == null) continue;

            var root = await syntaxTree.GetRootAsync();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name?.ToString();
                if (string.IsNullOrEmpty(namespaceName)) continue;

                // Find target file that declares this namespace
                var targetFile = graph.Files.FirstOrDefault(f => 
                    f.DeclaredNamespaces.Contains(namespaceName) && f.Id != sourceFile.Id);

                var usingDependency = new UsingDependencyEdge
                {
                    SourceFileId = sourceFile.Id,
                    TargetFileId = targetFile?.Id ?? string.Empty,
                    NamespaceName = namespaceName,
                    UsingDirective = usingDirective.ToString(),
                    DirectiveType = DetermineUsingDirectiveType(usingDirective),
                    IsGlobal = usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword),
                    IsStatic = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword),
                    IsAlias = usingDirective.Alias != null,
                    AliasName = usingDirective.Alias?.Name.ToString(),
                    IsExternalAssembly = targetFile == null,
                    UsingLocation = syntaxUtilities.GetLocationInfo(usingDirective)
                };

                // Check if the using is actually utilized
                AnalyzeUsingUtilization(usingDependency, sourceFile, compilation);

                graph.UsingDependencies.Add(usingDependency);
            }
        }
    }

    /// <summary>
    /// Determines the type of using directive
    /// </summary>
    private UsingDirectiveType DetermineUsingDirectiveType(UsingDirectiveSyntax usingDirective)
    {
        var isGlobal = usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
        var isStatic = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
        var isAlias = usingDirective.Alias != null;

        return (isGlobal, isStatic, isAlias) switch
        {
            (true, true, false) => UsingDirectiveType.GlobalStatic,
            (true, false, true) => UsingDirectiveType.GlobalAlias,
            (true, false, false) => UsingDirectiveType.Global,
            (false, true, false) => UsingDirectiveType.Static,
            (false, false, true) => UsingDirectiveType.Alias,
            _ => UsingDirectiveType.Namespace
        };
    }

    /// <summary>
    /// Analyzes whether a using directive is actually utilized
    /// </summary>
    private void AnalyzeUsingUtilization(UsingDependencyEdge usingDependency, FileNode sourceFile, Compilation compilation)
    {
        var namespaceName = usingDependency.NamespaceName;
        
        // Check if any referenced types belong to this namespace
        var utilizedTypes = sourceFile.ReferencedTypes
            .Where(rt => rt.Namespace == namespaceName)
            .ToList();

        usingDependency.IsUtilized = utilizedTypes.Count > 0;
        usingDependency.UtilizationCount = utilizedTypes.Sum(ut => ut.ReferenceCount);
        usingDependency.UtilizedTypes = utilizedTypes.Select(ut => ut.Name).ToList();
    }
}