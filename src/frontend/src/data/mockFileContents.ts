// Mock file contents for demonstration
export const mockFileContents: Record<string, string> = {
  // C# files
  'program-cs': `using System;

namespace KnowledgeNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("KnowledgeNetwork Code Analysis");
            var analyzer = new CodeAnalyzer();
            analyzer.Initialize();
        }
    }
}`,

  'codeanalyzer-cs': `using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork
{
    public class CodeAnalyzer
    {
        private readonly List<SyntaxTree> _syntaxTrees;
        
        public CodeAnalyzer()
        {
            _syntaxTrees = new List<SyntaxTree>();
        }
        
        public void Initialize()
        {
            // Initialize code analysis components
            Console.WriteLine("Code analyzer initialized");
        }
        
        public void AnalyzeProject(string projectPath)
        {
            // Analyze C# project files
            foreach (var file in Directory.GetFiles(projectPath, "*.cs"))
            {
                var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                _syntaxTrees.Add(tree);
            }
        }
    }
}`,

  // TypeScript/React files
  'app-tsx': `import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { HomePage } from './pages/HomePage';
import { CodeAnalysisPage } from './pages/CodeAnalysisPage';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-900 text-white">
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/code-analysis" element={<CodeAnalysisPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;`,

  // Configuration files
  'package-json': `{
  "name": "frontend",
  "private": true,
  "version": "0.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "lint": "eslint .",
    "preview": "vite preview"
  },
  "dependencies": {
    "@reduxjs/toolkit": "^2.8.2",
    "react": "^19.1.1",
    "react-dom": "^19.1.1",
    "react-redux": "^9.2.0"
  }
}`,

  // Default content for unknown files
  'default': `// File content not available
// This is a mock file viewer
// In a real application, this would load actual file content from the backend

console.log('Mock file content');`
};

// Helper function to get content by file ID
export function getFileContent(fileId: string): string {
  return mockFileContents[fileId] || mockFileContents['default'];
}

// Helper function to detect language for syntax highlighting
export function getFileLanguage(fileName: string): string {
  const extension = fileName.split('.').pop()?.toLowerCase();
  
  switch (extension) {
    case 'cs': return 'csharp';
    case 'tsx': case 'ts': return 'typescript';
    case 'jsx': case 'js': return 'javascript';
    case 'json': return 'json';
    case 'css': return 'css';
    case 'html': return 'html';
    case 'md': return 'markdown';
    default: return 'text';
  }
}