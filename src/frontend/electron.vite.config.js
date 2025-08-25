import { resolve } from 'path'
import { defineConfig, externalizeDepsPlugin } from 'electron-vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  /**
   * Main process configuration
   */
  main: {
    plugins: [externalizeDepsPlugin()],
    build: {
      rollupOptions: {
        input: {
          index: resolve(__dirname, 'src/electron/main.ts')
        }
      }
    }
  },

  /**
   * Preload script configuration  
   */
  preload: {
    plugins: [externalizeDepsPlugin()],
    build: {
      rollupOptions: {
        input: {
          index: resolve(__dirname, 'src/electron/preload.ts')
        },
        output: {
          format: 'cjs' // Force CommonJS format for Electron preload compatibility
        }
      }
    }
  },

  /**
   * Renderer (React app) configuration
   * Reuse existing Vite configuration with React plugin
   */
  renderer: {
    plugins: [react(), tailwindcss()],
    
    // Use existing React app as entry point
    root: '.',
    build: {
      rollupOptions: {
        input: resolve(__dirname, 'index.html')
      },
      outDir: 'dist-electron/renderer'
    },

    // Development server settings
    server: {
      port: 5173, // Match existing Vite dev server
      strictPort: true
    },

    // Resolve settings to match existing setup
    resolve: {
      alias: {
        '@': resolve(__dirname, 'src')
      }
    },

    // CSS processing handled by @tailwindcss/vite plugin

    // Environment variables
    define: {
      __IS_ELECTRON__: true
    }
  }
})