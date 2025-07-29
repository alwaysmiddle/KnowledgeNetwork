using Microsoft.Data.Sqlite;
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
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS nodes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                content TEXT,
                node_type TEXT NOT NULL DEFAULT 'concept',
                x_position REAL NOT NULL DEFAULT 0,
                y_position REAL NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";

        using var command = new SqliteCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public async Task<List<Node>> GetAllNodesAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT id, title, content, node_type, x_position, y_position, created_at FROM nodes ORDER BY created_at DESC";
        using var command = new SqliteCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        var nodes = new List<Node>();
        while (await reader.ReadAsync())
        {
            nodes.Add(new Node
            {
                Id = reader.GetInt32(0), // id
                Title = reader.GetString(1), // title
                Content = reader.IsDBNull(2) ? null : reader.GetString(2), // content
                NodeType = reader.GetString(3), // node_type
                XPosition = reader.GetDouble(4), // x_position
                YPosition = reader.GetDouble(5), // y_position
                CreatedAt = DateTime.Parse(reader.GetString(6)) // created_at
            });
        }

        return nodes;
    }

    public async Task<Node> CreateNodeAsync(CreateNodeRequest request)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO nodes (title, content, node_type, x_position, y_position, created_at)
            VALUES (@title, @content, @nodeType, @xPosition, @yPosition, @createdAt);
            SELECT last_insert_rowid();";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@title", request.Title);
        command.Parameters.AddWithValue("@content", request.Content ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@nodeType", request.NodeType);
        command.Parameters.AddWithValue("@xPosition", request.XPosition);
        command.Parameters.AddWithValue("@yPosition", request.YPosition);
        command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

        var id = Convert.ToInt32(await command.ExecuteScalarAsync());

        return new Node
        {
            Id = id,
            Title = request.Title,
            Content = request.Content,
            NodeType = request.NodeType,
            XPosition = request.XPosition,
            YPosition = request.YPosition,
            CreatedAt = DateTime.UtcNow
        };
    }
}