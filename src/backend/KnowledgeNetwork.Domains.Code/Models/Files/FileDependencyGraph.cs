using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents dependencies between files within a project or solution
/// </summary>
public class FileDependencyGraph
{
    /// <summary>
    /// Unique identifier for this graph
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name/identifier of the analyzed scope (project name, solution name, etc.)
    /// </summary>
    public string ScopeName { get; set; } = string.Empty;

    /// <summary>
    /// Type of scope this graph represents
    /// </summary>
    public FileDependencyScope ScopeType { get; set; } = FileDependencyScope.Project;

    /// <summary>
    /// All files found in the analyzed scope
    /// </summary>
    public List<FileNode> Files { get; set; } = new();

    /// <summary>
    /// Using directive dependencies between files
    /// </summary>
    public List<UsingDependencyEdge> UsingDependencies { get; set; } = new();

    /// <summary>
    /// Namespace dependencies between files
    /// </summary>
    public List<NamespaceDependencyEdge> NamespaceDependencies { get; set; } = new();

    /// <summary>
    /// External assembly dependencies
    /// </summary>
    public List<AssemblyDependencyEdge> AssemblyDependencies { get; set; } = new();

    /// <summary>
    /// Type reference dependencies between files
    /// </summary>
    public List<TypeReferenceDependencyEdge> TypeReferenceDependencies { get; set; } = new();

    /// <summary>
    /// Project reference dependencies (for multi-project solutions)
    /// </summary>
    public List<ProjectReferenceDependencyEdge> ProjectReferenceDependencies { get; set; } = new();

    /// <summary>
    /// Source location information for the analyzed scope
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Analysis timestamp
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Get all files that the specified file depends on
    /// </summary>
    public List<FileNode> GetDependenciesFor(string fileId)
    {
        var dependencyIds = new HashSet<string>();

        // From using dependencies
        var usingDeps = UsingDependencies
            .Where(u => u.SourceFileId == fileId)
            .Select(u => u.TargetFileId)
            .Where(id => !string.IsNullOrEmpty(id));

        // From namespace dependencies
        var namespaceDeps = NamespaceDependencies
            .Where(n => n.SourceFileId == fileId)
            .Select(n => n.TargetFileId)
            .Where(id => !string.IsNullOrEmpty(id));

        // From type reference dependencies
        var typeRefDeps = TypeReferenceDependencies
            .Where(t => t.SourceFileId == fileId)
            .Select(t => t.TargetFileId)
            .Where(id => !string.IsNullOrEmpty(id));

        foreach (var id in usingDeps.Concat(namespaceDeps).Concat(typeRefDeps))
        {
            dependencyIds.Add(id);
        }

        return Files.Where(f => dependencyIds.Contains(f.Id)).ToList();
    }

    /// <summary>
    /// Get all files that depend on the specified file
    /// </summary>
    public List<FileNode> GetDependentsFor(string fileId)
    {
        var dependentIds = new HashSet<string>();

        // From using dependencies
        var usingDeps = UsingDependencies
            .Where(u => u.TargetFileId == fileId)
            .Select(u => u.SourceFileId);

        // From namespace dependencies
        var namespaceDeps = NamespaceDependencies
            .Where(n => n.TargetFileId == fileId)
            .Select(n => n.SourceFileId);

        // From type reference dependencies
        var typeRefDeps = TypeReferenceDependencies
            .Where(t => t.TargetFileId == fileId)
            .Select(t => t.SourceFileId);

        foreach (var id in usingDeps.Concat(namespaceDeps).Concat(typeRefDeps))
        {
            dependentIds.Add(id);
        }

        return Files.Where(f => dependentIds.Contains(f.Id)).ToList();
    }

    /// <summary>
    /// Get all external assemblies referenced by the project
    /// </summary>
    public List<string> GetExternalAssemblies()
    {
        return AssemblyDependencies
            .Select(a => a.AssemblyName)
            .Distinct()
            .OrderBy(name => name)
            .ToList();
    }

    /// <summary>
    /// Get all namespaces used across all files
    /// </summary>
    public List<string> GetAllNamespaces()
    {
        var namespaces = new HashSet<string>();

        // From file namespaces
        foreach (var file in Files)
        {
            foreach (var ns in file.DeclaredNamespaces)
            {
                namespaces.Add(ns);
            }
        }

        // From namespace dependencies
        foreach (var nsDep in NamespaceDependencies)
        {
            namespaces.Add(nsDep.NamespaceName);
        }

        return namespaces.OrderBy(ns => ns).ToList();
    }

    /// <summary>
    /// Get circular dependencies between files
    /// </summary>
    public List<List<FileNode>> GetCircularDependencies()
    {
        var circularDeps = new List<List<FileNode>>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var file in Files)
        {
            if (!visited.Contains(file.Id))
            {
                var cycle = DetectCircularDependency(file.Id, visited, recursionStack, new List<string>());
                if (cycle.Count > 0)
                {
                    var cycleFiles = Files.Where(f => cycle.Contains(f.Id)).ToList();
                    if (cycleFiles.Count > 1)
                    {
                        circularDeps.Add(cycleFiles);
                    }
                }
            }
        }

        return circularDeps;
    }

    /// <summary>
    /// Helper method to detect circular dependencies using DFS
    /// </summary>
    private List<string> DetectCircularDependency(string fileId, HashSet<string> visited, HashSet<string> recursionStack, List<string> path)
    {
        visited.Add(fileId);
        recursionStack.Add(fileId);
        path.Add(fileId);

        var dependencies = GetDependenciesFor(fileId);
        foreach (var dependency in dependencies)
        {
            if (!visited.Contains(dependency.Id))
            {
                var cycle = DetectCircularDependency(dependency.Id, visited, recursionStack, new List<string>(path));
                if (cycle.Count > 0)
                {
                    return cycle;
                }
            }
            else if (recursionStack.Contains(dependency.Id))
            {
                // Found a cycle
                var cycleStart = path.IndexOf(dependency.Id);
                return path.Skip(cycleStart).Concat(new[] { dependency.Id }).ToList();
            }
        }

        recursionStack.Remove(fileId);
        return new List<string>();
    }

    /// <summary>
    /// Get dependency statistics for the analyzed scope
    /// </summary>
    public FileDependencyStatistics GetStatistics()
    {
        var stats = new FileDependencyStatistics
        {
            TotalFiles = Files.Count,
            TotalUsingDependencies = UsingDependencies.Count,
            TotalNamespaceDependencies = NamespaceDependencies.Count,
            TotalAssemblyDependencies = AssemblyDependencies.Count,
            TotalTypeReferenceDependencies = TypeReferenceDependencies.Count,
            UniqueExternalAssemblies = GetExternalAssemblies().Count,
            UniqueNamespaces = GetAllNamespaces().Count,
            CircularDependencyCount = GetCircularDependencies().Count
        };

        // Calculate average dependencies per file
        if (Files.Count > 0)
        {
            stats.AverageDependenciesPerFile = (double)(UsingDependencies.Count + NamespaceDependencies.Count + TypeReferenceDependencies.Count) / Files.Count;
        }

        // Find most dependent file
        var dependencyCounts = Files.ToDictionary(f => f.Id, f => GetDependenciesFor(f.Id).Count);
        if (dependencyCounts.Count > 0)
        {
            var mostDependent = dependencyCounts.OrderByDescending(kvp => kvp.Value).First();
            stats.MostDependentFileId = mostDependent.Key;
            stats.MaxDependenciesPerFile = mostDependent.Value;
        }

        // Find most referenced file
        var referenceCounts = Files.ToDictionary(f => f.Id, f => GetDependentsFor(f.Id).Count);
        if (referenceCounts.Count > 0)
        {
            var mostReferenced = referenceCounts.OrderByDescending(kvp => kvp.Value).First();
            stats.MostReferencedFileId = mostReferenced.Key;
            stats.MaxReferencesPerFile = mostReferenced.Value;
        }

        return stats;
    }
}

/// <summary>
/// Scope types for file dependency analysis
/// </summary>
public enum FileDependencyScope
{
    /// <summary>
    /// Single project analysis
    /// </summary>
    Project,

    /// <summary>
    /// Multi-project solution analysis
    /// </summary>
    Solution,

    /// <summary>
    /// Directory-based analysis
    /// </summary>
    Directory,

    /// <summary>
    /// Custom scope analysis
    /// </summary>
    Custom
}

/// <summary>
/// Statistics about file dependencies
/// </summary>
public class FileDependencyStatistics
{
    /// <summary>
    /// Total number of files analyzed
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Total number of using dependencies
    /// </summary>
    public int TotalUsingDependencies { get; set; }

    /// <summary>
    /// Total number of namespace dependencies
    /// </summary>
    public int TotalNamespaceDependencies { get; set; }

    /// <summary>
    /// Total number of assembly dependencies
    /// </summary>
    public int TotalAssemblyDependencies { get; set; }

    /// <summary>
    /// Total number of type reference dependencies
    /// </summary>
    public int TotalTypeReferenceDependencies { get; set; }

    /// <summary>
    /// Number of unique external assemblies referenced
    /// </summary>
    public int UniqueExternalAssemblies { get; set; }

    /// <summary>
    /// Number of unique namespaces used
    /// </summary>
    public int UniqueNamespaces { get; set; }

    /// <summary>
    /// Number of circular dependency cycles found
    /// </summary>
    public int CircularDependencyCount { get; set; }

    /// <summary>
    /// Average number of dependencies per file
    /// </summary>
    public double AverageDependenciesPerFile { get; set; }

    /// <summary>
    /// Maximum number of dependencies for any single file
    /// </summary>
    public int MaxDependenciesPerFile { get; set; }

    /// <summary>
    /// Maximum number of references to any single file
    /// </summary>
    public int MaxReferencesPerFile { get; set; }

    /// <summary>
    /// ID of the file with the most dependencies
    /// </summary>
    public string MostDependentFileId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the most referenced file
    /// </summary>
    public string MostReferencedFileId { get; set; } = string.Empty;
}