namespace KnowledgeNetwork.Api.Models.Analysis
{
    /// <summary>
    /// Represents a location in source code with rich position information
    /// </summary>
    public record SourceLocation
    {
        /// <summary>
        /// The file path where this element is defined
        /// </summary>
        public required string FilePath { get; init; }
        
        /// <summary>
        /// The line number (1-based) where the element starts
        /// </summary>
        public required int StartLine { get; init; }
        
        /// <summary>
        /// The column number (1-based) where the element starts
        /// </summary>
        public required int StartColumn { get; init; }
        
        /// <summary>
        /// The line number (1-based) where the element ends
        /// </summary>
        public required int EndLine { get; init; }
        
        /// <summary>
        /// The column number (1-based) where the element ends
        /// </summary>
        public required int EndColumn { get; init; }
        
        /// <summary>
        /// The absolute character position where the element starts
        /// </summary>
        public required int StartPosition { get; init; }
        
        /// <summary>
        /// The absolute character position where the element ends
        /// </summary>
        public required int EndPosition { get; init; }
        
        /// <summary>
        /// Gets the length of the span
        /// </summary>
        public int Length => EndPosition - StartPosition;
    }
}