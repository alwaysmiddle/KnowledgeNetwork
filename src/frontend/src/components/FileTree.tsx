import { useEffect } from 'react';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { selectFile, initializeFolders } from '../store/fileSystemSlice';
import { type FileNode } from '../types/fileSystem';
import { FileTreeNode } from './FileTreeNode';

interface FileTreeProps {
  data: FileNode;
}

export function FileTree({ data }: FileTreeProps) {
  const dispatch = useAppDispatch();
  const selectedFileId = useAppSelector((state) => state.fileSystem.selectedFileId);

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

  const handleFileSelect = (node: FileNode) => {
    dispatch(selectFile(node.id));
    console.log('Selected file:', node.name, 'ID:', node.id);
  };

  return (
    <div className="text-sm">
      <FileTreeNode 
        node={data} 
        selectedFileId={selectedFileId}
        onFileSelect={handleFileSelect}
      />
    </div>
  );
}