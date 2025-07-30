using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Extensions
{
    public static class CfgNodeExtensions
    {
        public static bool IsCfgBlock(this GraphNode node) => node.Types.Contains("cfg-block");
        
        public static bool IsEntryBlock(this GraphNode node) => node.Types.Contains("cfg-entry");
        
        public static bool IsExitBlock(this GraphNode node) => node.Types.Contains("cfg-exit");
        
        public static List<string> GetStatements(this GraphNode node) =>
            node.Properties.GetValueOrDefault("statements") as List<string> ?? new();
        
        public static string GetBlockKind(this GraphNode node) =>
            node.Properties.GetValueOrDefault("blockKind")?.ToString() ?? "normal";
            
        public static void SetStatements(this GraphNode node, List<string> statements) =>
            node.Properties["statements"] = statements;
            
        public static void SetBlockKind(this GraphNode node, string blockKind) =>
            node.Properties["blockKind"] = blockKind;
    }
}