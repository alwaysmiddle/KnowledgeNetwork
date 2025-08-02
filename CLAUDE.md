# CLAUDE.md - Knowledge Network Development Best Practices

## 📚 Resources & References

### Documentation
- [React Flow Documentation](https://reactflow.dev/docs)
- [Tauri Documentation](https://tauri.app/v1/guides/)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)

### Architecture Patterns
- Local-First Software: [ink & switch](https://www.inkandswitch.com/local-first/)
- Graph Database Design Patterns
- Event Sourcing for Knowledge Management

### Performance Resources
- React Performance Best Practices
- .NET Performance Tips

---

## 🚀 Quick Start Commands

```bash
# Start development environment
npm run dev              # Frontend with hot reload
dotnet run --project KnowledgeNetwork.Api  # Backend API
npm run tauri dev        # Full desktop app

# Build for production
npm run build            # Frontend build
dotnet publish -c Release  # Backend build
npm run tauri build      # Desktop application

# Testing
npm test                 # Frontend tests
dotnet test              # Backend tests

# Database operations
dotnet ef migrations add <Name>  # When using EF Core later
dotnet ef database update       # Apply migrations
```

---

## 🖥️ Development Server Management

**FOLLOWS UNIVERSAL RULES: See `/AGENT-DEV-SERVER-RULES.md`**

### Knowledge Network API Server
- **Port**: 5000 (HTTP)
- **Health Check**: `GET /api/CodeAnalysis/health`
- **Swagger UI**: http://localhost:5000/swagger
- **Project Path**: `KnowledgeNetwork.Api/KnowledgeNetwork.Api`

### Standard Commands for Agents
```bash
# Check if API is running
curl -s http://localhost:5000/api/CodeAnalysis/health

# Start API (only if not running)
cd KnowledgeNetwork.Api/KnowledgeNetwork.Api
dotnet run --urls "http://localhost:5000"

# Test endpoint
curl -X POST http://localhost:5000/api/CodeAnalysis/analyze \
  -H "Content-Type: application/json" \
  -d '{"code":"class Test { static void Main() { } }"}'
```

### Process Management Files
- `.current-pid` - Contains running API process ID
- `run-api.bat` - Windows startup script
- `process-manager.ps1` - Smart process management
- `api-check.bat` - Quick health check

### MANDATORY: Always check before starting!
1. Test health endpoint first
2. If healthy → reuse existing server
3. If not running → start new instance
4. Save PID for future reference

---

**Remember: This is a local-first application that prioritizes user control, performance, and privacy while maintaining a clear path to cloud collaboration when needed.**
