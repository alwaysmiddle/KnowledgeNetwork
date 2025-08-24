import type { AppDispatch } from '../store'
import { 
  setRootDirectory, 
  setWatchingDirectory, 
  setLoadingFileSystem, 
  setFileSystemError,
  addFileNode,
  removeFileNode,
  updateFileNode
} from '../store/fileSystemSlice'
import type { IFileSystemWatcher, FileSystemWatcherOptions } from './IFileSystemWatcher'

/**
 * Electron File System Watcher
 * Secure bridge between Electron main process and React renderer
 * Uses contextBridge API for secure IPC communication
 */
export class ElectronFileSystemWatcher implements IFileSystemWatcher {
  private dispatch: AppDispatch | null = null
  private watchPath: string | null = null
  private cleanupFunctions: Array<() => void> = []
  // Options stored for potential future use
  //private _options: FileSystemWatcherOptions

  constructor(dispatch: AppDispatch, _options?: FileSystemWatcherOptions) {
    this.dispatch = dispatch
    // Options are handled by the Electron main process, not needed here
    this.setupEventListeners()
  }

  /**
   * Check if we're running in Electron context
   */
  public static isElectronContext(): boolean {
    return typeof window !== 'undefined' && 
           typeof window.electronAPI !== 'undefined' &&
           window.electronAPI!.app.isElectron()
  }

  /**
   * Start watching a directory for changes
   */
  async startWatching(dirPath: string): Promise<void> {
    if (!ElectronFileSystemWatcher.isElectronContext()) {
      throw new Error('Not running in Electron context')
    }

    try {
      // Cleanup previous watchers
      this.cleanup()

      this.dispatch!(setLoadingFileSystem(true))
      this.dispatch!(setFileSystemError(null))

      // Start watching via secure IPC
      const result = await window.electronAPI!.fileSystem.startWatching(dirPath)
      
      if (result.error) {
        throw new Error(result.error)
      }

      if (result.success && result.rootNode) {
        // Update Redux state with initial directory structure
        this.dispatch!(setRootDirectory(result.rootNode))
        this.dispatch!(setWatchingDirectory(dirPath))
        this.watchPath = dirPath

        console.log(`🔍 Started watching: ${dirPath}`)
      }

    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      console.error('❌ Failed to start watching:', errorMessage)
      this.dispatch!(setFileSystemError(errorMessage))
    } finally {
      this.dispatch!(setLoadingFileSystem(false))
    }
  }

  /**
   * Stop watching the current directory
   */
  async stopWatching(): Promise<void> {
    if (!ElectronFileSystemWatcher.isElectronContext()) {
      return
    }

    try {
      await window.electronAPI!.fileSystem.stopWatching()
      this.cleanup()
      console.log('🛑 Stopped file system watching')
    } catch (error) {
      console.error('Failed to stop watching:', error)
    }
  }

  /**
   * Get the currently watched directory
   */
  getWatchedDirectory(): string | null {
    return this.watchPath
  }

  /**
   * Check if currently watching a directory
   */
  isWatching(): boolean {
    return this.watchPath !== null
  }

  /**
   * Set up secure event listeners for file system changes
   */
  private setupEventListeners(): void {
    if (!ElectronFileSystemWatcher.isElectronContext()) {
      return
    }

    // Listen for file additions
    const fileAddedCleanup = window.electronAPI!.fileSystem.onFileChange((data) => {
      if (data.type === 'add' && this.dispatch) {
        console.log(`📄 File added: ${data.node?.name}`)
        this.dispatch(addFileNode({ 
          parentPath: data.parentPath, 
          node: data.node 
        }))
      }
    })
    this.cleanupFunctions.push(fileAddedCleanup)

    // Listen for file changes
    const fileChangedCleanup = window.electronAPI!.fileSystem.onFileChange((data) => {
      if (data.type === 'change' && this.dispatch) {
        console.log(`📝 File changed: ${data.node?.name}`)
        this.dispatch(updateFileNode(data.node))
      }
    })
    this.cleanupFunctions.push(fileChangedCleanup)

    // Listen for file removals
    const fileRemovedCleanup = window.electronAPI!.fileSystem.onFileChange((data) => {
      if (data.type === 'unlink' && this.dispatch) {
        console.log(`🗑️ File removed: ${data.path}`)
        this.dispatch(removeFileNode(data.path))
      }
    })
    this.cleanupFunctions.push(fileRemovedCleanup)

    // Listen for directory changes
    const dirChangedCleanup = window.electronAPI!.fileSystem.onDirectoryChange((data) => {
      if (!this.dispatch) return

      switch (data.type) {
        case 'addDir':
          console.log(`📁 Directory added: ${data.node?.name}`)
          this.dispatch(addFileNode({ 
            parentPath: data.parentPath, 
            node: data.node 
          }))
          break
        
        case 'unlinkDir':
          console.log(`📂 Directory removed: ${data.path}`)
          this.dispatch(removeFileNode(data.path))
          break
      }
    })
    this.cleanupFunctions.push(dirChangedCleanup)

    // Listen for watcher errors
    const errorCleanup = window.electronAPI!.fileSystem.onFileChange((data) => {
      if (data.error && this.dispatch) {
        console.error('❌ File watcher error:', data.error)
        this.dispatch(setFileSystemError(data.error))
      }
    })
    this.cleanupFunctions.push(errorCleanup)
  }

  /**
   * Cleanup event listeners
   */
  private cleanup(): void {
    this.cleanupFunctions.forEach(cleanup => cleanup())
    this.cleanupFunctions = []
    this.watchPath = null
    
    if (this.dispatch) {
      this.dispatch(setWatchingDirectory(null))
    }
  }

  /**
   * Cleanup on instance destruction
   */
  public destroy(): void {
    this.cleanup()
    this.dispatch = null
  }
}

/**
 * Singleton instance for global file system watching
 */
let globalElectronWatcher: ElectronFileSystemWatcher | null = null

/**
 * Get or create the global Electron file system watcher instance
 */
export const getElectronFileSystemWatcher = (dispatch: AppDispatch): ElectronFileSystemWatcher => {
  if (!globalElectronWatcher) {
    globalElectronWatcher = new ElectronFileSystemWatcher(dispatch)
  }
  return globalElectronWatcher
}

/**
 * Cleanup function to properly close watchers
 */
export const cleanupElectronFileSystemWatcher = async (): Promise<void> => {
  if (globalElectronWatcher) {
    await globalElectronWatcher.stopWatching()
    globalElectronWatcher.destroy()
    globalElectronWatcher = null
  }
}