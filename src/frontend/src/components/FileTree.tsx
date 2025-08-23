import { useEffect, useRef } from 'react';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { selectFile, initializeFolders, toggleFolder, clearSearch } from '../store/fileSystemSlice';
import { type FileNode } from '../types/fileSystem';
import { FileTreeNode } from './FileTreeNode';
import { SearchBox } from './SearchBox';

interface FileTreeProps {
  data: FileNode;
}

export function FileTree({ data }: FileTreeProps) {
  const dispatch = useAppDispatch();
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);
  const searchResults = useAppSelector((state) => state.fileSystem.searchResults);
  const treeRef = useRef<HTMLDivElement>(null);

  // Initialize all folders to be expanded by default
  useEffect(() => {
    const collectFolderIds = (node: FileNode): string[] => {
      const ids: string[] = [];
      if (node.type === 'folder') {
        ids.push(node.id);
        if (node.children) {
          node.children.forEach(child => {
            ids.push(...collectFolderIds(child));
          });
        }
      }
      return ids;
    };

    const folderIds = collectFolderIds(data);
    dispatch(initializeFolders(folderIds));
  }, [data, dispatch]);

  // Helper to get all files in tree order
  const getAllFiles = (node: FileNode, files: FileNode[] = []): FileNode[] => {
    files.push(node);
    if (node.children) {
      node.children.forEach(child => getAllFiles(child, files));
    }
    return files;
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.target instanceof HTMLInputElement) return; // Don't interfere with search input
      
      const allFiles = getAllFiles(data);
      const currentIndex = selectedFileId ? allFiles.findIndex(f => f.id === selectedFileId) : -1;
      
      switch (e.key) {
        case 'ArrowDown':
          e.preventDefault();
          if (currentIndex < allFiles.length - 1) {
            const nextFile = allFiles[currentIndex + 1];
            if (nextFile.type === 'file') {
              dispatch(selectFile(nextFile.id));
            }
          }
          break;
          
        case 'ArrowUp':
          e.preventDefault();
          if (currentIndex > 0) {
            const prevFile = allFiles[currentIndex - 1];
            if (prevFile.type === 'file') {
              dispatch(selectFile(prevFile.id));
            }
          }
          break;
          
        case 'Enter':
          if (selectedFileId) {
            const selectedFile = allFiles.find(f => f.id === selectedFileId);
            if (selectedFile?.type === 'folder') {
              dispatch(toggleFolder(selectedFile.id));
            }
          }
          break;
          
        case 'Escape':
          // Clear search
          if (searchResults.length > 0) {
            dispatch(clearSearch());
          }
          break;
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [selectedFileId, searchResults, dispatch, data]);

  const handleFileSelect = (node: FileNode) => {
    dispatch(selectFile(node.id));
    console.log('Selected file:', node.name, 'ID:', node.id);
  };

  return (
    <div ref={treeRef} className="text-sm">
      <SearchBox />
      <div className="max-h-full overflow-auto">
        <FileTreeNode 
          node={data} 
          selectedFileId={selectedFileId}
          onFileSelect={handleFileSelect}
        />
      </div>
    </div>
  );
}