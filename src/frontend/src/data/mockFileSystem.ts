import { type FileNode } from '../types/fileSystem';

export const mockFileSystem: FileNode = {
  id: 'root',
  name: 'KnowledgeNetwork1',
  type: 'folder',
  path: '/',
  children: [
    {
      id: 'src',
      name: 'src',
      type: 'folder',
      path: '/src',
      children: [
        {
          id: 'backend',
          name: 'backend',
          type: 'folder',
          path: '/src/backend',
          children: [
            {
              id: 'controllers',
              name: 'Controllers',
              type: 'folder',
              path: '/src/backend/Controllers',
              children: [
                {
                  id: 'code-analysis-controller',
                  name: 'CodeAnalysisController.cs',
                  type: 'file',
                  path: '/src/backend/Controllers/CodeAnalysisController.cs',
                  extension: 'cs',
                  size: 4523,
                  modified: '2024-01-15T10:30:00'
                },
                {
                  id: 'graph-controller',
                  name: 'GraphController.cs',
                  type: 'file',
                  path: '/src/backend/Controllers/GraphController.cs',
                  extension: 'cs',
                  size: 2156,
                  modified: '2024-01-15T09:45:00'
                }
              ]
            },
            {
              id: 'services',
              name: 'Services',
              type: 'folder',
              path: '/src/backend/Services',
              children: [
                {
                  id: 'csharp-analysis-service',
                  name: 'CSharpAnalysisService.cs',
                  type: 'file',
                  path: '/src/backend/Services/CSharpAnalysisService.cs',
                  extension: 'cs',
                  size: 8934,
                  modified: '2024-01-15T11:20:00'
                },
                {
                  id: 'cfg-analyzer',
                  name: 'CSharpControlFlowAnalyzer.cs',
                  type: 'file',
                  path: '/src/backend/Services/CSharpControlFlowAnalyzer.cs',
                  extension: 'cs',
                  size: 6782,
                  modified: '2024-01-15T14:10:00'
                }
              ]
            },
            {
              id: 'models',
              name: 'Models',
              type: 'folder',
              path: '/src/backend/Models',
              children: [
                {
                  id: 'knowledge-node',
                  name: 'KnowledgeNode.cs',
                  type: 'file',
                  path: '/src/backend/Models/KnowledgeNode.cs',
                  extension: 'cs',
                  size: 3456,
                  modified: '2024-01-14T16:30:00'
                }
              ]
            }
          ]
        },
        {
          id: 'frontend',
          name: 'frontend',
          type: 'folder',
          path: '/src/frontend',
          children: [
            {
              id: 'src-frontend',
              name: 'src',
              type: 'folder',
              path: '/src/frontend/src',
              children: [
                {
                  id: 'app-tsx',
                  name: 'App.tsx',
                  type: 'file',
                  path: '/src/frontend/src/App.tsx',
                  extension: 'tsx',
                  size: 2156,
                  modified: '2024-01-15T14:20:00'
                },
                {
                  id: 'main-tsx',
                  name: 'main.tsx',
                  type: 'file',
                  path: '/src/frontend/src/main.tsx',
                  extension: 'tsx',
                  size: 456,
                  modified: '2024-01-15T09:00:00'
                },
                {
                  id: 'components',
                  name: 'components',
                  type: 'folder',
                  path: '/src/frontend/src/components',
                  children: [
                    {
                      id: 'app-layout',
                      name: 'AppLayout.tsx',
                      type: 'file',
                      path: '/src/frontend/src/components/AppLayout.tsx',
                      extension: 'tsx',
                      size: 1543,
                      modified: '2024-01-15T15:30:00'
                    },
                    {
                      id: 'navigation-tsx',
                      name: 'Navigation.tsx',
                      type: 'file',
                      path: '/src/frontend/src/components/Navigation.tsx',
                      extension: 'tsx',
                      size: 1234,
                      modified: '2024-01-15T13:15:00'
                    }
                  ]
                },
                {
                  id: 'pages',
                  name: 'pages',
                  type: 'folder',
                  path: '/src/frontend/src/pages',
                  children: [
                    {
                      id: 'home-page',
                      name: 'HomePage.tsx',
                      type: 'file',
                      path: '/src/frontend/src/pages/HomePage.tsx',
                      extension: 'tsx',
                      size: 3421,
                      modified: '2024-01-15T14:25:00'
                    },
                    {
                      id: 'code-analysis-page',
                      name: 'CodeAnalysisPage.tsx',
                      type: 'file',
                      path: '/src/frontend/src/pages/CodeAnalysisPage.tsx',
                      extension: 'tsx',
                      size: 1876,
                      modified: '2024-01-15T14:30:00'
                    }
                  ]
                }
              ]
            },
            {
              id: 'package-json',
              name: 'package.json',
              type: 'file',
              path: '/src/frontend/package.json',
              extension: 'json',
              size: 1543,
              modified: '2024-01-15T12:00:00'
            },
            {
              id: 'vite-config',
              name: 'vite.config.ts',
              type: 'file',
              path: '/src/frontend/vite.config.ts',
              extension: 'ts',
              size: 234,
              modified: '2024-01-14T10:00:00'
            }
          ]
        },
        {
          id: 'test-samples',
          name: 'test-samples',
          type: 'folder',
          path: '/src/test-samples',
          children: [
            {
              id: 'csharp-samples',
              name: 'csharp',
              type: 'folder',
              path: '/src/test-samples/csharp',
              children: [
                {
                  id: 'simple-folder',
                  name: 'simple',
                  type: 'folder',
                  path: '/src/test-samples/csharp/simple',
                  children: [
                    {
                      id: 'basic-class',
                      name: 'BasicClass.cs',
                      type: 'file',
                      path: '/src/test-samples/csharp/simple/BasicClass.cs',
                      extension: 'cs',
                      size: 567,
                      modified: '2024-01-14T15:00:00'
                    },
                    {
                      id: 'hello-world',
                      name: 'HelloWorld.cs',
                      type: 'file',
                      path: '/src/test-samples/csharp/simple/HelloWorld.cs',
                      extension: 'cs',
                      size: 234,
                      modified: '2024-01-14T15:05:00'
                    }
                  ]
                }
              ]
            }
          ]
        }
      ]
    },
    {
      id: 'docs',
      name: 'docs',
      type: 'folder',
      path: '/docs',
      children: [
        {
          id: 'readme',
          name: 'README.md',
          type: 'file',
          path: '/docs/README.md',
          extension: 'md',
          size: 1234,
          modified: '2024-01-15T08:00:00'
        },
        {
          id: 'api-docs',
          name: 'api-documentation.md',
          type: 'file',
          path: '/docs/api-documentation.md',
          extension: 'md',
          size: 5678,
          modified: '2024-01-15T10:00:00'
        }
      ]
    },
    {
      id: 'tests',
      name: 'tests',
      type: 'folder',
      path: '/tests',
      children: [
        {
          id: 'unit-tests',
          name: 'unit',
          type: 'folder',
          path: '/tests/unit',
          children: [
            {
              id: 'analysis-tests',
              name: 'AnalysisTests.cs',
              type: 'file',
              path: '/tests/unit/AnalysisTests.cs',
              extension: 'cs',
              size: 3456,
              modified: '2024-01-15T09:30:00'
            }
          ]
        }
      ]
    }
  ]
};