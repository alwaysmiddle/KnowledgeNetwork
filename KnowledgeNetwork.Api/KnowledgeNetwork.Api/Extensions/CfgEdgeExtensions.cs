using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Extensions
{
    public static class CfgEdgeExtensions
    {
        public static bool IsControlFlow(this GraphEdge edge) => edge.Types.Contains("control-flow");
        
        public static bool IsTrueBranch(this GraphEdge edge) => edge.Types.Contains("true-branch");
        
        public static bool IsFalseBranch(this GraphEdge edge) => edge.Types.Contains("false-branch");
        
        public static bool IsLoopBack(this GraphEdge edge) => edge.Types.Contains("loop-back");
        
        public static string GetCondition(this GraphEdge edge) =>
            edge.Properties.GetValueOrDefault("condition")?.ToString() ?? "";
            
        public static void SetCondition(this GraphEdge edge, string condition) =>
            edge.Properties["condition"] = condition;
    }
}