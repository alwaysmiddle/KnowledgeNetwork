"use strict";
const { contextBridge, ipcRenderer } = require("electron");
contextBridge.exposeInMainWorld("electronAPI", {
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
      const listener = (_event, data) => {
        callback(data);
      };
      ipcRenderer.on("fs:change", listener);
      return () => {
        ipcRenderer.removeListener("fs:change", listener);
      };
    },
    onDirectoryChange: (callback) => {
      const listener = (_event, data) => {
        callback(data);
      };
      ipcRenderer.on("fs:directoryChange", listener);
      return () => {
        ipcRenderer.removeListener("fs:directoryChange", listener);
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
});
contextBridge.exposeInMainWorld("electronSecurityCheck", {
  contextIsolated: true,
  nodeIntegration: process.env.NODE_INTEGRATION === "true",
  // Should be false
  sandbox: process.env.ELECTRON_SANDBOX === "true"
  // Should be true
});
document.addEventListener("DOMContentLoaded", () => {
  document.body.classList.add("electron-app");
  if (process.env.NODE_ENV === "development") {
    console.log("🔒 Electron Security Status:");
    console.log("  Context Isolation:", !!window.electronSecurityCheck?.contextIsolated);
    console.log("  Node Integration:", !!window.electronSecurityCheck?.nodeIntegration);
    console.log("  Sandbox:", !!window.electronSecurityCheck?.sandbox);
    console.log("  Electron API Available:", !!window.electronAPI);
  }
});
process.on("uncaughtException", (error) => {
  console.error("Preload script error:", error);
});
window.addEventListener("error", (event) => {
  console.error("Window error in preload context:", event.error);
});
