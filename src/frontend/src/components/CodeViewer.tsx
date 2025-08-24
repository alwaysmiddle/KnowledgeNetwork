import { useState, useEffect } from 'react';
import { useAppSelector } from '../store/hooks';
import { selectFileSystemData } from '../store/fileSystemSlice';
import { type FileNode } from '../types/fileSystem';

export function CodeViewer() {
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);
  const fileSystemData = useAppSelector(selectFileSystemData);
  const [fileContent, setFileContent] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Find the selected file node
  const findFileById = (node: FileNode, id: string): FileNode | null => {
    if (node.id === id) return node;
    if (node.children) {
      for (const child of node.children) {
        const found = findFileById(child, id);
        if (found) return found;
      }
    }
    return null;
  };

  const selectedFile = selectedFileId && fileSystemData ? findFileById(fileSystemData, selectedFileId) : null;

  // Helper function to detect language for syntax highlighting
  const getFileLanguage = (fileName: string): string => {
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
  };

  // Load file content when selected file changes
  useEffect(() => {
    if (!selectedFile || selectedFile.type !== 'file') {
      setFileContent('');
      setError(null);
      return;
    }

    const loadFileContent = async () => {
      setLoading(true);
      setError(null);

      try {
        // Check if Electron API is available
        if (!window.electronAPI?.fileSystem?.readFile) {
          setError('File reading not available - requires Electron context');
          setLoading(false);
          return;
        }

        // Read file content via Electron IPC
        const result = await window.electronAPI.fileSystem.readFile(selectedFile.path);

        if (result.error) {
          setError(`Failed to load file: ${result.error}`);
          setFileContent('');
        } else if (result.success && result.content !== undefined) {
          setFileContent(result.content);
          setError(null);
        } else {
          setError('Unexpected response when loading file');
          setFileContent('');
        }
      } catch (error) {
        console.error('Error loading file content:', error);
        setError(error instanceof Error ? error.message : 'Unknown error loading file');
        setFileContent('');
      } finally {
        setLoading(false);
      }
    };

    loadFileContent();
  }, [selectedFile]);

  if (!selectedFile || selectedFile.type !== 'file') {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-850">
        <div className="text-center text-gray-400">
          <div className="text-6xl mb-4">📄</div>
          <h2 className="text-xl font-semibold mb-2">No File Selected</h2>
          <p>Select a file from the explorer to view its contents</p>
        </div>
      </div>
    );
  }

  const language = getFileLanguage(selectedFile.name);

  return (
    <div className="flex-1 flex flex-col bg-gray-850">
      {/* File Header */}
      <div className="flex items-center px-4 py-2 bg-gray-800 border-b border-gray-700">
        <div className="flex items-center gap-2">
          <span className="text-sm text-gray-300">{selectedFile.name}</span>
          <span className="text-xs text-gray-500 bg-gray-700 px-2 py-1 rounded">
            {language}
          </span>
          {loading && (
            <span className="text-xs text-blue-400">Loading...</span>
          )}
        </div>
      </div>

      {/* Code Content */}
      <div className="flex-1 overflow-auto">
        {error ? (
          <div className="p-4 text-red-400">
            <p>Error loading file content:</p>
            <p className="text-sm mt-1">{error}</p>
          </div>
        ) : (
          <pre className="text-sm text-gray-300 p-4 font-mono leading-relaxed">
            <code>{fileContent}</code>
          </pre>
        )}
      </div>
    </div>
  );
}