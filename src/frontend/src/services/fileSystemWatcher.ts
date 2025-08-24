import chokidar from 'chokidar';
import path from 'path';
import type { FSWatcher } from 'chokidar';
import type { AppDispatch } from '../store';
import { 
  setRootDirectory, 
  setWatchingDirectory, 
  setLoadingFileSystem, 
  setFileSystemError,
  addFileNode,
  removeFileNode,
  updateFileNode
} from '../store/fileSystemSlice';
import { FileSystemService } from './fileSystemService';

/**
 * FileSystemWatcher manages live file system watching using Chokidar
 * with direct Redux integration for real-time UI updates
 */
export class FileSystemWatcher {
  private watcher: FSWatcher | null = null;
  private dispatch: AppDispatch | null = null;
  private watchPath: string | null = null;

  constructor(dispatch: AppDispatch) {
    this.dispatch = dispatch;
  }

  /**
   * Start watching a directory for changes
   */
  async startWatching(dirPath: string): Promise<void> {
    try {
      // Stop any existing watcher
      await this.stopWatching();

      // Validate directory
      if (!(await FileSystemService.isValidDirectory(dirPath))) {
        throw new Error(`Invalid directory: ${dirPath}`);
      }

      this.dispatch!(setLoadingFileSystem(true));
      this.dispatch!(setWatchingDirectory(dirPath));
      this.watchPath = dirPath;

      // Initial directory scan
      console.log('📁 Scanning initial directory structure...');
      const rootNode = await FileSystemService.scanDirectory(dirPath);
      this.dispatch!(setRootDirectory(rootNode));

      // Start Chokidar watcher with optimized configuration
      this.watcher = chokidar.watch(dirPath, {
        // Chokidar options for optimal performance
        ignored: this.getIgnorePatterns(),
        persistent: true,
        ignoreInitial: true, // Don't emit events for initial scan
        followSymlinks: false,
        depth: 10, // Limit depth to prevent infinite recursion
        atomic: true, // Wait for write operations to complete
        awaitWriteFinish: {
          stabilityThreshold: 100, // Wait 100ms after file stops changing
          pollInterval: 50 // Poll every 50ms during the stability check
        }
      });

      // Set up event handlers with direct Redux integration
      this.setupEventHandlers();

      console.log(`🔍 Started watching: ${dirPath}`);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      console.error('❌ Failed to start watching:', errorMessage);
      this.dispatch!(setFileSystemError(errorMessage));
    }
  }

  /**
   * Stop watching the current directory
   */
  async stopWatching(): Promise<void> {
    if (this.watcher) {
      await this.watcher.close();
      this.watcher = null;
      console.log('🛑 Stopped file system watching');
    }
    this.watchPath = null;
  }

  /**
   * Get the currently watched directory
   */
  getWatchedDirectory(): string | null {
    return this.watchPath;
  }

  /**
   * Check if currently watching a directory
   */
  isWatching(): boolean {
    return this.watcher !== null;
  }

  /**
   * Setup Chokidar event handlers with Redux actions
   */
  private setupEventHandlers(): void {
    if (!this.watcher || !this.dispatch) return;

    // File/directory added
    this.watcher.on('add', async (filePath: string) => {
      console.log(`📄 File added: ${path.basename(filePath)}`);
      await this.handleFileSystemChange('add', filePath);
    });

    this.watcher.on('addDir', async (dirPath: string) => {
      console.log(`📁 Directory added: ${path.basename(dirPath)}`);
      await this.handleFileSystemChange('addDir', dirPath);
    });

    // File/directory removed
    this.watcher.on('unlink', async (filePath: string) => {
      console.log(`🗑️ File removed: ${path.basename(filePath)}`);
      this.dispatch!(removeFileNode(filePath));
    });

    this.watcher.on('unlinkDir', async (dirPath: string) => {
      console.log(`📂 Directory removed: ${path.basename(dirPath)}`);
      this.dispatch!(removeFileNode(dirPath));
    });

    // File changed
    this.watcher.on('change', async (filePath: string) => {
      console.log(`📝 File changed: ${path.basename(filePath)}`);
      await this.handleFileSystemChange('change', filePath);
    });

    // Error handling
    this.watcher.on('error', (err: unknown) => {
      const error = err instanceof Error ? err : new Error(String(err));
      console.error('❌ File watcher error:', error.message);
      this.dispatch!(setFileSystemError(`File watcher error: ${error.message}`));
    });

    // Ready event (initial scan complete)
    this.watcher.on('ready', () => {
      console.log('✅ File system watcher ready and monitoring changes');
    });
  }

  /**
   * Handle file system changes by updating Redux state
   */
  private async handleFileSystemChange(eventType: string, filePath: string): Promise<void> {
    if (!this.dispatch || !this.watchPath) return;

    try {
      if (eventType === 'add' || eventType === 'addDir' || eventType === 'change') {
        // Get updated file info
        const fileNode = await FileSystemService.scanDirectory(filePath);
        
        if (eventType === 'change') {
          // Update existing file
          this.dispatch!(updateFileNode(fileNode));
        } else {
          // Add new file/directory
          const parentPath = path.dirname(filePath);
          this.dispatch!(addFileNode({ parentPath, node: fileNode }));
        }
      }
    } catch (error) {
      console.warn(`⚠️ Could not process ${eventType} event for ${filePath}:`, error);
      // Don't update Redux on error, just log it
    }
  }

  /**
   * Get ignore patterns for Chokidar
   * Using Chokidar's built-in filtering instead of custom logic
   */
  private getIgnorePatterns(): (string | RegExp)[] {
    return [
      // Hidden files and directories
      /(^|[\/\\])\../, // Matches .git, .vscode, .env, etc.
      
      // Node.js
      '**/node_modules/**',
      '**/npm-debug.log*',
      '**/yarn-debug.log*',
      '**/yarn-error.log*',
      
      // Build outputs
      '**/dist/**',
      '**/build/**',
      '**/out/**',
      '**/target/**',
      
      // .NET
      '**/bin/**',
      '**/obj/**',
      
      // IDE files
      '**/.vs/**',
      '**/.vscode/**',
      '**/.idea/**',
      
      // OS files
      '**/Thumbs.db',
      '**/.DS_Store',
      
      // Temporary files
      '**/*.tmp',
      '**/*.temp',
      '**/*~',
      
      // Logs
      '**/logs/**',
      '**/*.log',
      
      // Coverage reports
      '**/coverage/**',
      '**/.nyc_output/**'
    ];
  }
}

/**
 * Singleton instance for global file system watching
 */
let globalWatcher: FileSystemWatcher | null = null;

/**
 * Get or create the global file system watcher instance
 */
export const getFileSystemWatcher = (dispatch: AppDispatch): FileSystemWatcher => {
  if (!globalWatcher) {
    globalWatcher = new FileSystemWatcher(dispatch);
  }
  return globalWatcher;
};

/**
 * Cleanup function to properly close watchers
 */
export const cleanupFileSystemWatcher = async (): Promise<void> => {
  if (globalWatcher) {
    await globalWatcher.stopWatching();
    globalWatcher = null;
  }
};