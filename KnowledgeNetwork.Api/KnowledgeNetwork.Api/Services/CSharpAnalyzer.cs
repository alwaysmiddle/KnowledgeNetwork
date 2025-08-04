using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KnowledgeNetwork.Api.Models.Analysis;

namespace KnowledgeNetwork.Api.Services
{
    /// <summary>
    /// C#-specific analyzer using Roslyn compiler with hybrid approach
    /// </summary>
    public class CSharpAnalyzer : ILanguageAnalyzer<CSharpAnalysisResult>
    {
        public string Language => "csharp";
        public string[] SupportedExtensions => [".cs"];
        
        // Roslyn references that are commonly needed
        private static readonly MetadataReference[] DefaultReferences = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location)
        };
        
        public bool CanAnalyze(FileInfo file)
        {
            return file.Exists && 
                   SupportedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase);
        }
        
        public async Task<CSharpAnalysisResult> AnalyzeAsync(
            FileInfo sourceFile, 
            DirectoryInfo projectRoot,
            CancellationToken cancellationToken = default)
        {
            if (!CanAnalyze(sourceFile))
            {
                throw new ArgumentException($"Cannot analyze file: {sourceFile.FullName}");
            }
            
            var code = await File.ReadAllTextAsync(sourceFile.FullName, cancellationToken);
            
            // Parse the source code
            var syntaxTree = CSharpSyntaxTree.ParseText(
                code, 
                path: sourceFile.FullName,
                cancellationToken: cancellationToken);
                
            var root = await syntaxTree.GetRootAsync(cancellationToken);
            
            // Create compilation for semantic analysis
            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(sourceFile.Name),
                syntaxTrees: [syntaxTree],
                references: DefaultReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            
            // Extract lightweight data eagerly
            var types = await ExtractTypesAsync(root, compilation, syntaxTree, cancellationToken);
            var methods = await ExtractMethodsAsync(root, compilation, syntaxTree, cancellationToken);
            var properties = await ExtractPropertiesAsync(root, compilation, syntaxTree, cancellationToken);
            var fields = await ExtractFieldsAsync(root, compilation, syntaxTree, cancellationToken);
            var usings = ExtractUsings(root);
            var namespaceName = ExtractNamespace(root);
            var relationships = await ExtractRelationshipsAsync(root, compilation, syntaxTree, cancellationToken);
            
            // Create result with lazy semantic model
            var result = new CSharpAnalysisResult(
                sourceFile,
                projectRoot,
                syntaxTree,
                compilation,
                () => compilation.GetSemanticModel(syntaxTree))
            {
                Types = types,
                Methods = methods,
                Properties = properties,
                Fields = fields,
                Usings = usings,
                Namespace = namespaceName,
                Relationships = relationships
            };
            
            return result;
        }
        
        private async Task<IReadOnlyList<Models.Analysis.TypeInfo>> ExtractTypesAsync(
            SyntaxNode root, 
            CSharpCompilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken)
        {
            var types = new List<Models.Analysis.TypeInfo>();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            var typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .ToList();
            
            foreach (var typeDecl in typeDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(typeDecl);
                if (symbol == null) continue;
                
                var location = typeDecl.GetLocation();
                var lineSpan = location.GetLineSpan();
                
                types.Add(new Models.Analysis.TypeInfo
                {
                    QualifiedName = symbol.ToDisplayString(),
                    Name = symbol.Name,
                    Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? "",
                    Kind = GetTypeKind(typeDecl),
                    Location = CreateSourceLocation(location),
                    BaseTypes = GetBaseTypes(symbol),
                    ImplementedInterfaces = GetImplementedInterfaces(symbol),
                    AccessModifier = GetAccessibility(symbol.DeclaredAccessibility),
                    IsAbstract = symbol.IsAbstract,
                    IsSealed = symbol.IsSealed,
                    IsStatic = symbol.IsStatic,
                    IsPartial = typeDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)),
                    IsGeneric = symbol.IsGenericType,
                    GenericParameterCount = symbol.TypeParameters.Length
                });
            }
            
            return types;
        }
        
        private async Task<IReadOnlyList<MethodInfo>> ExtractMethodsAsync(
            SyntaxNode root,
            CSharpCompilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken)
        {
            var methods = new List<MethodInfo>();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            var methodDeclarations = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .ToList();
            
            foreach (var methodDecl in methodDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(methodDecl);
                if (symbol == null) continue;
                
                var parameters = methodDecl.ParameterList.Parameters
                    .Select((p, i) => CreateParameterInfo(p, symbol.Parameters[i], i))
                    .ToList();
                
                methods.Add(new MethodInfo
                {
                    QualifiedName = symbol.ToDisplayString(),
                    Name = symbol.Name,
                    ContainingType = symbol.ContainingType?.ToDisplayString() ?? "",
                    ReturnType = symbol.ReturnType.ToDisplayString(),
                    Location = CreateSourceLocation(methodDecl.GetLocation()),
                    Parameters = parameters,
                    AccessModifier = GetAccessibility(symbol.DeclaredAccessibility),
                    IsStatic = symbol.IsStatic,
                    IsAbstract = symbol.IsAbstract,
                    IsVirtual = symbol.IsVirtual,
                    IsOverride = symbol.IsOverride,
                    IsAsync = symbol.IsAsync,
                    IsGeneric = symbol.IsGenericMethod,
                    GenericParameterCount = symbol.TypeParameters.Length
                });
            }
            
            return methods;
        }
        
        private async Task<IReadOnlyList<PropertyInfo>> ExtractPropertiesAsync(
            SyntaxNode root,
            CSharpCompilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken)
        {
            var properties = new List<PropertyInfo>();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            var propertyDeclarations = root.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .ToList();
            
            foreach (var propDecl in propertyDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(propDecl);
                if (symbol == null) continue;
                
                properties.Add(new PropertyInfo
                {
                    QualifiedName = symbol.ToDisplayString(),
                    Name = symbol.Name,
                    ContainingType = symbol.ContainingType?.ToDisplayString() ?? "",
                    Type = symbol.Type.ToDisplayString(),
                    Location = CreateSourceLocation(propDecl.GetLocation()),
                    AccessModifier = GetAccessibility(symbol.DeclaredAccessibility),
                    IsStatic = symbol.IsStatic,
                    IsAbstract = symbol.IsAbstract,
                    IsVirtual = symbol.IsVirtual,
                    IsOverride = symbol.IsOverride,
                    HasGetter = symbol.GetMethod != null,
                    HasSetter = symbol.SetMethod != null,
                    IsReadOnly = symbol.IsReadOnly
                });
            }
            
            return properties;
        }
        
        private async Task<IReadOnlyList<FieldInfo>> ExtractFieldsAsync(
            SyntaxNode root,
            CSharpCompilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken)
        {
            var fields = new List<FieldInfo>();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            var fieldDeclarations = root.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .ToList();
            
            foreach (var fieldDecl in fieldDeclarations)
            {
                foreach (var variable in fieldDecl.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                    if (symbol == null) continue;
                    
                    fields.Add(new FieldInfo
                    {
                        QualifiedName = symbol.ToDisplayString(),
                        Name = symbol.Name,
                        ContainingType = symbol.ContainingType?.ToDisplayString() ?? "",
                        Type = symbol.Type.ToDisplayString(),
                        Location = CreateSourceLocation(variable.GetLocation()),
                        AccessModifier = GetAccessibility(symbol.DeclaredAccessibility),
                        IsStatic = symbol.IsStatic,
                        IsReadOnly = symbol.IsReadOnly,
                        IsConst = symbol.IsConst,
                        IsVolatile = symbol.IsVolatile
                    });
                }
            }
            
            return fields;
        }
        
        private async Task<IReadOnlyList<SymbolRelationship>> ExtractRelationshipsAsync(
            SyntaxNode root,
            CSharpCompilation compilation,
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken)
        {
            var relationships = new List<SymbolRelationship>();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            
            // Extract inheritance relationships
            var typeDeclarations = root.DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .ToList();
                
            foreach (var typeDecl in typeDeclarations)
            {
                var symbol = semanticModel.GetDeclaredSymbol(typeDecl) as INamedTypeSymbol;
                if (symbol == null) continue;
                
                var sourceId = symbol.ToDisplayString();
                
                // Base type relationships
                if (symbol.BaseType != null && symbol.BaseType.SpecialType != SpecialType.System_Object)
                {
                    relationships.Add(new SymbolRelationship
                    {
                        SourceId = sourceId,
                        TargetId = symbol.BaseType.ToDisplayString(),
                        RelationType = "inherits",
                        Location = typeDecl.BaseList != null ? 
                            CreateSourceLocation(typeDecl.BaseList.GetLocation()) : null
                    });
                }
                
                // Interface implementations
                foreach (var iface in symbol.Interfaces)
                {
                    relationships.Add(new SymbolRelationship
                    {
                        SourceId = sourceId,
                        TargetId = iface.ToDisplayString(),
                        RelationType = "implements",
                        Location = typeDecl.BaseList != null ? 
                            CreateSourceLocation(typeDecl.BaseList.GetLocation()) : null
                    });
                }
            }
            
            // TODO: Extract method calls, field references, etc.
            
            return relationships;
        }
        
        private IReadOnlyList<string> ExtractUsings(SyntaxNode root)
        {
            return root.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(u => u.Name?.ToString() ?? "")
                .Where(u => !string.IsNullOrEmpty(u))
                .ToList();
        }
        
        private string? ExtractNamespace(SyntaxNode root)
        {
            var namespaceDecl = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();
                
            return namespaceDecl?.Name.ToString();
        }
        
        private SourceLocation CreateSourceLocation(Location location)
        {
            var lineSpan = location.GetLineSpan();
            var span = location.SourceSpan;
            
            return new SourceLocation
            {
                FilePath = location.SourceTree?.FilePath ?? "",
                StartLine = lineSpan.StartLinePosition.Line + 1,
                StartColumn = lineSpan.StartLinePosition.Character + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndColumn = lineSpan.EndLinePosition.Character + 1,
                StartPosition = span.Start,
                EndPosition = span.End
            };
        }
        
        private ParameterInfo CreateParameterInfo(ParameterSyntax syntax, IParameterSymbol symbol, int position)
        {
            return new ParameterInfo
            {
                Name = symbol.Name,
                Type = symbol.Type.ToDisplayString(),
                HasDefaultValue = symbol.HasExplicitDefaultValue,
                DefaultValue = symbol.HasExplicitDefaultValue ? 
                    symbol.ExplicitDefaultValue?.ToString() : null,
                Modifier = GetParameterModifier(symbol),
                Position = position
            };
        }
        
        private string GetParameterModifier(IParameterSymbol symbol)
        {
            if (symbol.RefKind == RefKind.Ref) return "ref";
            if (symbol.RefKind == RefKind.Out) return "out";
            if (symbol.RefKind == RefKind.In) return "in";
            if (symbol.IsParams) return "params";
            return "";
        }
        
        private string GetTypeKind(TypeDeclarationSyntax typeDecl)
        {
            return typeDecl switch
            {
                ClassDeclarationSyntax => "class",
                InterfaceDeclarationSyntax => "interface",
                StructDeclarationSyntax => "struct",
                RecordDeclarationSyntax => "record",
                _ => "type"
            };
        }
        
        private IReadOnlyList<string> GetBaseTypes(INamedTypeSymbol symbol)
        {
            var baseTypes = new List<string>();
            
            if (symbol.BaseType != null && symbol.BaseType.SpecialType != SpecialType.System_Object)
            {
                baseTypes.Add(symbol.BaseType.ToDisplayString());
            }
            
            return baseTypes;
        }
        
        private IReadOnlyList<string> GetImplementedInterfaces(INamedTypeSymbol symbol)
        {
            return symbol.Interfaces
                .Select(i => i.ToDisplayString())
                .ToList();
        }
        
        private string GetAccessibility(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.Public => "public",
                _ => "private"
            };
        }
    }
}