using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Time-based metadata for entities
/// </summary>
public class TemporalData
{
    /// <summary>
    /// When this entity was created
    /// </summary>
    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this entity was last modified
    /// </summary>
    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this entity was last accessed
    /// </summary>
    [JsonPropertyName("lastAccessed")]
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// Time-to-live in seconds (optional)
    /// </summary>
    [JsonPropertyName("ttlSeconds")]
    public int? TtlSeconds { get; set; }
}