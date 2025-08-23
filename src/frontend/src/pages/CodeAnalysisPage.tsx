import { useState } from 'react';
import { AppLayout } from '../components/AppLayout';
import { Sidebar } from '../components/Sidebar';
import { FileTree } from '../components/FileTree';
import { mockFileSystem } from '../data/mockFileSystem';

export function CodeAnalysisPage() {
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const sidebarContent = (
    <Sidebar title="File Explorer">
      <FileTree data={mockFileSystem} />
    </Sidebar>
  );

  return (
    <AppLayout
      sidebar={sidebarContent}
      sidebarOpen={sidebarOpen}
      onToggleSidebar={() => setSidebarOpen(!sidebarOpen)}
    >
      <div className="p-8">
        <h1 className="text-3xl font-bold text-blue-400 mb-4">Code Analysis</h1>
        <p className="text-gray-400 text-lg mb-8">
          Select a file from the explorer to view and analyze its content
        </p>
        
        <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
          <h2 className="text-xl font-semibold mb-4">Main Content Area</h2>
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
      </div>
    </AppLayout>
  );
}