import { useState } from 'react';
import { type FileNode } from '../types/fileSystem';
import { FileTreeNode } from './FileTreeNode';

interface FileTreeProps {
  data: FileNode;
}

export function FileTree({ data }: FileTreeProps) {
  const [selectedFileId, setSelectedFileId] = useState<string | undefined>();

  const handleFileSelect = (node: FileNode) => {
    setSelectedFileId(node.id);
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