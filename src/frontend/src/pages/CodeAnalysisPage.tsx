import { useState } from 'react';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { AppLayout } from '../components/AppLayout';
import { Sidebar } from '../components/Sidebar';
import { FileTree } from '../components/FileTree';
import { CodeViewer } from '../components/CodeViewer';
import { mockFileSystem } from '../data/mockFileSystem';
import { selectCurrentFileSystemData, toggleFileSystemMode } from '../store/fileSystemSlice';
import { getMockFileSystemWatcher } from '../services/mockFileWatcher';
import { ActivityFeed } from '../components/ActivityFeed';

export function CodeAnalysisPage() {
  const dispatch = useAppDispatch();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [selectedTestPath, setSelectedTestPath] = useState('');
  
  const codeViewerVisible = useAppSelector((state) => state.fileSystem.codeViewerVisible);
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);
  const isUsingRealFileSystem = useAppSelector((state) => state.fileSystem.isUsingRealFileSystem);
  const isLoading = useAppSelector((state) => state.fileSystem.isLoading);
  const error = useAppSelector((state) => state.fileSystem.error);
  const watchingDirectory = useAppSelector((state) => state.fileSystem.watchingDirectory);
  const currentFileSystemData = useAppSelector((state) => 
    selectCurrentFileSystemData(state, mockFileSystem)
  );

  // Hard-coded test directories for quick validation
  const testDirectories = [
    { label: 'Current Project Root', path: '.' },
    { label: 'Frontend Source', path: './src' },
    { label: 'KnowledgeNetwork1/src', path: './KnowledgeNetwork1/src' },
    { label: 'Parent Directory', path: '..' },
    { label: 'Test Samples', path: './test-samples' },
  ];

  const watcher = getMockFileSystemWatcher(dispatch);
  const isWatching = !!watchingDirectory;

  const handleStartWatching = async () => {
    if (!selectedTestPath) return;
    try {
      await watcher.startWatching(selectedTestPath);
    } catch (error) {
      console.error('Failed to start watching:', error);
    }
  };

  const handleStopWatching = async () => {
    try {
      await watcher.stopWatching();
    } catch (error) {
      console.error('Failed to stop watching:', error);
    }
  };

  const sidebarContent = (
    <Sidebar title="File Explorer">
      {/* File system mode toggle */}
      <div className="mb-4 p-3 bg-gray-800 rounded-lg border border-gray-700">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium">Data Source</span>
          <button
            onClick={() => dispatch(toggleFileSystemMode())}
            className={`px-3 py-1 text-xs rounded-full transition-colors ${
              isUsingRealFileSystem 
                ? 'bg-green-600 text-white' 
                : 'bg-blue-600 text-white'
            }`}
          >
            {isUsingRealFileSystem ? 'Real Files' : 'Mock Data'}
          </button>
        </div>
        {isUsingRealFileSystem && (
          <div className="text-xs text-gray-400">
            {isLoading && '⏳ Loading...'}
            {error && <span className="text-red-400">❌ {error}</span>}
            {!isLoading && !error && currentFileSystemData && isWatching && (
              <div className="flex items-center justify-between">
                <span className="text-green-400">🔍 Watching: {watchingDirectory}</span>
                <button
                  onClick={handleStopWatching}
                  className="px-2 py-1 bg-red-600 hover:bg-red-700 text-white text-xs rounded transition-colors"
                >
                  Stop
                </button>
              </div>
            )}
            {!isLoading && !error && !currentFileSystemData && '📁 No directory selected'}
          </div>
        )}
      </div>
      
      {/* Real File System Testing Interface */}
      {isUsingRealFileSystem && !isWatching && (
        <div className="mb-4 p-4 bg-gray-800 rounded-lg border border-gray-700">
          <h3 className="text-sm font-medium mb-3">🧪 File Watching Test</h3>
          
          <div className="space-y-3">
            {/* Directory Selection Dropdown */}
            <div>
              <label className="block text-xs font-medium mb-1 text-gray-400">
                Test Directory
              </label>
              <select
                value={selectedTestPath}
                onChange={(e) => setSelectedTestPath(e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-sm focus:outline-none focus:border-blue-500"
              >
                <option value="">Select directory to watch...</option>
                {testDirectories.map((dir) => (
                  <option key={dir.path} value={dir.path}>
                    {dir.label} ({dir.path})
                  </option>
                ))}
              </select>
            </div>

            {/* Start Watching Button */}
            <button
              onClick={handleStartWatching}
              disabled={!selectedTestPath || isLoading}
              className="w-full px-4 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 text-white text-sm rounded-md transition-colors flex items-center justify-center gap-2"
            >
              {isLoading ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                  Starting...
                </>
              ) : (
                <>
                  🔍 Start Watching
                </>
              )}
            </button>

            <div className="text-xs text-gray-400 bg-blue-900/20 p-2 rounded">
              <strong>Test Instructions:</strong><br/>
              1. Select a directory above<br/>
              2. Click "Start Watching"<br/>
              3. Create/modify files in that directory<br/>
              4. Watch the tree update in real-time!
            </div>
          </div>
        </div>
      )}

      {/* File Tree */}
      {currentFileSystemData ? (
        <FileTree data={currentFileSystemData} />
      ) : (
        <div className="text-center py-8 text-gray-400">
          <p>No file system data available</p>
          {isUsingRealFileSystem && !isWatching && (
            <p className="text-sm mt-2">Select a test directory above</p>
          )}
        </div>
      )}
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
        
{/* Main content with activity feed for file watching */}
        <div className="space-y-6">
          {/* Activity feed when using real file system */}
          {isUsingRealFileSystem && (
            <ActivityFeed />
          )}

          {!selectedFileId ? (
            <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
              <h2 className="text-xl font-semibold mb-4">Welcome to Code Analysis</h2>
              <p className="text-gray-300 mb-4">
                {isUsingRealFileSystem 
                  ? "File watching system active! Start watching a directory to see real-time changes."
                  : "Click on any file in the explorer to:"
                }
              </p>
              {!isUsingRealFileSystem && (
                <ul className="space-y-2 text-gray-300">
                  <li>• View file content</li>
                  <li>• See C# analysis results</li>
                  <li>• Explore control flow graphs</li>
                  <li>• Interactive code analysis</li>
                </ul>
              )}
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
      </div>
    </AppLayout>
  );
}