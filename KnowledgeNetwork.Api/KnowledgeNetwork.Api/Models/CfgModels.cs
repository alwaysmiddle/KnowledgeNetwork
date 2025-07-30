namespace KnowledgeNetwork.Api.Models
{
    public class CfgExtractionRequest
    {
        public required string CSharpCode { get; set; }
        public string? MethodName { get; set; }
        public string? GraphName { get; set; }
    }
}