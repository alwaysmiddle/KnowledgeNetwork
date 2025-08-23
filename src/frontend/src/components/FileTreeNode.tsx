import { ChevronRight, ChevronDown } from 'lucide-react';
import clsx from 'clsx';
import { useAppSelector, useAppDispatch } from '../store/hooks';
import { toggleFolder } from '../store/fileSystemSlice';
import { type FileNode } from '../types/fileSystem';
import { FileIcon } from './FileIcon';

interface FileTreeNodeProps {
  node: FileNode;
  depth?: number;
  selectedFileId?: string;
  onFileSelect?: (node: FileNode) => void;
}

export function FileTreeNode({ 
  node, 
  depth = 0, 
  selectedFileId,
  onFileSelect 
}: FileTreeNodeProps) {
  const dispatch = useAppDispatch();
  const expandedFolders = useAppSelector((state) => state.fileSystem.expandedFolders);
  
  // Check if folder is expanded (default to true for initial state)
  const isExpanded = expandedFolders[node.id] !== undefined ? expandedFolders[node.id] : true;
  const indentWidth = depth * 8; // 8px per level (VS Code-style)
  const isSelected = selectedFileId === node.id;

  const handleClick = (e: React.MouseEvent) => {
    if (node.type === 'folder' && node.children && node.children.length > 0) {
      // Folder: toggle expand/collapse
      e.stopPropagation();
      dispatch(toggleFolder(node.id));
    } else if (node.type === 'file' && onFileSelect) {
      // File: select the file
      e.stopPropagation();
      onFileSelect(node);
    }
  };

  return (
    <div>
      {/* Current Node */}
      <div 
        className={clsx(
          "flex items-center gap-2 px-2 py-1 cursor-pointer text-sm min-w-0 transition-colors",
          {
            // Selected file gets blue background (VS Code style)
            "bg-blue-600/30 hover:bg-blue-600/40": isSelected && node.type === 'file',
            // Folders and unselected files get hover effect
            "hover:bg-gray-700": !isSelected || node.type === 'folder'
          }
        )}
        style={{ paddingLeft: `${8 + indentWidth}px` }}
        onClick={handleClick}
      >
        {/* Chevron Icon (only for folders with children) */}
        {node.type === 'folder' && node.children && node.children.length > 0 && (
          <div className="w-4 h-4 flex items-center justify-center">
            {isExpanded ? (
              <ChevronDown size={12} className="text-gray-400 hover:text-gray-200 transition-colors" />
            ) : (
              <ChevronRight size={12} className="text-gray-400 hover:text-gray-200 transition-colors" />
            )}
          </div>
        )}
        
        {/* Empty space for files or empty folders */}
        {!(node.type === 'folder' && node.children && node.children.length > 0) && (
          <div className="w-4" />
        )}
        
        <FileIcon 
          type={node.type} 
          extension={node.extension}
          size={16}
        />
        <span 
          className={clsx(
            "truncate flex-1 min-w-0 transition-colors",
            {
              "text-gray-300": !isSelected,
              "text-white font-medium": isSelected && node.type === 'file'
            }
          )}
          title={node.name}
        >
          {node.name}
        </span>
      </div>

      {/* Children (only if expanded) */}
      {node.children && isExpanded && node.children.map(child => (
        <FileTreeNode 
          key={child.id} 
          node={child} 
          depth={depth + 1}
          selectedFileId={selectedFileId}
          onFileSelect={onFileSelect}
        />
      ))}
    </div>
  );
}