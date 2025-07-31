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

        // Create new GraphNode table with proper schema
        var createTableSql = @"
CREATE TABLE IF NOT EXISTS graph_nodes (
id TEXT PRIMARY KEY,
label TEXT NOT NULL,
content TEXT,
position_x DOUBLE PRECISION NOT NULL DEFAULT 0,
position_y DOUBLE PRECISION NOT NULL DEFAULT 0,
types JSONB NOT NULL DEFAULT '[]'::jsonb,
properties JSONB NOT NULL DEFAULT '{}'::jsonb,
created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_graph_nodes_label ON graph_nodes(label);
CREATE INDEX IF NOT EXISTS idx_graph_nodes_types ON graph_nodes USING GIN(types);
CREATE INDEX IF NOT EXISTS idx_graph_nodes_created_at ON graph_nodes(created_at);
";

        using var dropCommand = new NpgsqlCommand(dropOldTablesSql, connection);
        dropCommand.ExecuteNonQuery();

        using var createCommand = new NpgsqlCommand(createTableSql, connection);
        createCommand.ExecuteNonQuery();
    }

    public async Task<List<GraphNode>> GetAllGraphNodesAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
SELECT id, label, content, position_x, position_y, types, properties, created_at, updated_at 
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
                UpdatedAt = reader.GetDateTime(8)  // updated_at
            });
        }

        return nodes;
    }

    public async Task<GraphNode?> GetGraphNodeByIdAsync(string id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
SELECT id, label, content, position_x, position_y, types, properties, created_at, updated_at 
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
            UpdatedAt = reader.GetDateTime(8)
        };
    }

    public async Task<GraphNode> CreateGraphNodeAsync(CreateGraphNodeRequest request)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var id = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow;
        var typesJson = JsonSerializer.Serialize(request.Types);
        var propertiesJson = JsonSerializer.Serialize(request.Properties);

        var sql = @"
INSERT INTO graph_nodes (id, label, content, position_x, position_y, types, properties, created_at, updated_at)
VALUES (@id, @label, @content, @positionX, @positionY, @types::jsonb, @properties::jsonb, @createdAt, @updatedAt)
RETURNING id;";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@label", request.Label);
        command.Parameters.AddWithValue("@content", request.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@positionX", request.Position.X);
        command.Parameters.AddWithValue("@positionY", request.Position.Y);
        command.Parameters.AddWithValue("@types", typesJson);
        command.Parameters.AddWithValue("@properties", propertiesJson);
        command.Parameters.AddWithValue("@createdAt", now);
        command.Parameters.AddWithValue("@updatedAt", now);

        await command.ExecuteScalarAsync();

        return new GraphNode
        {
            Id = id,
            Label = request.Label,
            Content = request.Content,
            Position = request.Position,
            Types = request.Types,
            Properties = request.Properties,
            CreatedAt = now,
            UpdatedAt = now
        };
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