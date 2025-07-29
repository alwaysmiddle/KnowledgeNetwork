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

**Remember: This is a local-first application that prioritizes user control, performance, and privacy while maintaining a clear path to cloud collaboration when needed.**
