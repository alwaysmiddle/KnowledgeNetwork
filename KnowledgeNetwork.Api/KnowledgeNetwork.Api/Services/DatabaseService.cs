using System.Text.Json;
using Npgsql;
using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException("Connection string not found");

        InitializeDatabase();
    }
    private void InitializeDatabase()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // Drop old tables if they exist
        var dropOldTablesSql = @"
DROP TABLE IF EXISTS nodes;
";

        // Create new GraphNode table with proper schema and versioning
        var createGraphNodesSql = @"
CREATE TABLE IF NOT EXISTS graph_nodes (
id TEXT PRIMARY KEY,
label TEXT NOT NULL,
content TEXT,
position_x DOUBLE PRECISION NOT NULL DEFAULT 0,
position_y DOUBLE PRECISION NOT NULL DEFAULT 0,
types JSONB NOT NULL DEFAULT '[]'::jsonb,
properties JSONB NOT NULL DEFAULT '{}'::jsonb,
created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
version BIGINT NOT NULL DEFAULT 1
);

CREATE INDEX IF NOT EXISTS idx_graph_nodes_label ON graph_nodes(label);
CREATE INDEX IF NOT EXISTS idx_graph_nodes_types ON graph_nodes USING GIN(types);
CREATE INDEX IF NOT EXISTS idx_graph_nodes_created_at ON graph_nodes(created_at);
CREATE INDEX IF NOT EXISTS idx_graph_nodes_version ON graph_nodes(version);
";

        // Create GraphEdge table for full graph support
        var createGraphEdgesSql = @"
CREATE TABLE IF NOT EXISTS graph_edges (
id TEXT PRIMARY KEY,
source_node_id TEXT NOT NULL,
target_node_id TEXT NOT NULL,
label TEXT,
types JSONB NOT NULL DEFAULT '[]'::jsonb,
properties JSONB NOT NULL DEFAULT '{}'::jsonb,
created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
FOREIGN KEY (source_node_id) REFERENCES graph_nodes(id) ON DELETE CASCADE,
FOREIGN KEY (target_node_id) REFERENCES graph_nodes(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_graph_edges_source ON graph_edges(source_node_id);
CREATE INDEX IF NOT EXISTS idx_graph_edges_target ON graph_edges(target_node_id);
CREATE INDEX IF NOT EXISTS idx_graph_edges_types ON graph_edges USING GIN(types);
";

        // Create precomputed layout storage for ultra-fast responses
        var createLayoutsSql = @"
CREATE TABLE IF NOT EXISTS graph_layouts (
id SERIAL PRIMARY KEY,
graph_version BIGINT NOT NULL,
context VARCHAR(50) NOT NULL,
algorithm VARCHAR(100) NOT NULL,  
layout_data JSONB NOT NULL,
computed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
is_current BOOLEAN DEFAULT TRUE,
node_count INTEGER NOT NULL DEFAULT 0,
edge_count INTEGER NOT NULL DEFAULT 0,
computation_time_ms INTEGER NOT NULL DEFAULT 0,

UNIQUE(graph_version, context)
);

CREATE INDEX IF NOT EXISTS idx_graph_layouts_version_context ON graph_layouts(graph_version, context);
CREATE INDEX IF NOT EXISTS idx_graph_layouts_current ON graph_layouts(is_current) WHERE is_current = TRUE;
CREATE INDEX IF NOT EXISTS idx_graph_layouts_computed_at ON graph_layouts(computed_at);
";

        // Create version tracking for invalidation
        var createVersionTrackingSql = @"
CREATE TABLE IF NOT EXISTS graph_version_log (
id SERIAL PRIMARY KEY,
version_number BIGINT NOT NULL,
change_type VARCHAR(50) NOT NULL, -- 'node_created', 'node_updated', 'node_deleted', 'edge_created', 'edge_deleted'
entity_id TEXT NOT NULL,
entity_type VARCHAR(20) NOT NULL, -- 'node', 'edge'
changed_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
metadata JSONB DEFAULT '{}'::jsonb
);

CREATE INDEX IF NOT EXISTS idx_version_log_version ON graph_version_log(version_number);
CREATE INDEX IF NOT EXISTS idx_version_log_entity ON graph_version_log(entity_id, entity_type);
CREATE INDEX IF NOT EXISTS idx_version_log_changed_at ON graph_version_log(changed_at);
";

        // Create sequence for global version tracking
        var createVersionSequenceSql = @"
CREATE SEQUENCE IF NOT EXISTS graph_version_seq START 1;
";

        // Execute all schema creation
        using var dropCommand = new NpgsqlCommand(dropOldTablesSql, connection);
        dropCommand.ExecuteNonQuery();

        using var createNodesCommand = new NpgsqlCommand(createGraphNodesSql, connection);
        createNodesCommand.ExecuteNonQuery();

        using var createEdgesCommand = new NpgsqlCommand(createGraphEdgesSql, connection);
        createEdgesCommand.ExecuteNonQuery();

        using var createLayoutsCommand = new NpgsqlCommand(createLayoutsSql, connection);
        createLayoutsCommand.ExecuteNonQuery();

        using var createVersionLogCommand = new NpgsqlCommand(createVersionTrackingSql, connection);
        createVersionLogCommand.ExecuteNonQuery();

        using var createVersionSeqCommand = new NpgsqlCommand(createVersionSequenceSql, connection);
        createVersionSeqCommand.ExecuteNonQuery();
    }
    public async Task<List<GraphNode>> GetAllGraphNodesAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
SELECT id, label, content, position_x, position_y, types, properties, created_at, updated_at, version 
FROM graph_nodes 
ORDER BY created_at DESC";

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var nodes = new List<GraphNode>();
        while (await reader.ReadAsync())
        {
            var typesJson = reader.GetString(5);
            var propertiesJson = reader.GetString(6);

            var types = JsonSerializer.Deserialize<HashSet<string>>(typesJson) ?? new HashSet<string>();
            var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson) ?? new Dictionary<string, object>();

            nodes.Add(new GraphNode
            {
                Id = reader.GetString(0), // id
                Label = reader.GetString(1), // label
                Content = reader.IsDBNull(2) ? null : reader.GetString(2), // content
                Position = new Position2D
                {
                    X = reader.GetDouble(3), // position_x
                    Y = reader.GetDouble(4)  // position_y
                },
                Types = types,
                Properties = properties,
                CreatedAt = reader.GetDateTime(7), // created_at
                UpdatedAt = reader.GetDateTime(8), // updated_at
                Version = reader.GetInt64(9) // version
            });
        }

        return nodes;
    }
    public async Task<GraphNode?> GetGraphNodeByIdAsync(string id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
SELECT id, label, content, position_x, position_y, types, properties, created_at, updated_at, version 
FROM graph_nodes 
WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var typesJson = reader.GetString(5);
        var propertiesJson = reader.GetString(6);

        var types = JsonSerializer.Deserialize<HashSet<string>>(typesJson) ?? new HashSet<string>();
        var properties = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson) ?? new Dictionary<string, object>();

        return new GraphNode
        {
            Id = reader.GetString(0),
            Label = reader.GetString(1),
            Content = reader.IsDBNull(2) ? null : reader.GetString(2),
            Position = new Position2D
            {
                X = reader.GetDouble(3),
                Y = reader.GetDouble(4)
            },
            Types = types,
            Properties = properties,
            CreatedAt = reader.GetDateTime(7),
            UpdatedAt = reader.GetDateTime(8),
            Version = reader.GetInt64(9)
        };
    }
    public async Task<GraphNode> CreateGraphNodeAsync(CreateGraphNodeRequest request)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            var typesJson = JsonSerializer.Serialize(request.Types);
            var propertiesJson = JsonSerializer.Serialize(request.Properties);

            // Get next version number for change tracking
            const string getVersionSql = "SELECT nextval('graph_version_seq')";
            await using var versionCommand = new NpgsqlCommand(getVersionSql, connection, transaction);
            var version = (long)await versionCommand.ExecuteScalarAsync();

            // Insert the new node with version
            var insertSql = @"
INSERT INTO graph_nodes (id, label, content, position_x, position_y, types, properties, created_at, updated_at, version)
VALUES (@id, @label, @content, @positionX, @positionY, @types::jsonb, @properties::jsonb, @createdAt, @updatedAt, @version)
RETURNING id;";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
            insertCommand.Parameters.AddWithValue("@id", id);
            insertCommand.Parameters.AddWithValue("@label", request.Label);
            insertCommand.Parameters.AddWithValue("@content", request.Content ?? (object)DBNull.Value);
            insertCommand.Parameters.AddWithValue("@positionX", request.Position.X);
            insertCommand.Parameters.AddWithValue("@positionY", request.Position.Y);
            insertCommand.Parameters.AddWithValue("@types", typesJson);
            insertCommand.Parameters.AddWithValue("@properties", propertiesJson);
            insertCommand.Parameters.AddWithValue("@createdAt", now);
            insertCommand.Parameters.AddWithValue("@updatedAt", now);
            insertCommand.Parameters.AddWithValue("@version", version);

            await insertCommand.ExecuteScalarAsync();

            // Log the change for layout invalidation
            const string logSql = @"
INSERT INTO graph_version_log (version_number, change_type, entity_id, entity_type, changed_at, metadata)
VALUES (@version, @changeType, @entityId, @entityType, @changedAt, @metadata::jsonb)";

            await using var logCommand = new NpgsqlCommand(logSql, connection, transaction);
            logCommand.Parameters.AddWithValue("@version", version);
            logCommand.Parameters.AddWithValue("@changeType", "node_created");
            logCommand.Parameters.AddWithValue("@entityId", id);
            logCommand.Parameters.AddWithValue("@entityType", "node");
            logCommand.Parameters.AddWithValue("@changedAt", now);
            logCommand.Parameters.AddWithValue("@metadata", JsonSerializer.Serialize(new { label = request.Label }));

            await logCommand.ExecuteNonQueryAsync();

            // Invalidate existing layouts by marking them as not current
            const string invalidateSql = "UPDATE graph_layouts SET is_current = FALSE WHERE is_current = TRUE";
            await using var invalidateCommand = new NpgsqlCommand(invalidateSql, connection, transaction);
            await invalidateCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            return new GraphNode
            {
                Id = id,
                Label = request.Label,
                Content = request.Content,
                Position = request.Position,
                Types = request.Types,
                Properties = request.Properties,
                CreatedAt = now,
                UpdatedAt = now,
                Version = version
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<GraphNode?> UpdateGraphNodeAsync(string id, UpdateGraphNodeRequest request)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // First, get the current node
        var currentNode = await GetGraphNodeByIdAsync(id);
        if (currentNode == null)
        {
            return null;
        }

        // Update only provided fields
        var updatedNode = new GraphNode
        {
            Id = currentNode.Id,
            Label = request.Label ?? currentNode.Label,
            Content = request.Content ?? currentNode.Content,
            Position = request.Position ?? currentNode.Position,
            Types = request.Types ?? currentNode.Types,
            Properties = request.Properties ?? currentNode.Properties,
            CreatedAt = currentNode.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        var typesJson = JsonSerializer.Serialize(updatedNode.Types);
        var propertiesJson = JsonSerializer.Serialize(updatedNode.Properties);

        var sql = @"
UPDATE graph_nodes 
SET label = @label, content = @content, position_x = @positionX, position_y = @positionY, 
types = @types::jsonb, properties = @properties::jsonb, updated_at = @updatedAt
WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@label", updatedNode.Label);
        command.Parameters.AddWithValue("@content", updatedNode.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@positionX", updatedNode.Position.X);
        command.Parameters.AddWithValue("@positionY", updatedNode.Position.Y);
        command.Parameters.AddWithValue("@types", typesJson);
        command.Parameters.AddWithValue("@properties", propertiesJson);
        command.Parameters.AddWithValue("@updatedAt", updatedNode.UpdatedAt);

        await command.ExecuteNonQueryAsync();
        return updatedNode;
    }

    public async Task<bool> DeleteGraphNodeAsync(string id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM graph_nodes WHERE id = @id";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}