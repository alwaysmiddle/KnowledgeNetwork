import { useAppSelector } from '../store/hooks';
import { getFileContent, getFileLanguage } from '../data/mockFileContents';
import { mockFileSystem } from '../data/mockFileSystem';
import { type FileNode } from '../types/fileSystem';

export function CodeViewer() {
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);

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

  const selectedFile = selectedFileId ? findFileById(mockFileSystem, selectedFileId) : null;

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

  const fileContent = getFileContent(selectedFile.id);
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
        </div>
      </div>

      {/* Code Content */}
      <div className="flex-1 overflow-auto">
        <pre className="text-sm text-gray-300 p-4 font-mono leading-relaxed">
          <code>{fileContent}</code>
        </pre>
      </div>
    </div>
  );
}