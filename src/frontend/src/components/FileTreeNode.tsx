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

// Helper function to highlight search text in file names
function highlightSearchText(text: string, query: string) {
  if (!query.trim()) return text;
  
  const parts = text.split(new RegExp(`(${query})`, 'gi'));
  return parts.map((part, i) => 
    part.toLowerCase() === query.toLowerCase() ? (
      <mark key={i} className="bg-yellow-400 text-black px-1 rounded-sm">{part}</mark>
    ) : (
      part
    )
  );
}

export function FileTreeNode({ 
  node, 
  depth = 0, 
  selectedFileId,
  onFileSelect 
}: FileTreeNodeProps) {
  const dispatch = useAppDispatch();
  const expandedFolders = useAppSelector((state) => state.fileSystem.expandedFolders);
  const searchQuery = useAppSelector((state) => state.fileSystem.searchQuery);
  const searchResults = useAppSelector((state) => state.fileSystem.searchResults);
  
  // Check if folder is expanded (default to true for initial state)
  const isExpanded = expandedFolders[node.id] !== undefined ? expandedFolders[node.id] : true;
  const indentWidth = depth * 8; // 8px per level (VS Code-style)
  const isSelected = selectedFileId === node.id;
  const isSearchMatch = searchResults.includes(node.id);
  const hasSearchQuery = searchQuery.trim().length > 0;

  // Helper function to check if node has child matches
  const hasChildMatches = (checkNode: FileNode): boolean => {
    if (!checkNode.children) return false;
    
    return checkNode.children.some(child => 
      searchResults.includes(child.id) || hasChildMatches(child)
    );
  };

  // Hide nodes that don't match search (and their parents don't contain matches)
  const shouldShow = !hasSearchQuery || isSearchMatch || (node.children && hasChildMatches(node));

  if (!shouldShow) return null;

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
            // Search matches get yellow background
            "bg-yellow-600/20 hover:bg-yellow-600/30": !isSelected && isSearchMatch && hasSearchQuery,
            // Folders and unselected files get hover effect
            "hover:bg-gray-700": !isSelected && (!isSearchMatch || !hasSearchQuery)
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
              "text-gray-300": !isSelected && (!isSearchMatch || !hasSearchQuery),
              "text-white font-medium": isSelected && node.type === 'file',
              "text-yellow-200 font-medium": !isSelected && isSearchMatch && hasSearchQuery
            }
          )}
          title={node.name}
        >
          {hasSearchQuery ? highlightSearchText(node.name, searchQuery) : node.name}
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