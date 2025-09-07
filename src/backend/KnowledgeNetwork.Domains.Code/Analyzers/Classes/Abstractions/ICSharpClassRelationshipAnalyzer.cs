using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

public interface ICSharpClassRelationshipAnalyzer
{
    /// <summary>
    /// Analyzes class relationships within a compilation unit (file)
    /// </summary>
    Task<ClassRelationshipGraph?> AnalyzeFileAsync(
        Compilation compilation,
        CompilationUnitSyntax compilationUnit,
        string fileName = "");
}