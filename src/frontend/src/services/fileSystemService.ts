import fs from 'fs/promises';
import path from 'path';
import type { FileNode } from '../types/fileSystem';

/**
 * Service for interacting with the real file system
 */
export class FileSystemService {
  /**
   * Scan a directory and convert it to FileNode tree structure
   */
  static async scanDirectory(dirPath: string): Promise<FileNode> {
    const stats = await fs.stat(dirPath);
    const basename = path.basename(dirPath);
    const extension = path.extname(basename).slice(1);
    
    // Create base node
    const node: FileNode = {
      id: this.generateId(dirPath),
      name: basename,
      type: stats.isDirectory() ? 'folder' : 'file',
      path: dirPath,
      extension: extension || undefined,
      size: stats.isFile() ? stats.size : undefined,
      modified: stats.mtime.toISOString()
    };

    // If it's a directory, recursively scan children
    if (stats.isDirectory()) {
      try {
        const entries = await fs.readdir(dirPath, { withFileTypes: true });
        const children: FileNode[] = [];

        // Process each entry
        for (const entry of entries) {
          const childPath = path.join(dirPath, entry.name);
          
          // Skip hidden files and common ignore patterns
          if (this.shouldIgnore(entry.name)) {
            continue;
          }

          try {
            const childNode = await this.scanDirectory(childPath);
            children.push(childNode);
          } catch (error) {
            // Skip files/folders we can't access
            console.warn(`Skipping inaccessible path: ${childPath}`, error);
          }
        }

        // Sort children: folders first, then files, both alphabetically
        children.sort((a, b) => {
          if (a.type === b.type) {
            return a.name.localeCompare(b.name, undefined, { numeric: true });
          }
          return a.type === 'folder' ? -1 : 1;
        });

        node.children = children;
      } catch (error) {
        console.warn(`Could not read directory: ${dirPath}`, error);
        node.children = [];
      }
    }

    return node;
  }

  /**
   * Generate a unique ID for a file system path
   */
  private static generateId(filePath: string): string {
    return filePath.replace(/[\\\/]/g, '_').replace(/[^a-zA-Z0-9_-]/g, '');
  }

  /**
   * Determine if a file/folder should be ignored
   */
  private static shouldIgnore(name: string): boolean {
    const ignorePatterns = [
      // Hidden files
      /^\./,
      // Common ignore directories
      /^node_modules$/,
      /^\.git$/,
      /^\.vscode$/,
      /^\.vs$/,
      /^bin$/,
      /^obj$/,
      /^dist$/,
      /^build$/,
      /^coverage$/,
      /^\.nyc_output$/,
      // Temporary files
      /~$/,
      /\.tmp$/,
      /\.temp$/,
      // OS files
      /^Thumbs\.db$/,
      /^\.DS_Store$/
    ];

    return ignorePatterns.some(pattern => pattern.test(name));
  }

  /**
   * Check if a directory path exists and is accessible
   */
  static async isValidDirectory(dirPath: string): Promise<boolean> {
    try {
      const stats = await fs.stat(dirPath);
      return stats.isDirectory();
    } catch {
      return false;
    }
  }

  /**
   * Get the current working directory as a FileNode tree
   */
  static async getCurrentDirectory(): Promise<FileNode> {
    const cwd = process.cwd();
    return this.scanDirectory(cwd);
  }
}