import { type FileNode } from '../types/fileSystem';
import { FileTreeNode } from './FileTreeNode';

interface FileTreeProps {
  data: FileNode;
}

export function FileTree({ data }: FileTreeProps) {
  return (
    <div className="text-sm">
      <FileTreeNode node={data} />
    </div>
  );
}