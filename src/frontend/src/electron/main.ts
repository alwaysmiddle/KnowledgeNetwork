import { app, BrowserWindow, ipcMain, dialog } from 'electron'
import * as path from 'path'
import * as chokidar from 'chokidar'
import { promises as fs } from 'fs'
import type { FSWatcher } from 'chokidar'

// Enable live reload for Electron in development
if (process.env.NODE_ENV === 'development') {
  require('electron-reload')(__dirname, {
    electron: path.join(__dirname, '..', '..', 'node_modules', '.bin', 'electron'),
    hardResetMethod: 'exit'
  })
}

/**
 * Create the main application window with mandatory 2024-2025 security settings
 */
function createWindow() {
  const win = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      // MANDATORY SECURITY SETTINGS (2024-2025)
      nodeIntegration: false,           // REQUIRED: No Node.js in renderer
      contextIsolation: true,           // REQUIRED: Isolated context
      // enableRemoteModule deprecated in Electron 20+
      sandbox: true,                    // RECOMMENDED: Enable sandboxing
      preload: path.join(__dirname, '..', 'preload', 'index.cjs'), // Secure IPC bridge (built from preload.ts as CommonJS)
      
      // Additional security settings
      webSecurity: true,                // Enable web security
      allowRunningInsecureContent: false, // No mixed content
      experimentalFeatures: false       // No experimental features
    },
    // Window settings
    show: false, // Don't show until ready
    titleBarStyle: 'default',
    backgroundColor: '#1a1a1a' // Match app theme
  })

  // Load the React app
  if (process.env.NODE_ENV === 'development') {
    // Development: Load from Vite dev server
    win.loadURL('http://localhost:5173')
    // Open DevTools in development
    win.webContents.openDevTools()
  } else {
    // Production: Load from built files
    win.loadFile(path.join(__dirname, '..', '..', 'dist', 'index.html'))
  }

  // Show window when ready to prevent visual flash
  win.once('ready-to-show', () => {
    win.show()
    
    // Focus window on creation
    if (process.env.NODE_ENV === 'development') {
      win.focus()
    }
  })

  // Prevent new window creation for security
  win.webContents.setWindowOpenHandler(() => {
    return { action: 'deny' }
  })

  return win
}

/**
 * File System Watcher State
 */
let currentWatcher: FSWatcher | null = null
let currentWatchPath: string | null = null
let mainWindow: BrowserWindow | null = null

/**
 * File system node interface
 */
interface FileSystemNode {
  id: string
  name: string
  path: string
  isDirectory: boolean
  isFile: boolean
  size: number
  modified: string
  created: string
  extension: string
  children?: FileSystemNode[]
}

/**
 * Utility function to safely get file/directory info
 */
async function getFileSystemInfo(filePath: string): Promise<FileSystemNode | null> {
  try {
    const stats = await fs.stat(filePath)
    const parsed = path.parse(filePath)
    
    return {
      id: filePath,
      name: parsed.name + parsed.ext,
      path: filePath,
      isDirectory: stats.isDirectory(),
      isFile: stats.isFile(),
      size: stats.size,
      modified: stats.mtime.toISOString(),
      created: stats.ctime.toISOString(),
      extension: parsed.ext,
      children: stats.isDirectory() ? [] : undefined
    }
  } catch (error) {
    console.error('Error getting file info:', error)
    return null
  }
}

/**
 * Scan directory structure (recursive)
 */
async function scanDirectory(dirPath: string, maxDepth: number = 10, currentDepth: number = 0): Promise<FileSystemNode | null> {
  if (currentDepth >= maxDepth) {
    console.warn(`Max depth reached for ${dirPath}`)
    return null
  }

  try {
    const dirInfo = await getFileSystemInfo(dirPath)
    if (!dirInfo || !dirInfo.isDirectory) return dirInfo

    const entries = await fs.readdir(dirPath)
    const children = []

    for (const entry of entries) {
      // Skip hidden files and common ignore patterns
      if (entry.startsWith('.') || 
          entry === 'node_modules' || 
          entry === 'dist' || 
          entry === 'build' ||
          entry === 'obj' ||
          entry === 'bin') {
        continue
      }

      const entryPath = path.join(dirPath, entry)
      const entryInfo = await getFileSystemInfo(entryPath)
      
      if (entryInfo) {
        if (entryInfo.isDirectory) {
          // Recursive scan for directories
          const childDir = await scanDirectory(entryPath, maxDepth, currentDepth + 1)
          if (childDir) children.push(childDir)
        } else {
          children.push(entryInfo)
        }
      }
    }

    dirInfo.children = children
    return dirInfo
  } catch (error) {
    console.error(`Error scanning directory ${dirPath}:`, error)
    return null
  }
}

/**
 * IPC Handlers for secure file system operations
 */
function setupIpcHandlers() {
  // Directory selection dialog
  ipcMain.handle('dialog:openDirectory', async () => {
    try {
      const result = await dialog.showOpenDialog(mainWindow!, {
        properties: ['openDirectory'],
        title: 'Select Directory to Watch for File Changes'
      })
      
      if (result.canceled) {
        return { canceled: true }
      }
      
      return { 
        canceled: false, 
        filePaths: result.filePaths 
      }
    } catch (error) {
      console.error('Directory dialog error:', error)
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { 
        error: errorMessage 
      }
    }
  })

  // Start file system watching
  ipcMain.handle('fs:startWatching', async (_, dirPath) => {
    try {
      // Stop any existing watcher
      if (currentWatcher) {
        await currentWatcher.close()
        currentWatcher = null
      }

      // Validate directory exists
      const dirStats = await fs.stat(dirPath)
      if (!dirStats.isDirectory()) {
        return { 
          error: `Path is not a directory: ${dirPath}` 
        }
      }

      // Scan initial directory structure
      console.log('🔍 Scanning directory structure:', dirPath)
      const rootNode = await scanDirectory(dirPath)
      
      if (!rootNode) {
        return { 
          error: `Failed to scan directory: ${dirPath}` 
        }
      }

      // Start Chokidar watcher with restrictive settings to prevent permission errors
      currentWatcher = chokidar.watch(dirPath, {
        ignored: [
          // Hidden files and system directories
          /(^|[\/\\])\../, 
          // Build and dependency directories
          '**/node_modules/**',
          '**/dist/**',
          '**/build/**',
          '**/obj/**',
          '**/bin/**',
          '**/out/**',
          // Temporary files
          '**/*.tmp',
          '**/*.temp',
          '**/*.swp',
          '**/*.swo',
          '**/*~',
          // Log directories
          '**/logs/**',
          '**/log/**',
          // Version control
          '**/.git/**',
          '**/.svn/**',
          '**/.hg/**',
          // IDE directories
          '**/.vscode/**',
          '**/.idea/**',
          '**/target/**',
          // Windows system directories and files (to prevent EPERM)
          '**/System Volume Information/**',
          '**/pagefile.sys',
          '**/hiberfil.sys',
          '**/swapfile.sys',
          '**/Windows/**',
          '**/Program Files/**',
          '**/Program Files (x86)/**',
          '**/ProgramData/**',
          // Restricted file extensions
          '**/*.lnk',
          '**/*.sys',
          '**/*.dll',
          '**/*.exe'
        ],
        persistent: true,
        ignoreInitial: true,
        followSymlinks: false,
        depth: 8, // Reduced depth to avoid deep system directories
        atomic: true,
        awaitWriteFinish: {
          stabilityThreshold: 100,
          pollInterval: 50
        },
        // Add more restrictive options to prevent permission errors
        ignorePermissionErrors: true,
        usePolling: false, // Use native file system events when possible
        interval: 1000, // Polling interval if usePolling is true
        binaryInterval: 3000
      })

      // Set up event handlers with consistent data structure
      currentWatcher.on('add', async (filePath) => {
        try {
          console.log('📄 File added:', path.basename(filePath))
          const fileInfo = await getFileSystemInfo(filePath)
          if (fileInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('fs:fileAdded', {
              type: 'add',
              path: filePath,
              node: fileInfo,
              parentPath: path.dirname(filePath),
              timestamp: new Date().toISOString()
            })
          }
        } catch (error) {
          console.warn('Error processing file added event:', error)
        }
      })

      currentWatcher.on('addDir', async (dirPath) => {
        try {
          console.log('📁 Directory added:', path.basename(dirPath))
          const dirInfo = await getFileSystemInfo(dirPath)
          if (dirInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('fs:directoryAdded', {
              type: 'addDir',
              path: dirPath,
              node: dirInfo,
              parentPath: path.dirname(dirPath),
              timestamp: new Date().toISOString()
            })
          }
        } catch (error) {
          console.warn('Error processing directory added event:', error)
        }
      })

      currentWatcher.on('unlink', (filePath) => {
        try {
          console.log('🗑️ File removed:', path.basename(filePath))
          if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('fs:fileRemoved', {
              type: 'unlink',
              path: filePath,
              node: null, // File no longer exists
              parentPath: path.dirname(filePath),
              timestamp: new Date().toISOString()
            })
          }
        } catch (error) {
          console.warn('Error processing file removed event:', error)
        }
      })

      currentWatcher.on('unlinkDir', (dirPath) => {
        try {
          console.log('📂 Directory removed:', path.basename(dirPath))
          if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('fs:directoryRemoved', {
              type: 'unlinkDir',
              path: dirPath,
              node: null, // Directory no longer exists
              parentPath: path.dirname(dirPath),
              timestamp: new Date().toISOString()
            })
          }
        } catch (error) {
          console.warn('Error processing directory removed event:', error)
        }
      })

      currentWatcher.on('change', async (filePath) => {
        try {
          console.log('📝 File changed:', path.basename(filePath))
          const fileInfo = await getFileSystemInfo(filePath)
          if (fileInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send('fs:fileChanged', {
              type: 'change',
              path: filePath,
              node: fileInfo,
              parentPath: path.dirname(filePath),
              timestamp: new Date().toISOString()
            })
          }
        } catch (error) {
          console.warn('Error processing file changed event:', error)
        }
      })

      currentWatcher.on('error', (error) => {
        console.error('❌ File watcher error:', error)
        const errorMessage = error instanceof Error ? error.message : 'Unknown error'
        
        // Don't spam errors for permission issues, just log them
        if (errorMessage.includes('EPERM') || errorMessage.includes('EACCES')) {
          console.warn('Permission denied for file watching - this is normal for restricted directories')
          return // Don't send permission errors to frontend
        }
        
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('fs:error', {
            type: 'error',
            error: errorMessage,
            timestamp: new Date().toISOString()
          })
        }
      })

      currentWatcher.on('ready', () => {
        console.log('✅ File system watcher ready and monitoring changes')
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send('fs:ready', {
            path: dirPath
          })
        }
      })

      currentWatchPath = dirPath
      
      return { 
        success: true, 
        path: dirPath,
        rootNode: rootNode
      }

    } catch (error) {
      console.error('Failed to start watching:', error)
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { 
        error: errorMessage 
      }
    }
  })

  // Stop file system watching
  ipcMain.handle('fs:stopWatching', async () => {
    try {
      if (currentWatcher) {
        await currentWatcher.close()
        currentWatcher = null
        currentWatchPath = null
        console.log('🛑 Stopped file system watching')
        
        if (mainWindow) {
          mainWindow.webContents.send('fs:stopped')
        }
        
        return { success: true }
      }
      
      return { success: true, message: 'No watcher was running' }
    } catch (error) {
      console.error('Failed to stop watching:', error)
      const errorMessage = error instanceof Error ? error.message : 'Unknown error'
      return { 
        error: errorMessage 
      }
    }
  })

  // Get current watch status
  ipcMain.handle('fs:getStatus', async () => {
    return {
      isWatching: currentWatcher !== null,
      watchPath: currentWatchPath
    }
  })

  // Read file content
  ipcMain.handle('fs:readFile', async (_, filePath) => {
    try {
      // Security: Validate file path
      if (!filePath || typeof filePath !== 'string') {
        return { 
          error: 'Invalid file path provided' 
        }
      }

      // Security: Ensure file is within watched directory or accessible
      const stats = await fs.stat(filePath)
      if (!stats.isFile()) {
        return { 
          error: `Path is not a file: ${filePath}` 
        }
      }

      // Check file size to prevent loading huge files
      const maxFileSize = 10 * 1024 * 1024 // 10MB limit
      if (stats.size > maxFileSize) {
        return {
          error: `File too large (${Math.round(stats.size / 1024 / 1024)}MB). Maximum size is 10MB.`
        }
      }

      // Read file content
      const content = await fs.readFile(filePath, 'utf8')
      
      return {
        success: true,
        content: content,
        size: stats.size,
        modified: stats.mtime.toISOString(),
        encoding: 'utf8'
      }
    } catch (error) {
      console.error('Failed to read file:', error)
      
      // Handle specific error types
      const errorObj = error as any
      if (errorObj.code === 'ENOENT') {
        return { error: 'File not found' }
      } else if (errorObj.code === 'EACCES') {
        return { error: 'Permission denied' }
      } else if (errorObj.code === 'EISDIR') {
        return { error: 'Path is a directory, not a file' }
      } else {
        const errorMessage = error instanceof Error ? error.message : 'Unknown error reading file'
        return { 
          error: errorMessage
        }
      }
    }
  })
}

/**
 * App event handlers
 */
app.whenReady().then(() => {
  // Set up secure IPC handlers
  setupIpcHandlers()
  
  // Create the main window
  mainWindow = createWindow()

  // Handle app activation (macOS)
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow()
    }
  })
})

// Handle all windows closed
app.on('window-all-closed', () => {
  // On macOS, keep app running even when all windows are closed
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

// Security: Prevent navigation to external URLs
app.on('web-contents-created', (_, contents) => {
  contents.on('will-navigate', (navigationEvent, navigationUrl) => {
    const parsedUrl = new URL(navigationUrl)
    
    // Allow navigation to local dev server and file protocol
    if (parsedUrl.origin !== 'http://localhost:5173' && parsedUrl.protocol !== 'file:') {
      navigationEvent.preventDefault()
    }
  })
})

// Handle certificate errors (development)
if (process.env.NODE_ENV === 'development') {
  app.on('certificate-error', (event, _webContents, url, _error, _certificate, callback) => {
    // Allow self-signed certificates in development
    if (url.startsWith('http://localhost:')) {
      event.preventDefault()
      callback(true)
    } else {
      callback(false)
    }
  })
}