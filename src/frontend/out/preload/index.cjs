"use strict";
const { contextBridge, ipcRenderer } = require("electron");
const electronAPI = {
  /**
   * Directory Selection
   */
  selectDirectory: async () => {
    try {
      return await ipcRenderer.invoke("dialog:openDirectory");
    } catch (error) {
      console.error("Directory selection error:", error);
      const errorMessage = error instanceof Error ? error.message : "Unknown error";
      return { error: errorMessage };
    }
  },
  /**
   * File System Watching
   */
  fileSystem: {
    startWatching: async (dirPath) => {
      try {
        return await ipcRenderer.invoke("fs:startWatching", dirPath);
      } catch (error) {
        console.error("Start watching error:", error);
        const errorMessage = error instanceof Error ? error.message : "Unknown error";
        return { error: errorMessage };
      }
    },
    stopWatching: async () => {
      try {
        return await ipcRenderer.invoke("fs:stopWatching");
      } catch (error) {
        console.error("Stop watching error:", error);
        const errorMessage = error instanceof Error ? error.message : "Unknown error";
        return { error: errorMessage };
      }
    },
    readFile: async (filePath) => {
      try {
        return await ipcRenderer.invoke("fs:readFile", filePath);
      } catch (error) {
        console.error("Read file error:", error);
        const errorMessage = error instanceof Error ? error.message : "Unknown error";
        return { error: errorMessage };
      }
    },
    // Event listeners for file system changes
    onFileChange: (callback) => {
      const fileAddedListener = (_event, data) => {
        callback({ ...data, type: "add" });
      };
      const fileChangedListener = (_event, data) => {
        callback({ ...data, type: "change" });
      };
      const fileRemovedListener = (_event, data) => {
        callback({ ...data, type: "unlink" });
      };
      const dirAddedListener = (_event, data) => {
        callback({ ...data, type: "addDir" });
      };
      const dirRemovedListener = (_event, data) => {
        callback({ ...data, type: "unlinkDir" });
      };
      const errorListener = (_event, data) => {
        callback({ ...data, type: "error" });
      };
      const readyListener = (_event, data) => {
        callback({ ...data, type: "ready" });
      };
      ipcRenderer.on("fs:fileAdded", fileAddedListener);
      ipcRenderer.on("fs:fileChanged", fileChangedListener);
      ipcRenderer.on("fs:fileRemoved", fileRemovedListener);
      ipcRenderer.on("fs:directoryAdded", dirAddedListener);
      ipcRenderer.on("fs:directoryRemoved", dirRemovedListener);
      ipcRenderer.on("fs:error", errorListener);
      ipcRenderer.on("fs:ready", readyListener);
      return () => {
        ipcRenderer.removeListener("fs:fileAdded", fileAddedListener);
        ipcRenderer.removeListener("fs:fileChanged", fileChangedListener);
        ipcRenderer.removeListener("fs:fileRemoved", fileRemovedListener);
        ipcRenderer.removeListener("fs:directoryAdded", dirAddedListener);
        ipcRenderer.removeListener("fs:directoryRemoved", dirRemovedListener);
        ipcRenderer.removeListener("fs:error", errorListener);
        ipcRenderer.removeListener("fs:ready", readyListener);
      };
    },
    onDirectoryChange: (callback) => {
      const dirAddedListener = (_event, data) => {
        callback({ ...data, type: "addDir" });
      };
      const dirRemovedListener = (_event, data) => {
        callback({ ...data, type: "unlinkDir" });
      };
      ipcRenderer.on("fs:directoryAdded", dirAddedListener);
      ipcRenderer.on("fs:directoryRemoved", dirRemovedListener);
      return () => {
        ipcRenderer.removeListener("fs:directoryAdded", dirAddedListener);
        ipcRenderer.removeListener("fs:directoryRemoved", dirRemovedListener);
      };
    }
  },
  /**
   * Application Information
   */
  app: {
    getVersion: () => {
      return process.env.npm_package_version || "1.0.0";
    },
    isElectron: () => {
      return true;
    },
    platform: () => {
      return process.platform;
    }
  },
  /**
   * Development Utilities
   */
  dev: {
    openDevTools: () => {
      if (process.env.NODE_ENV === "development") {
        ipcRenderer.send("dev:openDevTools");
      }
    },
    log: (message, level = "info") => {
      if (process.env.NODE_ENV === "development") {
        console.log(`[Electron ${level.toUpperCase()}]:`, message);
      }
    }
  }
};
console.log("🔒 process object available:", !!process);
console.log("🔒 process.contextIsolated:", process.contextIsolated);
console.log("🔒 process.contextIsolated type:", typeof process.contextIsolated);
if (process.contextIsolated) {
  console.log("🔒 Entering contextIsolated branch");
  try {
    contextBridge.exposeInMainWorld("electronAPI", electronAPI);
    console.log("🔒 contextBridge.exposeInMainWorld succeeded");
  } catch (error) {
    console.error("🔒 contextBridge.exposeInMainWorld failed:", error);
    window.electronAPI = electronAPI;
    console.log("🔒 Using fallback: assigned directly to window");
  }
} else {
  console.log("🔒 Using fallback branch: process.contextIsolated is", process.contextIsolated);
  window.electronAPI = electronAPI;
  console.log("🔒 Direct assignment completed");
}
const securityCheck = {
  contextIsolated: process.contextIsolated,
  // FIXED - use actual context isolation status
  nodeIntegration: typeof require !== "undefined",
  // Should be false
  sandbox: process.env.ELECTRON_SANDBOX === "true",
  // Should be true
  processType: process.type,
  electronVersion: process.versions.electron,
  contextBridgeAvailable: !!contextBridge
};
if (process.contextIsolated) {
  try {
    contextBridge.exposeInMainWorld("electronSecurityCheck", securityCheck);
  } catch (error) {
    console.error("🔒 Failed to expose security check:", error);
    window.electronSecurityCheck = securityCheck;
  }
} else {
  window.electronSecurityCheck = securityCheck;
}
console.log("🔒 Preload script is loading...");
console.log("🔒 Preload script executed, contextBridge available:", !!contextBridge);
process.on("uncaughtException", (error) => {
  console.error("Preload script error:", error);
});
window.addEventListener("error", (event) => {
  console.error("Window error in preload context:", event.error);
});
