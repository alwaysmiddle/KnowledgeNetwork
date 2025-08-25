const { contextBridge, ipcRenderer } = require('electron')

/**
 * Type definitions for the Electron API exposed to renderer
 */
interface ElectronAPI {
  selectDirectory: () => Promise<{ canceled: boolean; filePaths?: string[]; error?: string }>
  fileSystem: {
    startWatching: (dirPath: string) => Promise<{ success: boolean; path?: string; rootNode?: any; error?: string }>
    stopWatching: () => Promise<{ success: boolean; error?: string }>
    readFile: (filePath: string) => Promise<{ success?: boolean; content?: string; size?: number; modified?: string; error?: string }>
    onFileChange: (callback: (data: any) => void) => () => void
    onDirectoryChange: (callback: (data: any) => void) => () => void
  }
  app: {
    getVersion: () => string
    isElectron: () => boolean
    platform: () => string
  }
  dev: {
    openDevTools: () => void
    log: (message: string, level?: string) => void
  }
}

// Extend window interface for TypeScript (only if not already declared)
declare global {
  interface Window {
    electronAPI?: ElectronAPI
    electronSecurityCheck?: {
      contextIsolated: boolean
      nodeIntegration: boolean
      sandbox: boolean
    }
  }
}

/**
 * Secure preload script with contextBridge API
 * Following 2024-2025 Electron security best practices
 * 
 * IMPORTANT: Only expose minimal, purpose-specific APIs
 * Never expose raw ipcRenderer or Node.js APIs to renderer
 */

// Create the API object first
const electronAPI = {
  /**
   * Directory Selection
   */
  selectDirectory: async () => {
    try {
      return await ipcRenderer.invoke('dialog:openDirectory')
    } catch (error) {
      console.error('Directory selection error:', error)
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { error: errorMessage }
    }
  },

  /**
   * File System Watching
   */
  fileSystem: {
    startWatching: async (dirPath: string) => {
      try {
        return await ipcRenderer.invoke('fs:startWatching', dirPath)
      } catch (error) {
        console.error('Start watching error:', error)
        const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { error: errorMessage }
      }
    },

    stopWatching: async () => {
      try {
        return await ipcRenderer.invoke('fs:stopWatching')
      } catch (error) {
        console.error('Stop watching error:', error)
        const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { error: errorMessage }
      }
    },

    readFile: async (filePath: string) => {
      try {
        return await ipcRenderer.invoke('fs:readFile', filePath)
      } catch (error) {
        console.error('Read file error:', error)
        const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { error: errorMessage }
      }
    },

    // Event listeners for file system changes
    onFileChange: (callback: (data: any) => void) => {
      const listeners: Array<() => void> = []
      
      // File events
      const fileAddedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'add' })
      }
      const fileChangedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'change' })
      }
      const fileRemovedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'unlink' })
      }
      
      // Directory events  
      const dirAddedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'addDir' })
      }
      const dirRemovedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'unlinkDir' })
      }
      
      // Error events
      const errorListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'error' })
      }
      
      // Ready events
      const readyListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'ready' })
      }

      // Register all listeners
      ipcRenderer.on('fs:fileAdded', fileAddedListener)
      ipcRenderer.on('fs:fileChanged', fileChangedListener)
      ipcRenderer.on('fs:fileRemoved', fileRemovedListener)
      ipcRenderer.on('fs:directoryAdded', dirAddedListener)
      ipcRenderer.on('fs:directoryRemoved', dirRemovedListener)
      ipcRenderer.on('fs:error', errorListener)
      ipcRenderer.on('fs:ready', readyListener)
      
      // Return unified cleanup function
      return () => {
        ipcRenderer.removeListener('fs:fileAdded', fileAddedListener)
        ipcRenderer.removeListener('fs:fileChanged', fileChangedListener)
        ipcRenderer.removeListener('fs:fileRemoved', fileRemovedListener)
        ipcRenderer.removeListener('fs:directoryAdded', dirAddedListener)
        ipcRenderer.removeListener('fs:directoryRemoved', dirRemovedListener)
        ipcRenderer.removeListener('fs:error', errorListener)
        ipcRenderer.removeListener('fs:ready', readyListener)
      }
    },

    onDirectoryChange: (callback: (data: any) => void) => {
      // Directory-specific events (subset of onFileChange)
      const dirAddedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'addDir' })
      }
      const dirRemovedListener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback({ ...data, type: 'unlinkDir' })
      }

      ipcRenderer.on('fs:directoryAdded', dirAddedListener)
      ipcRenderer.on('fs:directoryRemoved', dirRemovedListener)
      
      return () => {
        ipcRenderer.removeListener('fs:directoryAdded', dirAddedListener)
        ipcRenderer.removeListener('fs:directoryRemoved', dirRemovedListener)
      }
    }
  },

  /**
   * Application Information
   */
  app: {
    getVersion: () => {
      return process.env.npm_package_version || '1.0.0'
    },
    
    isElectron: () => {
      return true
    },

    platform: () => {
      return process.platform
    }
  },

  /**
   * Development Utilities
   */
  dev: {
    openDevTools: () => {
      if (process.env.NODE_ENV === 'development') {
        ipcRenderer.send('dev:openDevTools')
      }
    },

    log: (message: string, level: string = 'info') => {
      if (process.env.NODE_ENV === 'development') {
        console.log(`[Electron ${level.toUpperCase()}]:`, message)
      }
    }
  }
}

// PROPER CONTEXTBRIDGE PATTERN - Check contextIsolated first
console.log('🔒 process object available:', !!process)
console.log('🔒 process.contextIsolated:', process.contextIsolated)
console.log('🔒 process.contextIsolated type:', typeof process.contextIsolated)

// RESEARCH-BASED FIX: Execute contextBridge immediately, no async wrappers
if (process.contextIsolated) {
  console.log('🔒 Entering contextIsolated branch')
  try {
    contextBridge.exposeInMainWorld('electronAPI', electronAPI)
    console.log('🔒 contextBridge.exposeInMainWorld succeeded')
  } catch (error) {
    console.error('🔒 contextBridge.exposeInMainWorld failed:', error)
    // Fallback: assign directly to window
    window.electronAPI = electronAPI
    console.log('🔒 Using fallback: assigned directly to window')
  }
} else {
  // Fallback when context isolation is disabled
  console.log('🔒 Using fallback branch: process.contextIsolated is', process.contextIsolated)
  window.electronAPI = electronAPI
  console.log('🔒 Direct assignment completed')
}

/**
 * Security validation - ensure context isolation is working
 */
const securityCheck = {
  contextIsolated: process.contextIsolated, // FIXED - use actual context isolation status
  nodeIntegration: typeof require !== 'undefined', // Should be false
  sandbox: process.env.ELECTRON_SANDBOX === 'true', // Should be true
  processType: process.type,
  electronVersion: process.versions.electron,
  contextBridgeAvailable: !!contextBridge
}

if (process.contextIsolated) {
  try {
    contextBridge.exposeInMainWorld('electronSecurityCheck', securityCheck)
  } catch (error) {
    console.error('🔒 Failed to expose security check:', error)
    window.electronSecurityCheck = securityCheck
  }
} else {
  window.electronSecurityCheck = securityCheck
}

/**
 * Preload script initialization
 */
console.log('🔒 Preload script is loading...')

// Note: Security status and electronAPI availability will be tested 
// from the renderer context (React App), not here in the preload script
// This follows proper contextBridge patterns where preload exposes APIs
// and renderer consumes them.

// Add immediate logging
console.log('🔒 Preload script executed, contextBridge available:', !!contextBridge)

/**
 * Error handling for preload script
 */
process.on('uncaughtException', (error) => {
  console.error('Preload script error:', error)
})

window.addEventListener('error', (event) => {
  console.error('Window error in preload context:', event.error)
})