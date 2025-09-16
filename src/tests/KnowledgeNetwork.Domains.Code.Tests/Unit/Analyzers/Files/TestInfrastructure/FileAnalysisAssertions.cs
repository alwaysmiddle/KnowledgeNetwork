using KnowledgeNetwork.Domains.Code.Models.Files;
using Shouldly;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.TestInfrastructure;

/// <summary>
/// Custom Shouldly assertions for FileDependencyGraph validation
/// </summary>
public static class FileAnalysisAssertions
{
    /// <summary>
    /// Assert that the graph contains the expected number of files
    /// </summary>
    public static void ShouldHaveFileCount(this FileDependencyGraph graph, int expectedCount)
    {
        graph.ShouldNotBeNull();
        graph.Files.Count.ShouldBe(expectedCount, $"Expected {expectedCount} files but found {graph.Files.Count}");
    }

    /// <summary>
    /// Assert that the graph contains a file with the specified name
    /// </summary>
    public static void ShouldContainFile(this FileDependencyGraph graph, string fileName)
    {
        graph.ShouldNotBeNull();
        graph.Files.ShouldContain(f => f.FileName == fileName, $"Expected to find file '{fileName}' in the graph");
    }

    /// <summary>
    /// Assert that the graph contains files with all specified names
    /// </summary>
    public static void ShouldContainFiles(this FileDependencyGraph graph, params string[] fileNames)
    {
        graph.ShouldNotBeNull();
        foreach (var fileName in fileNames)
        {
            graph.ShouldContainFile(fileName);
        }
    }

    /// <summary>
    /// Assert that the graph contains the expected number of using dependencies
    /// </summary>
    public static void ShouldHaveUsingDependencyCount(this FileDependencyGraph graph, int expectedCount)
    {
        graph.ShouldNotBeNull();
        graph.UsingDependencies.Count.ShouldBe(expectedCount,
            $"Expected {expectedCount} using dependencies but found {graph.UsingDependencies.Count}");
    }

    /// <summary>
    /// Assert that a using dependency exists between two files for a specific namespace
    /// </summary>
    public static void ShouldHaveUsingDependency(this FileDependencyGraph graph, string sourceFileName, string namespaceName)
    {
        graph.ShouldNotBeNull();
        var sourceFile = graph.Files.FirstOrDefault(f => f.FileName == sourceFileName);
        sourceFile.ShouldNotBeNull($"Source file '{sourceFileName}' not found in graph");

        var dependency = graph.UsingDependencies.FirstOrDefault(d =>
            d.SourceFileId == sourceFile.Id && d.NamespaceName == namespaceName);

        dependency.ShouldNotBeNull($"Expected using dependency from '{sourceFileName}' to namespace '{namespaceName}'");
    }

    /// <summary>
    /// Assert that a using dependency has specific characteristics
    /// </summary>
    public static void ShouldHaveUsingDependency(this FileDependencyGraph graph, string sourceFileName, string namespaceName,
        bool isGlobal = false, bool isStatic = false, bool isAlias = false, bool isUtilized = true)
    {
        graph.ShouldHaveUsingDependency(sourceFileName, namespaceName);

        var sourceFile = graph.Files.First(f => f.FileName == sourceFileName);
        var dependency = graph.UsingDependencies.First(d =>
            d.SourceFileId == sourceFile.Id && d.NamespaceName == namespaceName);

        dependency.IsGlobal.ShouldBe(isGlobal, $"Expected IsGlobal to be {isGlobal}");
        dependency.IsStatic.ShouldBe(isStatic, $"Expected IsStatic to be {isStatic}");
        dependency.IsAlias.ShouldBe(isAlias, $"Expected IsAlias to be {isAlias}");
        dependency.IsUtilized.ShouldBe(isUtilized, $"Expected IsUtilized to be {isUtilized}");
    }

    /// <summary>
    /// Assert that the graph contains the expected number of namespace dependencies
    /// </summary>
    public static void ShouldHaveNamespaceDependencyCount(this FileDependencyGraph graph, int expectedCount)
    {
        graph.ShouldNotBeNull();
        graph.NamespaceDependencies.Count.ShouldBe(expectedCount,
            $"Expected {expectedCount} namespace dependencies but found {graph.NamespaceDependencies.Count}");
    }

    /// <summary>
    /// Assert that a namespace dependency exists between two files
    /// </summary>
    public static void ShouldHaveNamespaceDependency(this FileDependencyGraph graph, string sourceFileName, string targetFileName, string namespaceName)
    {
        graph.ShouldNotBeNull();
        var sourceFile = graph.Files.FirstOrDefault(f => f.FileName == sourceFileName);
        var targetFile = graph.Files.FirstOrDefault(f => f.FileName == targetFileName);

        sourceFile.ShouldNotBeNull($"Source file '{sourceFileName}' not found in graph");
        targetFile.ShouldNotBeNull($"Target file '{targetFileName}' not found in graph");

        var dependency = graph.NamespaceDependencies.FirstOrDefault(d =>
            d.SourceFileId == sourceFile.Id && d.TargetFileId == targetFile.Id && d.NamespaceName == namespaceName);

        dependency.ShouldNotBeNull($"Expected namespace dependency from '{sourceFileName}' to '{targetFileName}' for namespace '{namespaceName}'");
    }

    /// <summary>
    /// Assert that the graph contains the expected number of type reference dependencies
    /// </summary>
    public static void ShouldHaveTypeReferenceDependencyCount(this FileDependencyGraph graph, int expectedCount)
    {
        graph.ShouldNotBeNull();
        graph.TypeReferenceDependencies.Count.ShouldBe(expectedCount,
            $"Expected {expectedCount} type reference dependencies but found {graph.TypeReferenceDependencies.Count}");
    }

    /// <summary>
    /// Assert that a type reference dependency exists
    /// </summary>
    public static void ShouldHaveTypeReferenceDependency(this FileDependencyGraph graph, string sourceFileName, string targetFileName, string typeName)
    {
        graph.ShouldNotBeNull();
        var sourceFile = graph.Files.FirstOrDefault(f => f.FileName == sourceFileName);
        var targetFile = graph.Files.FirstOrDefault(f => f.FileName == targetFileName);

        sourceFile.ShouldNotBeNull($"Source file '{sourceFileName}' not found in graph");
        targetFile.ShouldNotBeNull($"Target file '{targetFileName}' not found in graph");

        var dependency = graph.TypeReferenceDependencies.FirstOrDefault(d =>
            d.SourceFileId == sourceFile.Id && d.TargetFileId == targetFile.Id && d.TypeName == typeName);

        dependency.ShouldNotBeNull($"Expected type reference dependency from '{sourceFileName}' to '{targetFileName}' for type '{typeName}'");
    }

    /// <summary>
    /// Assert that the graph contains the expected number of assembly dependencies
    /// </summary>
    public static void ShouldHaveAssemblyDependencyCount(this FileDependencyGraph graph, int expectedCount)
    {
        graph.ShouldNotBeNull();
        graph.AssemblyDependencies.Count.ShouldBe(expectedCount,
            $"Expected {expectedCount} assembly dependencies but found {graph.AssemblyDependencies.Count}");
    }

    /// <summary>
    /// Assert that an assembly dependency exists
    /// </summary>
    public static void ShouldHaveAssemblyDependency(this FileDependencyGraph graph, string sourceFileName, string assemblyName)
    {
        graph.ShouldNotBeNull();
        var sourceFile = graph.Files.FirstOrDefault(f => f.FileName == sourceFileName);
        sourceFile.ShouldNotBeNull($"Source file '{sourceFileName}' not found in graph");

        var dependency = graph.AssemblyDependencies.FirstOrDefault(d =>
            d.SourceFileId == sourceFile.Id && d.AssemblyName == assemblyName);

        dependency.ShouldNotBeNull($"Expected assembly dependency from '{sourceFileName}' to assembly '{assemblyName}'");
    }

    /// <summary>
    /// Assert that a file declares the expected namespaces
    /// </summary>
    public static void ShouldDeclareNamespaces(this FileDependencyGraph graph, string fileName, params string[] expectedNamespaces)
    {
        graph.ShouldNotBeNull();
        var file = graph.Files.FirstOrDefault(f => f.FileName == fileName);
        file.ShouldNotBeNull($"File '{fileName}' not found in graph");

        foreach (var expectedNamespace in expectedNamespaces)
        {
            file.DeclaredNamespaces.ShouldContain(expectedNamespace,
                $"Expected file '{fileName}' to declare namespace '{expectedNamespace}'");
        }
    }

    /// <summary>
    /// Assert that a file declares the expected types
    /// </summary>
    public static void ShouldDeclareTypes(this FileDependencyGraph graph, string fileName, params string[] expectedTypeNames)
    {
        graph.ShouldNotBeNull();
        var file = graph.Files.FirstOrDefault(f => f.FileName == fileName);
        file.ShouldNotBeNull($"File '{fileName}' not found in graph");

        foreach (var expectedTypeName in expectedTypeNames)
        {
            file.DeclaredTypes.ShouldContain(t => t.Name == expectedTypeName,
                $"Expected file '{fileName}' to declare type '{expectedTypeName}'");
        }
    }

    /// <summary>
    /// Assert that a file references the expected types
    /// </summary>
    public static void ShouldReferenceTypes(this FileDependencyGraph graph, string fileName, params string[] expectedTypeNames)
    {
        graph.ShouldNotBeNull();
        var file = graph.Files.FirstOrDefault(f => f.FileName == fileName);
        file.ShouldNotBeNull($"File '{fileName}' not found in graph");

        foreach (var expectedTypeName in expectedTypeNames)
        {
            file.ReferencedTypes.ShouldContain(t => t.Name == expectedTypeName,
                $"Expected file '{fileName}' to reference type '{expectedTypeName}'");
        }
    }

    /// <summary>
    /// Assert that the graph represents a valid project structure
    /// </summary>
    public static void ShouldBeValidProjectStructure(this FileDependencyGraph graph, string expectedProjectName)
    {
        graph.ShouldNotBeNull();
        graph.ScopeName.ShouldBe(expectedProjectName);
        graph.ScopeType.ShouldBe(FileDependencyScope.Project);
        graph.Files.ShouldNotBeEmpty();

        // All files should belong to the same project
        graph.Files.ShouldAllBe(f => f.ProjectName == expectedProjectName);

        // All files should have unique IDs
        var fileIds = graph.Files.Select(f => f.Id).ToList();
        fileIds.Count.ShouldBe(fileIds.Distinct().Count(), "All files should have unique IDs");
    }

    /// <summary>
    /// Assert that circular dependencies exist in the graph
    /// </summary>
    public static void ShouldHaveCircularDependencies(this FileDependencyGraph graph)
    {
        graph.ShouldNotBeNull();

        // Check for circular using dependencies
        var hasCircularUsing = HasCircularDependencies(graph.UsingDependencies, d => d.SourceFileId, d => d.TargetFileId);

        // Check for circular namespace dependencies
        var hasCircularNamespace = HasCircularDependencies(graph.NamespaceDependencies, d => d.SourceFileId, d => d.TargetFileId);

        // Check for circular type reference dependencies
        var hasCircularTypeRef = HasCircularDependencies(graph.TypeReferenceDependencies, d => d.SourceFileId, d => d.TargetFileId);

        (hasCircularUsing || hasCircularNamespace || hasCircularTypeRef)
            .ShouldBeTrue("Expected to find circular dependencies in the graph");
    }

    private static bool HasCircularDependencies<T>(IEnumerable<T> dependencies,
        Func<T, string> getSource, Func<T, string> getTarget)
    {
        var graph = new Dictionary<string, HashSet<string>>();

        foreach (var dep in dependencies)
        {
            var source = getSource(dep);
            var target = getTarget(dep);

            if (string.IsNullOrEmpty(target)) continue;

            if (!graph.ContainsKey(source))
                graph[source] = new HashSet<string>();

            graph[source].Add(target);
        }

        return HasCycle(graph);
    }

    private static bool HasCycle(Dictionary<string, HashSet<string>> graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (HasCycleDfs(node, graph, visited, recursionStack))
                return true;
        }

        return false;
    }

    private static bool HasCycleDfs(string node, Dictionary<string, HashSet<string>> graph,
        HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (recursionStack.Contains(node))
            return true;

        if (visited.Contains(node))
            return false;

        visited.Add(node);
        recursionStack.Add(node);

        if (graph.ContainsKey(node))
        {
            foreach (var neighbor in graph[node])
            {
                if (HasCycleDfs(neighbor, graph, visited, recursionStack))
                    return true;
            }
        }

        recursionStack.Remove(node);
        return false;
    }
}