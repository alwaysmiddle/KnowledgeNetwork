using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KnowledgeNetwork.Api.Models.Analysis
{
    /// <summary>
    /// C#-specific analysis result that maintains rich Roslyn data while exposing lightweight DTOs
    /// </summary>
    public class CSharpAnalysisResult : ILanguageAnalysisResult
    {
        private readonly Lazy<SemanticModel> _semanticModel;
        private readonly WeakReference<CSharpCompilation> _compilation;
        private bool _disposed;
        
        public string Language => "csharp";
        public FileInfo SourceFile { get; init; }
        public DateTime AnalyzedAt { get; init; }
        public DirectoryInfo ProjectRoot { get; init; }
        
        // Extracted lightweight data for common operations
        public required IReadOnlyList<TypeInfo> Types { get; init; }
        public required IReadOnlyList<MethodInfo> Methods { get; init; }
        public required IReadOnlyList<PropertyInfo> Properties { get; init; }
        public required IReadOnlyList<FieldInfo> Fields { get; init; }
        public required IReadOnlyList<string> Usings { get; init; }
        public required string? Namespace { get; init; }
        
        // Relationships between symbols
        public required IReadOnlyList<SymbolRelationship> Relationships { get; init; }
        
        // Syntax tree for advanced operations
        public SyntaxTree SyntaxTree { get; init; }
        
        public string RelativePath => Path.GetRelativePath(ProjectRoot.FullName, SourceFile.FullName);
        
        public bool IsTestFile => 
            SourceFile.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
            RelativePath.Contains("Test", StringComparison.OrdinalIgnoreCase);
            
        public bool IsGeneratedCode =>
            RelativePath.Contains("obj/", StringComparison.OrdinalIgnoreCase) ||
            RelativePath.Contains("obj\\", StringComparison.OrdinalIgnoreCase) ||
            SourceFile.Name.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            SourceFile.Name.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase);
        
        public CSharpAnalysisResult(
            FileInfo sourceFile,
            DirectoryInfo projectRoot,
            SyntaxTree syntaxTree,
            CSharpCompilation compilation,
            Func<SemanticModel> semanticModelFactory)
        {
            SourceFile = sourceFile;
            ProjectRoot = projectRoot;
            SyntaxTree = syntaxTree;
            AnalyzedAt = DateTime.UtcNow;
            
            _compilation = new WeakReference<CSharpCompilation>(compilation);
            _semanticModel = new Lazy<SemanticModel>(semanticModelFactory);
            
            // Initialize required collections (will be populated by analyzer)
            Types = Array.Empty<TypeInfo>();
            Methods = Array.Empty<MethodInfo>();
            Properties = Array.Empty<PropertyInfo>();
            Fields = Array.Empty<FieldInfo>();
            Usings = Array.Empty<string>();
            Relationships = Array.Empty<SymbolRelationship>();
        }
        
        /// <summary>
        /// Gets the semantic model for advanced operations. Use sparingly as it keeps compilation in memory.
        /// </summary>
        public SemanticModel GetSemanticModel()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CSharpAnalysisResult));
                
            return _semanticModel.Value;
        }
        
        /// <summary>
        /// Gets the compilation if still available
        /// </summary>
        public CSharpCompilation? TryGetCompilation()
        {
            return _compilation.TryGetTarget(out var compilation) ? compilation : null;
        }
        
        // Note: Advanced reference finding requires Microsoft.CodeAnalysis.Workspaces package
        // For now, we keep the semantic model available for future enhancements
        
        public void Dispose()
        {
            if (_disposed) return;
            
            // Clear the compilation reference if it's still alive
            if (_compilation.TryGetTarget(out var compilation))
            {
                // Compilation doesn't implement IDisposable, but we clear the reference
                _compilation.SetTarget(null!);
            }
            
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}