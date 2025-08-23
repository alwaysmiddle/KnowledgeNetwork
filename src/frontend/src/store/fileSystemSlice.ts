import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

interface FileSystemState {
  selectedFileId: string | undefined;
  expandedFolders: Record<string, boolean>;
  codeViewerVisible: boolean;
}

const initialState: FileSystemState = {
  selectedFileId: undefined,
  expandedFolders: {},
  codeViewerVisible: true, // Show code viewer by default
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
  resetFileSystem 
} = fileSystemSlice.actions;

export default fileSystemSlice.reducer;