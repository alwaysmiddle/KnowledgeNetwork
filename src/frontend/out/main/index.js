import { app, BrowserWindow, ipcMain, dialog } from "electron";
import * as path from "path";
import * as chokidar from "chokidar";
import { promises } from "fs";
import __cjs_mod__ from "node:module";
const __filename = import.meta.filename;
const __dirname = import.meta.dirname;
const require2 = __cjs_mod__.createRequire(import.meta.url);
if (process.env.NODE_ENV === "development") {
  require2("electron-reload")(__dirname, {
    electron: path.join(__dirname, "..", "..", "node_modules", ".bin", "electron"),
    hardResetMethod: "exit"
  });
}
function createWindow() {
  const win = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      // MANDATORY SECURITY SETTINGS (2024-2025)
      nodeIntegration: false,
      // REQUIRED: No Node.js in renderer
      contextIsolation: true,
      // REQUIRED: Isolated context
      // enableRemoteModule deprecated in Electron 20+
      sandbox: true,
      // RECOMMENDED: Enable sandboxing
      preload: path.join(__dirname, "..", "preload", "index.cjs"),
      // Secure IPC bridge (built from preload.ts as CommonJS)
      // Additional security settings
      webSecurity: true,
      // Enable web security
      allowRunningInsecureContent: false,
      // No mixed content
      experimentalFeatures: false
      // No experimental features
    },
    // Window settings
    show: false,
    // Don't show until ready
    titleBarStyle: "default",
    backgroundColor: "#1a1a1a"
    // Match app theme
  });
  if (process.env.NODE_ENV === "development") {
    win.loadURL("http://localhost:5173");
    win.webContents.openDevTools();
  } else {
    win.loadFile(path.join(__dirname, "..", "..", "dist", "index.html"));
  }
  win.once("ready-to-show", () => {
    win.show();
    if (process.env.NODE_ENV === "development") {
      win.focus();
    }
  });
  win.webContents.setWindowOpenHandler(() => {
    return { action: "deny" };
  });
  return win;
}
let currentWatcher = null;
let currentWatchPath = null;
let mainWindow = null;
async function getFileSystemInfo(filePath) {
  try {
    const stats = await promises.stat(filePath);
    const parsed = path.parse(filePath);
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
      children: stats.isDirectory() ? [] : void 0
    };
  } catch (error) {
    console.error("Error getting file info:", error);
    return null;
  }
}
async function scanDirectory(dirPath, maxDepth = 10, currentDepth = 0) {
  if (currentDepth >= maxDepth) {
    console.warn(`Max depth reached for ${dirPath}`);
    return null;
  }
  try {
    const dirInfo = await getFileSystemInfo(dirPath);
    if (!dirInfo || !dirInfo.isDirectory) return dirInfo;
    const entries = await promises.readdir(dirPath);
    const children = [];
    for (const entry of entries) {
      if (entry.startsWith(".") || entry === "node_modules" || entry === "dist" || entry === "build" || entry === "obj" || entry === "bin") {
        continue;
      }
      const entryPath = path.join(dirPath, entry);
      const entryInfo = await getFileSystemInfo(entryPath);
      if (entryInfo) {
        if (entryInfo.isDirectory) {
          const childDir = await scanDirectory(entryPath, maxDepth, currentDepth + 1);
          if (childDir) children.push(childDir);
        } else {
          children.push(entryInfo);
        }
      }
    }
    dirInfo.children = children;
    return dirInfo;
  } catch (error) {
    console.error(`Error scanning directory ${dirPath}:`, error);
    return null;
  }
}
function setupIpcHandlers() {
  ipcMain.handle("dialog:openDirectory", async () => {
    try {
      const result = await dialog.showOpenDialog(mainWindow, {
        properties: ["openDirectory"],
        title: "Select Directory to Watch for File Changes"
      });
      if (result.canceled) {
        return { canceled: true };
      }
      return {
        canceled: false,
        filePaths: result.filePaths
      };
    } catch (error) {
      console.error("Directory dialog error:", error);
      const errorMessage = error instanceof Error ? error.message : "Unknown error";
      return {
        error: errorMessage
      };
    }
  });
  ipcMain.handle("fs:startWatching", async (_, dirPath) => {
    try {
      if (currentWatcher) {
        await currentWatcher.close();
        currentWatcher = null;
      }
      const dirStats = await promises.stat(dirPath);
      if (!dirStats.isDirectory()) {
        return {
          error: `Path is not a directory: ${dirPath}`
        };
      }
      console.log("🔍 Scanning directory structure:", dirPath);
      const rootNode = await scanDirectory(dirPath);
      if (!rootNode) {
        return {
          error: `Failed to scan directory: ${dirPath}`
        };
      }
      currentWatcher = chokidar.watch(dirPath, {
        ignored: [
          // Hidden files and system directories
          /(^|[\/\\])\../,
          // Build and dependency directories
          "**/node_modules/**",
          "**/dist/**",
          "**/build/**",
          "**/obj/**",
          "**/bin/**",
          "**/out/**",
          // Temporary files
          "**/*.tmp",
          "**/*.temp",
          "**/*.swp",
          "**/*.swo",
          "**/*~",
          // Log directories
          "**/logs/**",
          "**/log/**",
          // Version control
          "**/.git/**",
          "**/.svn/**",
          "**/.hg/**",
          // IDE directories
          "**/.vscode/**",
          "**/.idea/**",
          "**/target/**",
          // Windows system directories and files (to prevent EPERM)
          "**/System Volume Information/**",
          "**/pagefile.sys",
          "**/hiberfil.sys",
          "**/swapfile.sys",
          "**/Windows/**",
          "**/Program Files/**",
          "**/Program Files (x86)/**",
          "**/ProgramData/**",
          // Restricted file extensions
          "**/*.lnk",
          "**/*.sys",
          "**/*.dll",
          "**/*.exe"
        ],
        persistent: true,
        ignoreInitial: true,
        followSymlinks: false,
        depth: 8,
        // Reduced depth to avoid deep system directories
        atomic: true,
        awaitWriteFinish: {
          stabilityThreshold: 100,
          pollInterval: 50
        },
        // Add more restrictive options to prevent permission errors
        ignorePermissionErrors: true,
        usePolling: false,
        // Use native file system events when possible
        interval: 1e3,
        // Polling interval if usePolling is true
        binaryInterval: 3e3
      });
      currentWatcher.on("add", async (filePath) => {
        try {
          console.log("📄 File added:", path.basename(filePath));
          const fileInfo = await getFileSystemInfo(filePath);
          if (fileInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send("fs:fileAdded", {
              type: "add",
              path: filePath,
              node: fileInfo,
              parentPath: path.dirname(filePath),
              timestamp: (/* @__PURE__ */ new Date()).toISOString()
            });
          }
        } catch (error) {
          console.warn("Error processing file added event:", error);
        }
      });
      currentWatcher.on("addDir", async (dirPath2) => {
        try {
          console.log("📁 Directory added:", path.basename(dirPath2));
          const dirInfo = await getFileSystemInfo(dirPath2);
          if (dirInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send("fs:directoryAdded", {
              type: "addDir",
              path: dirPath2,
              node: dirInfo,
              parentPath: path.dirname(dirPath2),
              timestamp: (/* @__PURE__ */ new Date()).toISOString()
            });
          }
        } catch (error) {
          console.warn("Error processing directory added event:", error);
        }
      });
      currentWatcher.on("unlink", (filePath) => {
        try {
          console.log("🗑️ File removed:", path.basename(filePath));
          if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send("fs:fileRemoved", {
              type: "unlink",
              path: filePath,
              node: null,
              // File no longer exists
              parentPath: path.dirname(filePath),
              timestamp: (/* @__PURE__ */ new Date()).toISOString()
            });
          }
        } catch (error) {
          console.warn("Error processing file removed event:", error);
        }
      });
      currentWatcher.on("unlinkDir", (dirPath2) => {
        try {
          console.log("📂 Directory removed:", path.basename(dirPath2));
          if (mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send("fs:directoryRemoved", {
              type: "unlinkDir",
              path: dirPath2,
              node: null,
              // Directory no longer exists
              parentPath: path.dirname(dirPath2),
              timestamp: (/* @__PURE__ */ new Date()).toISOString()
            });
          }
        } catch (error) {
          console.warn("Error processing directory removed event:", error);
        }
      });
      currentWatcher.on("change", async (filePath) => {
        try {
          console.log("📝 File changed:", path.basename(filePath));
          const fileInfo = await getFileSystemInfo(filePath);
          if (fileInfo && mainWindow && !mainWindow.isDestroyed()) {
            mainWindow.webContents.send("fs:fileChanged", {
              type: "change",
              path: filePath,
              node: fileInfo,
              parentPath: path.dirname(filePath),
              timestamp: (/* @__PURE__ */ new Date()).toISOString()
            });
          }
        } catch (error) {
          console.warn("Error processing file changed event:", error);
        }
      });
      currentWatcher.on("error", (error) => {
        console.error("❌ File watcher error:", error);
        const errorMessage = error instanceof Error ? error.message : "Unknown error";
        if (errorMessage.includes("EPERM") || errorMessage.includes("EACCES")) {
          console.warn("Permission denied for file watching - this is normal for restricted directories");
          return;
        }
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send("fs:error", {
            type: "error",
            error: errorMessage,
            timestamp: (/* @__PURE__ */ new Date()).toISOString()
          });
        }
      });
      currentWatcher.on("ready", () => {
        console.log("✅ File system watcher ready and monitoring changes");
        if (mainWindow && !mainWindow.isDestroyed()) {
          mainWindow.webContents.send("fs:ready", {
            path: dirPath
          });
        }
      });
      currentWatchPath = dirPath;
      return {
        success: true,
        path: dirPath,
        rootNode
      };
    } catch (error) {
      console.error("Failed to start watching:", error);
      const errorMessage = error instanceof Error ? error.message : "Unknown error";
      return {
        error: errorMessage
      };
    }
  });
  ipcMain.handle("fs:stopWatching", async () => {
    try {
      if (currentWatcher) {
        await currentWatcher.close();
        currentWatcher = null;
        currentWatchPath = null;
        console.log("🛑 Stopped file system watching");
        if (mainWindow) {
          mainWindow.webContents.send("fs:stopped");
        }
        return { success: true };
      }
      return { success: true, message: "No watcher was running" };
    } catch (error) {
      console.error("Failed to stop watching:", error);
      const errorMessage = error instanceof Error ? error.message : "Unknown error";
      return {
        error: errorMessage
      };
    }
  });
  ipcMain.handle("fs:getStatus", async () => {
    return {
      isWatching: currentWatcher !== null,
      watchPath: currentWatchPath
    };
  });
  ipcMain.handle("fs:readFile", async (_, filePath) => {
    try {
      if (!filePath || typeof filePath !== "string") {
        return {
          error: "Invalid file path provided"
        };
      }
      const stats = await promises.stat(filePath);
      if (!stats.isFile()) {
        return {
          error: `Path is not a file: ${filePath}`
        };
      }
      const maxFileSize = 10 * 1024 * 1024;
      if (stats.size > maxFileSize) {
        return {
          error: `File too large (${Math.round(stats.size / 1024 / 1024)}MB). Maximum size is 10MB.`
        };
      }
      const content = await promises.readFile(filePath, "utf8");
      return {
        success: true,
        content,
        size: stats.size,
        modified: stats.mtime.toISOString(),
        encoding: "utf8"
      };
    } catch (error) {
      console.error("Failed to read file:", error);
      const errorObj = error;
      if (errorObj.code === "ENOENT") {
        return { error: "File not found" };
      } else if (errorObj.code === "EACCES") {
        return { error: "Permission denied" };
      } else if (errorObj.code === "EISDIR") {
        return { error: "Path is a directory, not a file" };
      } else {
        const errorMessage = error instanceof Error ? error.message : "Unknown error reading file";
        return {
          error: errorMessage
        };
      }
    }
  });
}
app.whenReady().then(() => {
  setupIpcHandlers();
  mainWindow = createWindow();
  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});
app.on("web-contents-created", (_, contents) => {
  contents.on("will-navigate", (navigationEvent, navigationUrl) => {
    const parsedUrl = new URL(navigationUrl);
    if (parsedUrl.origin !== "http://localhost:5173" && parsedUrl.protocol !== "file:") {
      navigationEvent.preventDefault();
    }
  });
});
if (process.env.NODE_ENV === "development") {
  app.on("certificate-error", (event, _webContents, url, _error, _certificate, callback) => {
    if (url.startsWith("http://localhost:")) {
      event.preventDefault();
      callback(true);
    } else {
      callback(false);
    }
  });
}
