/**
 * File System Watcher Factory
 * Provides clean abstraction for Electron-based file system watching
 */

import type { AppDispatch } from '../store'
import type { IFileSystemWatcher, FileSystemWatcherOptions } from './IFileSystemWatcher'
import { ElectronFileSystemWatcher, getElectronFileSystemWatcher } from './electronFileSystemWatcher'

/**
 * File system watcher type enumeration
 */
export const FileSystemWatcherType = {
  Electron: 'electron' as const
} as const

export type FileSystemWatcherType = typeof FileSystemWatcherType[keyof typeof FileSystemWatcherType]

/**
 * File system watcher context information
 */
export interface FileSystemWatcherContext {
  type: FileSystemWatcherType
  isElectron: boolean
  capabilities: {
    nativeDirectoryPicker: boolean
    realFileSystem: boolean
    secureIPC: boolean
  }
}

/**
 * Detect current file system watcher context
 */
export function detectFileSystemWatcherContext(): FileSystemWatcherContext {
  const isElectron = ElectronFileSystemWatcher.isElectronContext()
  
  if (!isElectron) {
    throw new Error('File system watcher requires Electron context')
  }
  
  return {
    type: FileSystemWatcherType.Electron,
    isElectron: true,
    capabilities: {
      nativeDirectoryPicker: true,
      realFileSystem: true,
      secureIPC: true
    }
  }
}

/**
 * Create Electron file system watcher
 */
export function createFileSystemWatcher(
  dispatch: AppDispatch,
  _options?: FileSystemWatcherOptions
): IFileSystemWatcher {
  
  if (!ElectronFileSystemWatcher.isElectronContext()) {
    throw new Error('File system watcher requires Electron context')
  }
  
  return getElectronFileSystemWatcher(dispatch)
}

/**
 * Get the singleton instance of the Electron file system watcher
 */
export function getFileSystemWatcher(
  dispatch: AppDispatch,
  options?: FileSystemWatcherOptions
): IFileSystemWatcher {
  return createFileSystemWatcher(dispatch, options)
}

/**
 * Check if native directory picker is available
 */
export function hasNativeDirectoryPicker(): boolean {
  return ElectronFileSystemWatcher.isElectronContext()
}

/**
 * Check if real file system access is available
 */
export function hasRealFileSystem(): boolean {
  return ElectronFileSystemWatcher.isElectronContext()
}

/**
 * Get user-friendly description of current file system capabilities
 */
export function getFileSystemCapabilitiesDescription(): string {
  if (ElectronFileSystemWatcher.isElectronContext()) {
    return "Real-time file system monitoring with native directory picker"
  }
  
  return "File system access not available - requires Electron context"
}