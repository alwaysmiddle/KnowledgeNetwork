import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface FileSystemState {
  selectedFileId: string | undefined;
  expandedFolders: Record<string, boolean>;
  codeViewerVisible: boolean;
  searchQuery: string;
  searchResults: string[]; // File IDs that match search
}

const initialState: FileSystemState = {
  selectedFileId: undefined,
  expandedFolders: {},
  codeViewerVisible: true, // Show code viewer by default
  searchQuery: '',
  searchResults: [],
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
  resetFileSystem 
} = fileSystemSlice.actions;

export default fileSystemSlice.reducer;