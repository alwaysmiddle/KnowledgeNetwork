import type { AppDispatch } from '../store';
import { 
  setRootDirectory, 
  setWatchingDirectory, 
  setLoadingFileSystem, 
  setFileSystemError,
  addFileNode,
  removeFileNode,
  updateFileNode,
  removeOldChangeIndicators
} from '../store/fileSystemSlice';
import type { FileNode } from '../types/fileSystem';

/**
 * MockFileWatcher simulates file system watching for testing Redux flow
 * without requiring Node.js file system APIs
 */
export class MockFileWatcher {
  private dispatch: AppDispatch | null = null;
  private watchPath: string | null = null;
  private isActive: boolean = false;
  private simulationTimer: NodeJS.Timeout | null = null;
  private cleanupTimer: NodeJS.Timeout | null = null;
  private changeCounter: number = 0;

  constructor(dispatch: AppDispatch) {
    this.dispatch = dispatch;
  }

  /**
   * Start "watching" a directory by creating mock file system data
   */
  async startWatching(dirPath: string): Promise<void> {
    try {
      this.dispatch!(setLoadingFileSystem(true));
      this.dispatch!(setWatchingDirectory(dirPath));
      this.watchPath = dirPath;
      this.isActive = true;

      // Simulate initial directory scan delay
      await new Promise(resolve => setTimeout(resolve, 800));

      // Create mock file system structure
      const rootNode = this.createMockFileSystem(dirPath);
      this.dispatch!(setRootDirectory(rootNode));

      // Start simulating file changes
      this.startSimulation();

      // Start cleanup timer for old change indicators
      this.startCleanupTimer();

      console.log(`🧪 Started mock file watching: ${dirPath}`);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      console.error('❌ Failed to start mock watching:', errorMessage);
      this.dispatch!(setFileSystemError(errorMessage));
    }
  }

  /**
   * Stop watching the current directory
   */
  async stopWatching(): Promise<void> {
    if (this.simulationTimer) {
      clearInterval(this.simulationTimer);
      this.simulationTimer = null;
    }

    if (this.cleanupTimer) {
      clearInterval(this.cleanupTimer);
      this.cleanupTimer = null;
    }
    
    this.isActive = false;
    this.watchPath = null;
    this.changeCounter = 0;
    
    console.log('🛑 Stopped mock file system watching');
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
    return this.isActive;
  }

  /**
   * Create a realistic mock file system structure
   */
  private createMockFileSystem(rootPath: string): FileNode {
    const baseName = rootPath.split('/').pop() || rootPath.split('\\').pop() || 'root';
    
    return {
      id: 'mock-root',
      name: baseName,
      type: 'folder',
      path: rootPath,
      modified: new Date().toISOString(),
      children: [
        {
          id: 'mock-src',
          name: 'src',
          type: 'folder',
          path: `${rootPath}/src`,
          modified: new Date(Date.now() - 3600000).toISOString(), // 1 hour ago
          children: [
            {
              id: 'mock-components',
              name: 'components',
              type: 'folder',
              path: `${rootPath}/src/components`,
              modified: new Date(Date.now() - 1800000).toISOString(), // 30 min ago
              children: [
                {
                  id: 'mock-app-tsx',
                  name: 'App.tsx',
                  type: 'file',
                  path: `${rootPath}/src/components/App.tsx`,
                  extension: 'tsx',
                  size: 2156,
                  modified: new Date(Date.now() - 900000).toISOString() // 15 min ago
                },
                {
                  id: 'mock-button-tsx',
                  name: 'Button.tsx',
                  type: 'file',
                  path: `${rootPath}/src/components/Button.tsx`,
                  extension: 'tsx',
                  size: 1234,
                  modified: new Date(Date.now() - 600000).toISOString() // 10 min ago
                }
              ]
            },
            {
              id: 'mock-utils',
              name: 'utils',
              type: 'folder',
              path: `${rootPath}/src/utils`,
              modified: new Date(Date.now() - 1200000).toISOString(), // 20 min ago
              children: [
                {
                  id: 'mock-helpers-ts',
                  name: 'helpers.ts',
                  type: 'file',
                  path: `${rootPath}/src/utils/helpers.ts`,
                  extension: 'ts',
                  size: 3456,
                  modified: new Date(Date.now() - 1200000).toISOString() // 20 min ago
                }
              ]
            }
          ]
        },
        {
          id: 'mock-docs',
          name: 'docs',
          type: 'folder',
          path: `${rootPath}/docs`,
          modified: new Date(Date.now() - 2400000).toISOString(), // 40 min ago
          children: [
            {
              id: 'mock-readme',
              name: 'README.md',
              type: 'file',
              path: `${rootPath}/docs/README.md`,
              extension: 'md',
              size: 2500,
              modified: new Date(Date.now() - 2400000).toISOString() // 40 min ago
            }
          ]
        }
      ]
    };
  }

  /**
   * Start simulating file system changes
   */
  private startSimulation(): void {
    if (this.simulationTimer) {
      clearInterval(this.simulationTimer);
    }

    // Simulate file changes every 5-10 seconds
    this.simulationTimer = setInterval(() => {
      if (!this.isActive || !this.dispatch || !this.watchPath) return;

      this.changeCounter++;
      const changeType = this.getRandomChangeType();

      switch (changeType) {
        case 'add':
          this.simulateFileAdd();
          break;
        case 'update':
          this.simulateFileUpdate();
          break;
        case 'remove':
          this.simulateFileRemove();
          break;
      }
    }, Math.random() * 5000 + 3000); // Random interval between 3-8 seconds
  }

  private getRandomChangeType(): 'add' | 'update' | 'remove' {
    const types: Array<'add' | 'update' | 'remove'> = ['add', 'update', 'update', 'remove']; // Weight towards updates
    return types[Math.floor(Math.random() * types.length)];
  }

  private simulateFileAdd(): void {
    const newFile: FileNode = {
      id: `mock-new-${this.changeCounter}`,
      name: `NewFile${this.changeCounter}.tsx`,
      type: 'file',
      path: `${this.watchPath}/src/components/NewFile${this.changeCounter}.tsx`,
      extension: 'tsx',
      size: Math.floor(Math.random() * 5000) + 500,
      modified: new Date().toISOString()
    };

    console.log(`📄 Simulated file added: ${newFile.name}`);
    this.dispatch!(addFileNode({ 
      parentPath: `${this.watchPath}/src/components`, 
      node: newFile 
    }));
  }

  private simulateFileUpdate(): void {
    // Simulate updating an existing file
    const updatedFile: FileNode = {
      id: 'mock-app-tsx',
      name: 'App.tsx',
      type: 'file',
      path: `${this.watchPath}/src/components/App.tsx`,
      extension: 'tsx',
      size: Math.floor(Math.random() * 3000) + 1000,
      modified: new Date().toISOString()
    };

    console.log(`📝 Simulated file updated: ${updatedFile.name}`);
    this.dispatch!(updateFileNode(updatedFile));
  }

  private simulateFileRemove(): void {
    if (this.changeCounter > 2) { // Only remove files after we've added some
      const fileToRemove = `${this.watchPath}/src/components/NewFile${this.changeCounter - 1}.tsx`;
      console.log(`🗑️ Simulated file removed: NewFile${this.changeCounter - 1}.tsx`);
      this.dispatch!(removeFileNode(fileToRemove));
    }
  }

  /**
   * Start cleanup timer to remove old change indicators
   */
  private startCleanupTimer(): void {
    if (this.cleanupTimer) {
      clearInterval(this.cleanupTimer);
    }

    // Clean up change indicators every 15 seconds
    this.cleanupTimer = setInterval(() => {
      if (!this.isActive || !this.dispatch) return;

      // Remove indicators older than 30 seconds
      this.dispatch!(removeOldChangeIndicators(30000));
    }, 15000);
  }
}

/**
 * Singleton instance for global mock file system watching
 */
let globalMockWatcher: MockFileWatcher | null = null;

/**
 * Get or create the global mock file system watcher instance
 */
export const getMockFileSystemWatcher = (dispatch: AppDispatch): MockFileWatcher => {
  if (!globalMockWatcher) {
    globalMockWatcher = new MockFileWatcher(dispatch);
  }
  return globalMockWatcher;
};

/**
 * Cleanup function to properly close mock watchers
 */
export const cleanupMockFileSystemWatcher = async (): Promise<void> => {
  if (globalMockWatcher) {
    await globalMockWatcher.stopWatching();
    globalMockWatcher = null;
  }
};