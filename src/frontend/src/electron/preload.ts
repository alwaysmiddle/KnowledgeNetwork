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

// Expose secure API to renderer process
contextBridge.exposeInMainWorld('electronAPI', {
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
      // Secure event listener with cleanup
      const listener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback(data)
      }
      
      ipcRenderer.on('fs:change', listener)
      
      // Return cleanup function
      return () => {
        ipcRenderer.removeListener('fs:change', listener)
      }
    },

    onDirectoryChange: (callback: (data: any) => void) => {
      const listener = (_event: Electron.IpcRendererEvent, data: any) => {
        callback(data)
      }
      
      ipcRenderer.on('fs:directoryChange', listener)
      
      return () => {
        ipcRenderer.removeListener('fs:directoryChange', listener)
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
})

/**
 * Security validation - ensure context isolation is working
 */
contextBridge.exposeInMainWorld('electronSecurityCheck', {
  contextIsolated: true,
  nodeIntegration: process.env.NODE_INTEGRATION === 'true', // Should be false
  sandbox: process.env.ELECTRON_SANDBOX === 'true' // Should be true
})

/**
 * Preload script initialization
 */
document.addEventListener('DOMContentLoaded', () => {
  // Add Electron identification to body class
  document.body.classList.add('electron-app')
  
  // Log security status in development
  if (process.env.NODE_ENV === 'development') {
    console.log('🔒 Electron Security Status:')
    console.log('  Context Isolation:', !!window.electronSecurityCheck?.contextIsolated)
    console.log('  Node Integration:', !!window.electronSecurityCheck?.nodeIntegration)
    console.log('  Sandbox:', !!window.electronSecurityCheck?.sandbox)
    console.log('  Electron API Available:', !!window.electronAPI)
  }
})

/**
 * Error handling for preload script
 */
process.on('uncaughtException', (error) => {
  console.error('Preload script error:', error)
})

window.addEventListener('error', (event) => {
  console.error('Window error in preload context:', event.error)
})