import { useState } from 'react';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { AppLayout } from '../components/AppLayout';
import { Sidebar } from '../components/Sidebar';
import { FileTree } from '../components/FileTree';
import { CodeViewer } from '../components/CodeViewer';
import { selectFileSystemData } from '../store/fileSystemSlice';
import { getFileSystemWatcher, detectFileSystemWatcherContext, hasNativeDirectoryPicker } from '../services/fileSystemWatcherFactory';
import { ActivityFeed } from '../components/ActivityFeed';

export function CodeAnalysisPage() {
  const dispatch = useAppDispatch();
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [selectedTestPath, setSelectedTestPath] = useState('');
  
  const codeViewerVisible = useAppSelector((state) => state.fileSystem.codeViewerVisible);
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);
  const isLoading = useAppSelector((state) => state.fileSystem.isLoading);
  const error = useAppSelector((state) => state.fileSystem.error);
  const watchingDirectory = useAppSelector((state) => state.fileSystem.watchingDirectory);
  const currentFileSystemData = useAppSelector(selectFileSystemData);

  // Hard-coded test directories for quick validation
  const testDirectories = [
    { label: 'Current Project Root', path: '.' },
    { label: 'Frontend Source', path: './src' },
    { label: 'KnowledgeNetwork1/src', path: './KnowledgeNetwork1/src' },
    { label: 'Parent Directory', path: '..' },
    { label: 'Test Samples', path: './test-samples' },
  ];

  // Get file system context and watcher
  const fsContext = detectFileSystemWatcherContext();
  const watcher = getFileSystemWatcher(dispatch);
  const isWatching = !!watchingDirectory;

  const handleStartWatching = async () => {
    if (!selectedTestPath) return;
    try {
      await watcher.startWatching(selectedTestPath);
    } catch (error) {
      console.error('Failed to start watching:', error);
    }
  };

  const handleSelectNativeDirectory = async () => {
    if (!hasNativeDirectoryPicker() || !window.electronAPI) {
      console.error('Native directory picker not available in current context');
      return;
    }

    try {
      const result = await window.electronAPI.selectDirectory();
      
      if (result.error) {
        console.error('Directory selection error:', result.error);
        return;
      }

      if (!result.canceled && result.filePaths && result.filePaths.length > 0) {
        const selectedPath = result.filePaths[0];
        console.log('📁 Selected directory:', selectedPath);
        
        // Start watching the selected directory
        await watcher.startWatching(selectedPath);
      }
    } catch (error) {
      console.error('Failed to select directory:', error);
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
      {/* File system status */}
      <div className="mb-4 p-3 bg-gray-800 rounded-lg border border-gray-700">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium">File System</span>
          <div className="flex items-center gap-2">
            {fsContext.isElectron && (
              <span className="text-xs bg-purple-600 text-white px-2 py-1 rounded-full">
                ⚡ Electron
              </span>
            )}
            <span className="text-xs bg-green-600 text-white px-2 py-1 rounded-full">
              Real Files
            </span>
          </div>
        </div>
        <div className="text-xs text-gray-400">
          {isLoading && '⏳ Loading...'}
          {error && <span className="text-red-400">❌ {error}</span>}
          {!isLoading && !error && currentFileSystemData && isWatching && (
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-green-400 font-medium">🔍 Watching</span>
                <div className="flex gap-2">
                  <button
                    onClick={handleStopWatching}
                    className="px-2 py-1 bg-red-600 hover:bg-red-700 text-white text-xs rounded transition-colors"
                  >
                    Stop Watching
                  </button>
                </div>
              </div>
              <div className="bg-green-900/20 border border-green-600/30 rounded p-2">
                <div className="text-xs text-green-300 font-mono truncate" title={watchingDirectory}>
                  {watchingDirectory}
                </div>
                <div className="text-xs text-green-500 mt-1">
                  Live file monitoring active
                </div>
              </div>
            </div>
          )}
          {!isLoading && !error && !currentFileSystemData && '📁 No directory selected'}
        </div>
      </div>
      
      {/* Directory Selection Interface */}
      <div className="mb-4 p-4 bg-gray-800 rounded-lg border border-gray-700">
        <h3 className="text-sm font-medium mb-3">
          {fsContext.isElectron ? '🚀 Directory Selection' : '🧪 File Watching Test'}
        </h3>
        
        {!isWatching ? (
          <div className="space-y-3">
            {hasNativeDirectoryPicker() ? (
              // Electron: Native directory picker
              <>
                <button
                  onClick={handleSelectNativeDirectory}
                  disabled={isLoading}
                  className="w-full px-4 py-2 bg-purple-600 hover:bg-purple-700 disabled:bg-gray-600 text-white text-sm rounded-md transition-colors flex items-center justify-center gap-2"
                >
                  {isLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                      Loading...
                    </>
                  ) : (
                    <>
                      📁 Choose Directory to Watch
                    </>
                  )}
                </button>

                <div className="text-xs text-gray-400 bg-purple-900/20 p-2 rounded">
                  <strong>Native Directory Picker:</strong><br/>
                  Select any directory on your system for real-time file monitoring.
                  Visual indicators will show file additions, modifications, and deletions.
                </div>
              </>
            ) : (
              // Browser: Test directory dropdown
              <>
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
              </>
            )}
          </div>
        ) : (
          // Watching state - show change directory option
          <div className="space-y-3">
            <div className="text-center">
              <div className="text-green-400 mb-2">✅ Directory watching is active</div>
            </div>
            
            {hasNativeDirectoryPicker() ? (
              <button
                onClick={handleSelectNativeDirectory}
                disabled={isLoading}
                className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-600 text-white text-sm rounded-md transition-colors flex items-center justify-center gap-2"
              >
                {isLoading ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                    Changing...
                  </>
                ) : (
                  <>
                    🔄 Change Directory
                  </>
                )}
              </button>
            ) : (
              <>
                <div>
                  <label className="block text-xs font-medium mb-1 text-gray-400">
                    Change to Different Directory
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

                <button
                  onClick={handleStartWatching}
                  disabled={!selectedTestPath || isLoading}
                  className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 disabled:bg-gray-600 text-white text-sm rounded-md transition-colors flex items-center justify-center gap-2"
                >
                  {isLoading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-2 border-white border-t-transparent"></div>
                      Changing...
                    </>
                  ) : (
                    <>
                      🔄 Change Directory
                    </>
                  )}
                </button>
              </>
            )}
            
            <div className="text-xs text-gray-400 bg-blue-900/20 p-2 rounded text-center">
              <strong>Tip:</strong> Changing directory will automatically stop watching the current one and start watching the new selection.
            </div>
          </div>
        )}
      </div>

      {/* File Tree */}
      {currentFileSystemData ? (
        <FileTree data={currentFileSystemData} />
      ) : (
        <div className="text-center py-8 text-gray-400">
          <p>No file system data available</p>
          {!isWatching && (
            <p className="text-sm mt-2">Select a directory above to start watching</p>
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
          {/* Activity feed for file changes */}
          <ActivityFeed />

          {!selectedFileId ? (
            <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
              <h2 className="text-xl font-semibold mb-4">Welcome to Code Analysis</h2>
              <p className="text-gray-300 mb-4">
                File watching system active! Start watching a directory to see real-time changes.
              </p>
              <ul className="space-y-2 text-gray-300">
                <li>• View real file content</li>
                <li>• See live file system changes</li>
                <li>• Explore directory structure</li>
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
      </div>
    </AppLayout>
  );
}