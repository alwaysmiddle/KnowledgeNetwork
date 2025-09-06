namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Complexity metrics for a class
/// </summary>
public class ClassComplexityMetrics
{
    /// <summary>
    /// Total lines of code in the class
    /// </summary>
    public int TotalLineCount { get; set; }

    /// <summary>
    /// Number of public members
    /// </summary>
    public int PublicMemberCount { get; set; }

    /// <summary>
    /// Number of dependencies (other classes this class uses)
    /// </summary>
    public int DependencyCount { get; set; }

    /// <summary>
    /// Number of classes that depend on this class
    /// </summary>
    public int DependentCount { get; set; }

    /// <summary>
    /// Inheritance depth (how many levels deep in inheritance hierarchy)
    /// </summary>
    public int InheritanceDepth { get; set; }

    /// <summary>
    /// Number of interfaces implemented
    /// </summary>
    public int InterfaceCount { get; set; }

    /// <summary>
    /// Weighted Methods per Class (WMC) - sum of method complexities
    /// </summary>
    public int WeightedMethodsPerClass { get; set; }

    /// <summary>
    /// Response for Class (RFC) - number of methods that can be invoked
    /// </summary>
    public int ResponseForClass { get; set; }

    /// <summary>
    /// Lack of Cohesion of Methods (LCOM) - measure of class cohesion
    /// </summary>
    public double LackOfCohesion { get; set; }

    /// <summary>
    /// Coupling Between Objects (CBO) - number of classes coupled to this class
    /// </summary>
    public int CouplingBetweenObjects { get; set; }
}