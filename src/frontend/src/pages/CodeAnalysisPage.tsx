import { useState } from 'react';
import { useAppSelector } from '../store/hooks';
import { AppLayout } from '../components/AppLayout';
import { Sidebar } from '../components/Sidebar';
import { FileTree } from '../components/FileTree';
import { CodeViewer } from '../components/CodeViewer';
import { mockFileSystem } from '../data/mockFileSystem';

export function CodeAnalysisPage() {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const codeViewerVisible = useAppSelector((state) => state.fileSystem.codeViewerVisible);
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);

  const sidebarContent = (
    <Sidebar title="File Explorer">
      <FileTree data={mockFileSystem} />
    </Sidebar>
  );

  const codeViewerContent = <CodeViewer />;

  return (
    <AppLayout
      sidebar={sidebarContent}
      sidebarOpen={sidebarOpen}
      onToggleSidebar={() => setSidebarOpen(!sidebarOpen)}
      codeViewer={codeViewerContent}
      codeViewerVisible={codeViewerVisible && !!selectedFileId}
    >
      <div className="p-8">
        <h1 className="text-3xl font-bold text-blue-400 mb-4">Code Analysis</h1>
        <p className="text-gray-400 text-lg mb-8">
          Select a file from the explorer to view and analyze its content
        </p>
        
        {!selectedFileId ? (
          <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
            <h2 className="text-xl font-semibold mb-4">Welcome to Code Analysis</h2>
            <p className="text-gray-300 mb-4">
              Click on any file in the explorer to:
            </p>
            <ul className="space-y-2 text-gray-300">
              <li>• View file content</li>
              <li>• See C# analysis results</li>
              <li>• Explore control flow graphs</li>
              <li>• Interactive code analysis</li>
            </ul>
          </div>
        ) : (
          <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
            <h2 className="text-xl font-semibold mb-4">Analysis Dashboard</h2>
            <p className="text-gray-300 mb-4">
              File content is displayed in the code viewer panel →
            </p>
            <div className="grid grid-cols-2 gap-4">
              <div className="bg-gray-700 rounded-lg p-4">
                <h3 className="font-semibold mb-2">Code Metrics</h3>
                <p className="text-sm text-gray-400">Lines, complexity, etc.</p>
              </div>
              <div className="bg-gray-700 rounded-lg p-4">
                <h3 className="font-semibold mb-2">Analysis Results</h3>
                <p className="text-sm text-gray-400">Issues, suggestions, etc.</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </AppLayout>
  );
}