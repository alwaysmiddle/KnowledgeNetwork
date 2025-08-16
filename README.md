# Knowledge Network

A modern multi-language code analysis and visualization platform built with .NET 9 and React 19.

## Overview

Knowledge Network provides intelligent code analysis and interactive graph visualization for multiple programming languages. It combines the power of compiler APIs (like Roslyn for C#) with advanced visualization techniques to help developers understand complex codebases.

## Architecture

- **Backend**: .NET 9 with clean architecture (Core, Infrastructure, Domains, API)
- **Frontend**: React 19 + TypeScript + Vite + Tailwind CSS + G6 visualization
- **Visualization**: AntV G6 for high-performance graph rendering
- **Analysis**: Direct compiler API integration for maximum accuracy

## Quick Start

### Prerequisites

- .NET 9 SDK
- Node.js 18+ 
- Git

### Backend

```bash
cd src/
dotnet build
dotnet run --project KnowledgeNetwork.Api
```

API will be available at `http://localhost:5000`

### Frontend

```bash
cd src/frontend/
npm install
npm run dev
```

Frontend will be available at `http://localhost:3000`

## Project Structure

```
KnowledgeNetwork/
├── src/                              # All source code
│   ├── KnowledgeNetwork.Core/        # Domain abstractions
│   ├── KnowledgeNetwork.Infrastructure/ # Data access, caching
│   ├── KnowledgeNetwork.Domains.Code/  # Code analysis domain
│   ├── KnowledgeNetwork.Api/         # ASP.NET Core API
│   ├── KnowledgeNetwork.sln          # Solution file
│   └── frontend/                     # React application
├── test-data/                        # Test files and examples
├── global.json                       # .NET SDK configuration
└── README.md                         # This file
```

## Current Status

- ✅ **Phase 0 Complete**: Clean architecture foundation
- ✅ **Backend**: .NET 9 with layered architecture 
- ✅ **Frontend**: React 19 + modern toolchain
- ✅ **Build Pipeline**: Automated build verification
- 🚧 **Phase 1**: C# language support (in progress)

## Technology Stack

### Backend
- .NET 9 (latest)
- ASP.NET Core Web API
- Roslyn Compiler APIs
- Clean Architecture pattern

### Frontend  
- React 19.1.1
- TypeScript 5.8.3
- Vite 7.1.2 (build tool)
- Tailwind CSS 4.1.12
- AntV G6 5.0.49 (visualization)
- Redux Toolkit 2.8.2

## Development

### Build Verification

Both projects include automated build verification:

```bash
# Backend
cd src/
dotnet build

# Frontend  
cd src/frontend/
npm run build
```

### Adding Languages

The architecture supports multiple programming languages:

1. Implement `ILanguageAnalyzer<TResult>`
2. Create language-specific analysis result
3. Add visualization layout engine
4. Update orchestration layer

## License

See [LICENSE](LICENSE) for details.

## Contributing

This project follows clean architecture principles and incremental development practices. See the codebase architecture for detailed implementation guidance.