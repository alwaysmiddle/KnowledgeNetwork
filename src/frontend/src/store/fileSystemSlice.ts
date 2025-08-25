import { createSlice, type PayloadAction } from '@reduxjs/toolkit';
import { type FileNode } from '../types/fileSystem';

interface FileActivity {
  id: string;
  type: 'added' | 'modified' | 'removed';
  fileName: string;
  filePath: string;
  timestamp: string;
}

interface FileSystemState {
  selectedFileId: string | undefined;
  expandedFolders: Record<string, boolean>;
  codeViewerVisible: boolean;
  searchQuery: string;
  searchResults: string[]; // File IDs that match search
  // File system data
  rootDirectory: FileNode | null;
  watchingDirectory: string | null;
  isLoading: boolean;
  error: string | null;
  // Visual feedback for file changes
  recentActivity: FileActivity[];
  changedFiles: Record<string, { type: 'added' | 'modified' | 'removed'; timestamp: string }>; // File path -> change info
  lastActivityTime: string | null;
}

const initialState: FileSystemState = {
  selectedFileId: undefined,
  expandedFolders: {},
  codeViewerVisible: true, // Show code viewer by default
  searchQuery: '',
  searchResults: [],
  // File system data
  rootDirectory: null,
  watchingDirectory: null,
  isLoading: false,
  error: null,
  // Visual feedback for file changes
  recentActivity: [],
  changedFiles: {},
  lastActivityTime: null,
};

const fileSystemSlice = createSlice({
  name: 'fileSystem',
  initialState,
  reducers: {
    selectFile: (state, action: PayloadAction<string>) => {
      state.selectedFileId = action.payload;
    },
    clearSelection: (state) => {
      state.selectedFileId = undefined;
    },
    toggleFolder: (state, action: PayloadAction<string>) => {
      const folderId = action.payload;
      state.expandedFolders[folderId] = !state.expandedFolders[folderId];
    },
    setFolderExpanded: (state, action: PayloadAction<{ folderId: string; expanded: boolean }>) => {
      state.expandedFolders[action.payload.folderId] = action.payload.expanded;
    },
    initializeFolders: (state, action: PayloadAction<string[]>) => {
      action.payload.forEach(folderId => {
        if (state.expandedFolders[folderId] === undefined) {
          state.expandedFolders[folderId] = true; // Default expanded
        }
      });
    },
    toggleCodeViewer: (state) => {
      state.codeViewerVisible = !state.codeViewerVisible;
    },
    setCodeViewerVisible: (state, action: PayloadAction<boolean>) => {
      state.codeViewerVisible = action.payload;
    },
    setSearchQuery: (state, action: PayloadAction<string>) => {
      state.searchQuery = action.payload;
    },
    setSearchResults: (state, action: PayloadAction<string[]>) => {
      state.searchResults = action.payload;
    },
    clearSearch: (state) => {
      state.searchQuery = '';
      state.searchResults = [];
    },
    resetFileSystem: (state) => {
      state.selectedFileId = undefined;
      state.expandedFolders = {};
    },
    // Real file system actions
    setLoadingFileSystem: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload;
      if (action.payload) {
        state.error = null;
      }
    },
    setFileSystemError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
      state.isLoading = false;
    },
    setRootDirectory: (state, action: PayloadAction<FileNode>) => {
      state.rootDirectory = action.payload;
      state.isLoading = false;
      state.error = null;
    },
    setWatchingDirectory: (state, action: PayloadAction<string | null>) => {
      state.watchingDirectory = action.payload;
    },
    // Real-time file system update actions with activity tracking
    addFileNode: (state, action: PayloadAction<{ parentPath: string; node: FileNode }>) => {
      if (state.rootDirectory) {
        const { parentPath, node } = action.payload;
        const parentNode = findNodeByPath(state.rootDirectory, parentPath);
        if (parentNode && parentNode.children) {
          parentNode.children.push(node);
          // Sort children after adding
          sortFileNodes(parentNode.children);
          
          // Track activity
          addActivity(state, {
            id: `add-${Date.now()}-${Math.random().toString(36).substr(2, 9)}-${node.path}`,
            type: 'added',
            fileName: node.name,
            filePath: node.path,
            timestamp: new Date().toISOString()
          });
          
          // Mark file as recently changed
          state.changedFiles[node.path] = {
            type: 'added',
            timestamp: new Date().toISOString()
          };
        }
      }
    },
    removeFileNode: (state, action: PayloadAction<string>) => {
      if (state.rootDirectory) {
        const filePath = action.payload;
        const removedNode = findNodeByPath(state.rootDirectory, filePath);
        
        if (removedNode && removeNodeByPath(state.rootDirectory, filePath)) {
          // Track activity
          addActivity(state, {
            id: `remove-${Date.now()}-${Math.random().toString(36).substr(2, 9)}-${removedNode.path}`,
            type: 'removed',
            fileName: removedNode.name,
            filePath: removedNode.path,
            timestamp: new Date().toISOString()
          });
          
          // Mark file as recently changed
          state.changedFiles[removedNode.path] = {
            type: 'removed',
            timestamp: new Date().toISOString()
          };
        }
      }
    },
    updateFileNode: (state, action: PayloadAction<FileNode>) => {
      if (state.rootDirectory) {
        const updatedNode = action.payload;
        if (updateNodeByPath(state.rootDirectory, updatedNode)) {
          // Track activity
          addActivity(state, {
            id: `update-${Date.now()}-${Math.random().toString(36).substr(2, 9)}-${updatedNode.path}`,
            type: 'modified',
            fileName: updatedNode.name,
            filePath: updatedNode.path,
            timestamp: new Date().toISOString()
          });
          
          // Mark file as recently changed
          state.changedFiles[updatedNode.path] = {
            type: 'modified',
            timestamp: new Date().toISOString()
          };
        }
      }
    },
    // Visual feedback actions
    clearRecentActivity: (state) => {
      state.recentActivity = [];
    },
    clearChangedFileIndicators: (state) => {
      state.changedFiles = {};
    },
    removeOldChangeIndicators: (state, action: PayloadAction<number>) => {
      const maxAge = action.payload; // milliseconds
      const cutoffTime = Date.now() - maxAge;
      
      Object.keys(state.changedFiles).forEach(filePath => {
        const changeInfo = state.changedFiles[filePath];
        if (new Date(changeInfo.timestamp).getTime() < cutoffTime) {
          delete state.changedFiles[filePath];
        }
      });
    },
    removeChangeIndicatorByPath: (state, action: PayloadAction<string>) => {
      const filePath = action.payload;
      if (state.changedFiles[filePath]) {
        delete state.changedFiles[filePath];
      }
    },
  },
});

export const { 
  selectFile, 
  clearSelection, 
  toggleFolder, 
  setFolderExpanded, 
  initializeFolders,
  toggleCodeViewer,
  setCodeViewerVisible,
  setSearchQuery,
  setSearchResults,
  clearSearch,
  resetFileSystem,
  // File system actions
  setLoadingFileSystem,
  setFileSystemError,
  setRootDirectory,
  setWatchingDirectory,
  addFileNode,
  removeFileNode,
  updateFileNode,
  // Visual feedback actions
  clearRecentActivity,
  clearChangedFileIndicators,
  removeOldChangeIndicators,
  removeChangeIndicatorByPath
} = fileSystemSlice.actions;

// Helper functions for file system tree manipulation
function findNodeByPath(root: FileNode, targetPath: string): FileNode | null {
  if (root.path === targetPath) {
    return root;
  }
  
  if (root.children) {
    for (const child of root.children) {
      const found = findNodeByPath(child, targetPath);
      if (found) return found;
    }
  }
  
  return null;
}

function removeNodeByPath(root: FileNode, targetPath: string): boolean {
  if (root.children) {
    const index = root.children.findIndex(child => child.path === targetPath);
    if (index !== -1) {
      root.children.splice(index, 1);
      return true;
    }
    
    // Recursively search in children
    for (const child of root.children) {
      if (removeNodeByPath(child, targetPath)) {
        return true;
      }
    }
  }
  
  return false;
}

function updateNodeByPath(root: FileNode, updatedNode: FileNode): boolean {
  if (root.path === updatedNode.path) {
    // Update properties but preserve children structure if it's a directory
    Object.assign(root, { ...updatedNode, children: root.children });
    return true;
  }
  
  if (root.children) {
    for (const child of root.children) {
      if (updateNodeByPath(child, updatedNode)) {
        return true;
      }
    }
  }
 
  return false;
}

function sortFileNodes(nodes: FileNode[]): void {
  nodes.sort((a, b) => {
    // Folders first, then files
    if (a.type === b.type) {
      return a.name.localeCompare(b.name, undefined, { numeric: true });
    }
    return a.type === 'folder' ? -1 : 1;
  });
}

// Helper function to add activity with size limit
function addActivity(state: FileSystemState, activity: FileActivity): void {
  // Check if activity with same ID already exists before adding
  if (state.recentActivity.some(a => a.id === activity.id)) return;
  state.recentActivity.unshift(activity); // Add to beginning
  state.lastActivityTime = activity.timestamp;
  
  // Keep only last 20 activities to prevent memory growth
  if (state.recentActivity.length > 20) {
    state.recentActivity = state.recentActivity.slice(0, 20);
  }
}

// Note: Cleanup scheduling is handled by visualIndicatorCleanup service
// This keeps Redux actions pure and allows proper dispatch handling

// Selectors
export const selectFileSystemData = (state: { fileSystem: FileSystemState }) => {
  return state.fileSystem.rootDirectory;
};

export default fileSystemSlice.reducer;