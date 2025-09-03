using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;

/// <summary>
/// Resolves effective file names using multiple fallback strategies
/// </summary>
public class FileNameResolver(ILogger<FileNameResolver> logger) : IFileNameResolver
{
    private readonly ILogger<FileNameResolver> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Resolves the effective file name using multiple fallback strategies
    /// </summary>
    public string ResolveEffectiveFileName(CompilationUnitSyntax compilationUnit, string providedFileName)
    {
        // 1. Use provided filename if not empty
        if (!string.IsNullOrWhiteSpace(providedFileName))
        {
            _logger.LogDebug("Using provided filename: {FileName}", providedFileName);
            return providedFileName;
        }

        // 2. Try to get filename from syntax tree
        var syntaxTreePath = compilationUnit.SyntaxTree?.FilePath;
        if (!string.IsNullOrWhiteSpace(syntaxTreePath))
        {
            _logger.LogDebug("Using syntax tree filename: {FileName}", syntaxTreePath);
            return syntaxTreePath;
        }

        // 3. Generate identifier based on content hash for in-memory code
        var contentHash = GenerateContentBasedIdentifier(compilationUnit);
        var syntheticName = $"<in-memory-{contentHash}>";
        _logger.LogDebug("Generated synthetic filename: {FileName}", syntheticName);
        return syntheticName;
    }

    /// <summary>
    /// Generates a content-based identifier for in-memory compilation units
    /// </summary>
    private string GenerateContentBasedIdentifier(CompilationUnitSyntax compilationUnit)
    {
        // Use a simple hash of the first type name + member count for identification
        var firstType = compilationUnit.DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .FirstOrDefault();

        if (firstType != null)
        {
            var typeName = firstType switch
            {
                ClassDeclarationSyntax cls => cls.Identifier.ValueText,
                InterfaceDeclarationSyntax intf => intf.Identifier.ValueText,
                StructDeclarationSyntax str => str.Identifier.ValueText,
                RecordDeclarationSyntax rec => rec.Identifier.ValueText,
                EnumDeclarationSyntax enm => enm.Identifier.ValueText,
                _ => "UnknownType"
            };

            var memberCount = compilationUnit.DescendantNodes().OfType<MemberDeclarationSyntax>().Count();
            var hashCode = (typeName + memberCount).GetHashCode();
            return $"{typeName}-{Math.Abs(hashCode):X6}";
        }

        // Fallback to simple hash of the full text
        var textHash = compilationUnit.GetText().ToString().GetHashCode();
        return $"code-{Math.Abs(textHash):X6}";
    }
}