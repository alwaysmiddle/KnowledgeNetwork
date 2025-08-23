import { type FileNode } from '../types/fileSystem';
import { FileIcon } from './FileIcon';

interface FileTreeNodeProps {
  node: FileNode;
  depth?: number;
}

export function FileTreeNode({ node, depth = 0 }: FileTreeNodeProps) {
  const indentWidth = depth * 20; // 20px per level

  return (
    <div>
      {/* Current Node */}
      <div 
        className="flex items-center gap-2 px-2 py-1 hover:bg-gray-700 cursor-pointer text-sm"
        style={{ paddingLeft: `${8 + indentWidth}px` }}
      >
        <FileIcon 
          type={node.type} 
          extension={node.extension}
          size={16}
        />
        <span className="text-gray-300 truncate">{node.name}</span>
      </div>

      {/* Children (always shown for now) */}
      {node.children && node.children.map(child => (
        <FileTreeNode 
          key={child.id} 
          node={child} 
          depth={depth + 1}
        />
      ))}
    </div>
  );
}