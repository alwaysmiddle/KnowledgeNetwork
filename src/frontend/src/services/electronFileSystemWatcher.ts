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
import { scheduleIndicatorCleanup, clearAllIndicatorCleanups } from './visualIndicatorCleanup'

/**
 * Electron File System Watcher
 * Secure bridge between Electron main process and React renderer
 * Uses contextBridge API for secure IPC communication
 */
export class ElectronFileSystemWatcher implements IFileSystemWatcher {
  private dispatch: AppDispatch | null = null
  private watchPath: string | null = null
  private cleanupFunctions: Array<() => void> = []

  constructor(dispatch: AppDispatch, _options?: FileSystemWatcherOptions) {
    this.dispatch = dispatch
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
      // Clear all visual indicator cleanup timers
      clearAllIndicatorCleanups()
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

    // Single unified event listener to handle all file system changes
    const fileSystemChangeCleanup = window.electronAPI!.fileSystem.onFileChange((data) => {
      if (!this.dispatch) return

      try {
        switch (data.type) {
          case 'add':
            if (data.node && data.parentPath) {
              console.log(`📄 File added: ${data.node.name}`)
              this.dispatch(addFileNode({ 
                parentPath: data.parentPath, 
                node: data.node 
              }))
              scheduleIndicatorCleanup(this.dispatch, data.node.path)
            }
            break

          case 'change':
            if (data.node) {
              console.log(`📝 File changed: ${data.node.name}`)
              this.dispatch(updateFileNode(data.node))
              scheduleIndicatorCleanup(this.dispatch, data.node.path)
            }
            break

          case 'unlink':
            if (data.path) {
              const fileName = data.path.split(/[/\\]/).pop() || 'unknown file'
              console.log(`🗑️ File removed: ${fileName}`)
              this.dispatch(removeFileNode(data.path))
              scheduleIndicatorCleanup(this.dispatch, data.path)
            }
            break

          case 'addDir':
            if (data.node && data.parentPath) {
              console.log(`📁 Directory added: ${data.node.name}`)
              this.dispatch(addFileNode({ 
                parentPath: data.parentPath, 
                node: data.node 
              }))
              scheduleIndicatorCleanup(this.dispatch, data.node.path)
            }
            break

          case 'unlinkDir':
            if (data.path) {
              const dirName = data.path.split(/[/\\]/).pop() || 'unknown directory'
              console.log(`📂 Directory removed: ${dirName}`)
              this.dispatch(removeFileNode(data.path))
              scheduleIndicatorCleanup(this.dispatch, data.path)
            }
            break

          case 'ready':
            console.log(`✅ File system watcher ready for: ${data.path}`)
            break

          case 'error':
            if (data.error) {
              console.error('❌ File watcher error:', data.error)
              this.dispatch(setFileSystemError(data.error))
            }
            break

          default:
            console.log(`🔍 Unhandled event type: ${data.type}`)
        }
      } catch (error) {
        console.error('❌ Error processing file system event:', error)
        this.dispatch(setFileSystemError(`Failed to process file system event: ${error instanceof Error ? error.message : 'Unknown error'}`))
      }
    })
    this.cleanupFunctions.push(fileSystemChangeCleanup)

    // Also listen for directory-specific events if they're separate
    const directoryChangeCleanup = window.electronAPI!.fileSystem.onDirectoryChange?.((data) => {
      console.log('📁 Directory-specific event received:', data)
      // This might be redundant if all events come through onFileChange
      // Will be cleaned up if not needed
    })
    if (directoryChangeCleanup) {
      this.cleanupFunctions.push(directoryChangeCleanup)
    }
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