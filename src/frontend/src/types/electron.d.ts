/**
 * TypeScript definitions for Electron API exposed to renderer process
 */

interface ElectronAPI {
  selectDirectory(): Promise<{
    canceled: boolean
    filePaths?: string[]
    error?: string
  }>
  
  fileSystem: {
    startWatching(dirPath: string): Promise<{
      success: boolean
      path?: string
      rootNode?: any
      error?: string
    }>
    
    stopWatching(): Promise<{
      success: boolean
      error?: string
    }>
    
    readFile(filePath: string): Promise<{
      success?: boolean
      content?: string
      size?: number
      modified?: string
      error?: string
    }>
    
    onFileChange(callback: (data: any) => void): () => void
    onDirectoryChange(callback: (data: any) => void): () => void
  }
  
  app: {
    getVersion(): string
    isElectron(): boolean
    platform(): string
  }
  
  dev: {
    openDevTools(): void
    log(message: string, level?: string): void
  }
}

interface ElectronSecurityCheck {
  contextIsolated: boolean
  nodeIntegration: boolean
  sandbox: boolean
}

// Extend the global Window interface
declare global {
  interface Window {
    electronAPI?: ElectronAPI
    electronSecurityCheck?: ElectronSecurityCheck
  }
}

// Ensure this file is treated as a module
export {}