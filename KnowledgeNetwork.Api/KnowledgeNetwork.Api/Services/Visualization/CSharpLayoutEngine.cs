using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeNetwork.Api.Models.Analysis;
using KnowledgeNetwork.Api.Models.Visualization;

namespace KnowledgeNetwork.Api.Services.Visualization
{
    /// <summary>
    /// Layout engine for C# code that creates hierarchical namespace/class/member layouts
    /// </summary>
    public class CSharpLayoutEngine : ILanguageLayoutEngine<CSharpAnalysisResult>
    {
        public string Language => "csharp";
        
        public GraphLayout GenerateLayout(CSharpAnalysisResult result)
        {
            var nodes = new List<GraphNode>();
            var edges = new List<GraphEdge>();
            
            // Create namespace node if present
            if (!string.IsNullOrEmpty(result.Namespace))
            {
                nodes.Add(CreateNamespaceNode(result.Namespace));
            }
            
            // Create type nodes (classes, interfaces, etc.)
            foreach (var type in result.Types)
            {
                var typeNode = CreateTypeNode(type, result.Namespace);
                nodes.Add(typeNode);
                
                // Add edge from namespace to type
                if (!string.IsNullOrEmpty(result.Namespace))
                {
                    edges.Add(CreateContainsEdge(
                        GetNamespaceId(result.Namespace),
                        typeNode.Id,
                        "contains"));
                }
                
                // Add inheritance edges
                foreach (var baseType in type.BaseTypes)
                {
                    edges.Add(CreateInheritanceEdge(typeNode.Id, GetTypeId(baseType)));
                }
                
                // Add interface implementation edges
                foreach (var iface in type.ImplementedInterfaces)
                {
                    edges.Add(CreateImplementsEdge(typeNode.Id, GetTypeId(iface)));
                }
            }
            
            // Create method nodes
            foreach (var method in result.Methods)
            {
                var methodNode = CreateMethodNode(method);
                nodes.Add(methodNode);
                
                // Add edge from containing type to method
                if (!string.IsNullOrEmpty(method.ContainingType))
                {
                    edges.Add(CreateContainsEdge(
                        GetTypeId(method.ContainingType),
                        methodNode.Id,
                        "contains"));
                }
            }
            
            // Create property nodes
            foreach (var property in result.Properties)
            {
                var propertyNode = CreatePropertyNode(property);
                nodes.Add(propertyNode);
                
                // Add edge from containing type to property
                if (!string.IsNullOrEmpty(property.ContainingType))
                {
                    edges.Add(CreateContainsEdge(
                        GetTypeId(property.ContainingType),
                        propertyNode.Id,
                        "contains"));
                }
            }
            
            // Create field nodes (only for important fields)
            foreach (var field in result.Fields.Where(f => f.AccessModifier != "private" || f.IsConst))
            {
                var fieldNode = CreateFieldNode(field);
                nodes.Add(fieldNode);
                
                // Add edge from containing type to field
                if (!string.IsNullOrEmpty(field.ContainingType))
                {
                    edges.Add(CreateContainsEdge(
                        GetTypeId(field.ContainingType),
                        fieldNode.Id,
                        "contains"));
                }
            }
            
            // Add relationship edges
            foreach (var relationship in result.Relationships)
            {
                edges.Add(CreateRelationshipEdge(relationship));
            }
            
            return new GraphLayout
            {
                Nodes = nodes,
                Edges = edges,
                Configuration = CreateCSharpLayoutConfiguration(),
                Metadata = CreateMetadata(result, nodes.Count, edges.Count)
            };
        }
        
        private GraphNode CreateNamespaceNode(string namespaceName)
        {
            return new GraphNode
            {
                Id = GetNamespaceId(namespaceName),
                Label = namespaceName,
                NodeType = "namespace",
                Language = "csharp",
                Visual = new NodeVisualProperties
                {
                    Color = "#2E7D32", // Green
                    Shape = "hexagon",
                    Size = "large",
                    Icon = "folder",
                    IsExpandable = true,
                    IsExpanded = true
                },
                Data = new Dictionary<string, object>
                {
                    ["fullName"] = namespaceName
                }
            };
        }
        
        private GraphNode CreateTypeNode(TypeInfo type, string? namespaceName)
        {
            var color = type.Kind switch
            {
                "class" => "#1976D2", // Blue
                "interface" => "#7B1FA2", // Purple
                "struct" => "#388E3C", // Dark Green
                "enum" => "#F57C00", // Orange
                _ => "#757575" // Grey
            };
            
            var icon = type.Kind switch
            {
                "class" => "class",
                "interface" => "interface",
                "struct" => "struct",
                "enum" => "enum",
                _ => "code"
            };
            
            return new GraphNode
            {
                Id = GetTypeId(type.QualifiedName),
                Label = type.Name,
                NodeType = type.Kind,
                Language = "csharp",
                ParentId = !string.IsNullOrEmpty(namespaceName) ? GetNamespaceId(namespaceName) : null,
                Visual = new NodeVisualProperties
                {
                    Color = color,
                    Shape = type.Kind == "interface" ? "diamond" : "rectangle",
                    Size = "medium",
                    Icon = icon,
                    IsExpandable = true,
                    IsExpanded = false
                },
                Data = new Dictionary<string, object>
                {
                    ["qualifiedName"] = type.QualifiedName,
                    ["accessModifier"] = type.AccessModifier,
                    ["isAbstract"] = type.IsAbstract,
                    ["isSealed"] = type.IsSealed,
                    ["isStatic"] = type.IsStatic,
                    ["isGeneric"] = type.IsGeneric,
                    ["location"] = type.Location
                }
            };
        }
        
        private GraphNode CreateMethodNode(MethodInfo method)
        {
            var color = method.AccessModifier switch
            {
                "public" => "#4CAF50", // Light Green
                "protected" => "#FF9800", // Orange
                "internal" => "#795548", // Brown
                _ => "#9E9E9E" // Grey
            };
            
            return new GraphNode
            {
                Id = GetMethodId(method.QualifiedName),
                Label = $"{method.Name}({method.Parameters.Count})",
                NodeType = "method",
                Language = "csharp",
                ParentId = !string.IsNullOrEmpty(method.ContainingType) ? 
                    GetTypeId(method.ContainingType) : null,
                Visual = new NodeVisualProperties
                {
                    Color = color,
                    Shape = "ellipse",
                    Size = "small",
                    Icon = method.IsAsync ? "async-function" : "function",
                    IsExpandable = false,
                    IsExpanded = false
                },
                Data = new Dictionary<string, object>
                {
                    ["qualifiedName"] = method.QualifiedName,
                    ["returnType"] = method.ReturnType,
                    ["parameters"] = method.Parameters,
                    ["accessModifier"] = method.AccessModifier,
                    ["isStatic"] = method.IsStatic,
                    ["isAsync"] = method.IsAsync,
                    ["location"] = method.Location
                }
            };
        }
        
        private GraphNode CreatePropertyNode(PropertyInfo property)
        {
            return new GraphNode
            {
                Id = GetPropertyId(property.QualifiedName),
                Label = property.Name,
                NodeType = "property",
                Language = "csharp",
                ParentId = !string.IsNullOrEmpty(property.ContainingType) ? 
                    GetTypeId(property.ContainingType) : null,
                Visual = new NodeVisualProperties
                {
                    Color = "#00BCD4", // Cyan
                    Shape = "roundrectangle",
                    Size = "small",
                    Icon = "property",
                    IsExpandable = false,
                    IsExpanded = false
                },
                Data = new Dictionary<string, object>
                {
                    ["qualifiedName"] = property.QualifiedName,
                    ["type"] = property.Type,
                    ["accessModifier"] = property.AccessModifier,
                    ["hasGetter"] = property.HasGetter,
                    ["hasSetter"] = property.HasSetter,
                    ["location"] = property.Location
                }
            };
        }
        
        private GraphNode CreateFieldNode(FieldInfo field)
        {
            return new GraphNode
            {
                Id = GetFieldId(field.QualifiedName),
                Label = field.Name,
                NodeType = "field",
                Language = "csharp",
                ParentId = !string.IsNullOrEmpty(field.ContainingType) ? 
                    GetTypeId(field.ContainingType) : null,
                Visual = new NodeVisualProperties
                {
                    Color = "#FFC107", // Amber
                    Shape = "circle",
                    Size = "tiny",
                    Icon = field.IsConst ? "constant" : "field",
                    IsExpandable = false,
                    IsExpanded = false
                },
                Data = new Dictionary<string, object>
                {
                    ["qualifiedName"] = field.QualifiedName,
                    ["type"] = field.Type,
                    ["accessModifier"] = field.AccessModifier,
                    ["isConst"] = field.IsConst,
                    ["isReadOnly"] = field.IsReadOnly,
                    ["location"] = field.Location
                }
            };
        }
        
        private GraphEdge CreateContainsEdge(string sourceId, string targetId, string label)
        {
            return new GraphEdge
            {
                Id = $"{sourceId}-contains-{targetId}",
                Source = sourceId,
                Target = targetId,
                EdgeType = "contains",
                Label = null,
                Visual = new EdgeVisualProperties
                {
                    Color = "#E0E0E0",
                    Style = "solid",
                    ArrowStyle = "none",
                    Width = 1.0f,
                    IsAnimated = false
                }
            };
        }
        
        private GraphEdge CreateInheritanceEdge(string sourceId, string targetId)
        {
            return new GraphEdge
            {
                Id = $"{sourceId}-inherits-{targetId}",
                Source = sourceId,
                Target = targetId,
                EdgeType = "inherits",
                Label = "inherits",
                Visual = new EdgeVisualProperties
                {
                    Color = "#2196F3",
                    Style = "solid",
                    ArrowStyle = "arrow",
                    Width = 2.0f,
                    IsAnimated = false
                }
            };
        }
        
        private GraphEdge CreateImplementsEdge(string sourceId, string targetId)
        {
            return new GraphEdge
            {
                Id = $"{sourceId}-implements-{targetId}",
                Source = sourceId,
                Target = targetId,
                EdgeType = "implements",
                Label = "implements",
                Visual = new EdgeVisualProperties
                {
                    Color = "#9C27B0",
                    Style = "dashed",
                    ArrowStyle = "arrow",
                    Width = 2.0f,
                    IsAnimated = false
                }
            };
        }
        
        private GraphEdge CreateRelationshipEdge(SymbolRelationship relationship)
        {
            var visual = relationship.RelationType switch
            {
                "calls" => new EdgeVisualProperties
                {
                    Color = "#FF5722",
                    Style = "dotted",
                    ArrowStyle = "arrow",
                    Width = 1.5f,
                    IsAnimated = true
                },
                "references" => new EdgeVisualProperties
                {
                    Color = "#795548",
                    Style = "dotted",
                    ArrowStyle = "arrow",
                    Width = 1.0f,
                    IsAnimated = false
                },
                _ => new EdgeVisualProperties
                {
                    Color = "#9E9E9E",
                    Style = "solid",
                    ArrowStyle = "arrow",
                    Width = 1.0f,
                    IsAnimated = false
                }
            };
            
            return new GraphEdge
            {
                Id = $"{relationship.SourceId}-{relationship.RelationType}-{relationship.TargetId}",
                Source = relationship.SourceId,
                Target = relationship.TargetId,
                EdgeType = relationship.RelationType,
                Label = relationship.RelationType,
                Visual = visual
            };
        }
        
        private LayoutConfiguration CreateCSharpLayoutConfiguration()
        {
            return new LayoutConfiguration
            {
                LayoutType = "hierarchical",
                Direction = "TB", // Top to Bottom
                NodeSpacing = 50.0f,
                RankSpacing = 100.0f,
                EnableClustering = true
            };
        }
        
        private GraphMetadata CreateMetadata(CSharpAnalysisResult result, int nodeCount, int edgeCount)
        {
            return new GraphMetadata
            {
                NodeCount = nodeCount,
                EdgeCount = edgeCount,
                Languages = new[] { "csharp" },
                GeneratedAt = DateTime.UtcNow,
                SourceFiles = new[] { result.SourceFile.FullName }
            };
        }
        
        // ID generation helpers
        private string GetNamespaceId(string namespaceName) => $"namespace:{namespaceName}";
        private string GetTypeId(string typeName) => $"type:{typeName}";
        private string GetMethodId(string methodName) => $"method:{methodName}";
        private string GetPropertyId(string propertyName) => $"property:{propertyName}";
        private string GetFieldId(string fieldName) => $"field:{fieldName}";
    }
}