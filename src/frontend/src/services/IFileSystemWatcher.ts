/**
 * Unified File System Watcher Interface
 * Provides clean abstraction for different file watching implementations
 */

import type { FileNode } from '../types/fileSystem'

/**
 * Common interface for all file system watchers
 */
export interface IFileSystemWatcher {
  /**
   * Start watching a directory for changes
   */
  startWatching(dirPath: string): Promise<void>
  
  /**
   * Stop watching the current directory
   */
  stopWatching(): Promise<void>
  
  /**
   * Get the currently watched directory path
   */
  getWatchedDirectory(): string | null
  
  /**
   * Check if currently watching a directory
   */
  isWatching(): boolean
  
  /**
   * Clean up resources when instance is destroyed
   */
  destroy?(): void
}

/**
 * File system watcher events for Redux integration
 */
export interface FileSystemWatcherEvents {
  onFileAdded?: (node: FileNode, parentPath: string) => void
  onFileChanged?: (node: FileNode) => void
  onFileRemoved?: (path: string) => void
  onDirectoryAdded?: (node: FileNode, parentPath: string) => void
  onDirectoryRemoved?: (path: string) => void
  onError?: (error: string) => void
  onReady?: (path: string) => void
}

/**
 * File system watcher configuration options
 */
export interface FileSystemWatcherOptions {
  // Directories and files to ignore
  ignoredPatterns?: (string | RegExp)[]
  
  // Maximum directory depth to scan
  maxDepth?: number
  
  // File stability thresholds for write operations
  stabilityThreshold?: number
  pollInterval?: number
  
  // Enable/disable specific features
  enableInitialScan?: boolean
  enableRealTimeUpdates?: boolean
}

/**
 * Factory function signature for creating watchers
 */
export type FileSystemWatcherFactory = (
  dispatch: any, 
  options?: FileSystemWatcherOptions
) => IFileSystemWatcher