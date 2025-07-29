# Knowledge Network API

A minimal .NET 8 Web API for the Knowledge Network MVP with SQLite integration.

## Quick Start

1. **Restore packages and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```
   
   The API will start on `http://localhost:5000`

3. **Test the API:**
   ```powershell
   # Run the test script
   .\test-api.ps1
   ```

## API Endpoints

### Health Check
- **GET** `/api/health`
- Returns: `{ "status": "OK", "timestamp": "2024-01-01T00:00:00Z" }`

### Nodes
- **GET** `/api/nodes` - Retrieve all nodes
- **POST** `/api/nodes` - Create a new node

#### Create Node Request Body:
```json
{
  "title": "My Node Title",
  "content": "Optional content text",
  "nodeType": "concept",
  "xPosition": 100.0,
  "yPosition": 200.0
}
```

#### Node Response:
```json
{
  "id": 1,
  "title": "My Node Title",
  "content": "Optional content text",
  "nodeType": "concept",
  "xPosition": 100.0,
  "yPosition": 200.0,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

## Database

The API uses SQLite with a local database file `knowledge_network.db` that will be created automatically on first run.

## CORS Configuration

CORS is enabled for development servers running on:
- `http://localhost:5173` (Vite default)
- `http://localhost:3000` (Create React App default)
- `http://localhost:1420` (Tauri default)

## Project Structure

```
KnowledgeNetwork.Api/
├── Controllers/
│   ├── HealthController.cs      # Health check endpoint
│   └── NodesController.cs       # Node CRUD operations
├── Models/
│   └── Node.cs                  # Node data models
├── Services/
│   └── DatabaseService.cs      # SQLite database operations
├── Program.cs                   # Application startup
├── appsettings.json            # Configuration
└── KnowledgeNetwork.Api.csproj # Project file
```

## Development Notes

- The API is designed for local-first operation
- SQLite database is initialized automatically with required schema
- Basic error handling is included for MVP functionality
- Swagger UI available at `/swagger` in development mode