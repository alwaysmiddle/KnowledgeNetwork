using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Extensions
{
    public static class CfgBuilders
    {
        public static GraphNode CreateCfgBlock(string id, List<string> statements, Position2D position, bool isEntry = false, bool isExit = false)
        {
            var types = new HashSet<string> { "cfg-block" };
            if (isEntry) types.Add("cfg-entry");
            if (isExit) types.Add("cfg-exit");
            
            return new GraphNode
            {
                Id = id,
                Label = $"Block {id}",
                Types = types,
                Position = position,
                Properties = new Dictionary<string, object>
                {
                    ["statements"] = statements,
                    ["blockKind"] = statements.Count == 0 ? "empty" : "normal"
                }
            };
        }
        
        public static GraphEdge CreateControlFlowEdge(string sourceId, string targetId, string branchType = "unconditional", string? condition = null)
        {
            var types = new HashSet<string> { "control-flow" };
            
            switch (branchType.ToLower())
            {
                case "true": types.Add("true-branch"); break;
                case "false": types.Add("false-branch"); break;
                case "loop-back": types.Add("loop-back"); break;
            }
            
            var edge = new GraphEdge
            {
                Id = $"{sourceId}->{targetId}",
                SourceNodeId = sourceId,
                TargetNodeId = targetId,
                Types = types,
                Label = branchType == "unconditional" ? null : branchType
            };
            
            if (condition != null)
                edge.Properties["condition"] = condition;
                
            return edge;
        }
    }
}