/**
 * Visual Indicator Cleanup Service
 * Handles automatic cleanup of file change visual indicators after a timeout
 */

import type { AppDispatch } from '../store'
import { removeChangeIndicatorByPath } from '../store/fileSystemSlice'

// Map to track cleanup timeouts
const cleanupTimeouts = new Map<string, NodeJS.Timeout>()

/**
 * Schedule cleanup of visual indicator for a specific file path
 */
export function scheduleIndicatorCleanup(
  dispatch: AppDispatch,
  filePath: string,
  timeoutMs: number = 30000
): void {
  // Clear any existing timeout for this file path
  const existingTimeout = cleanupTimeouts.get(filePath)
  if (existingTimeout) {
    clearTimeout(existingTimeout)
  }

  // Schedule new cleanup
  const timeoutId = setTimeout(() => {
    console.log(`🧹 Auto-cleaning visual indicator for: ${filePath}`)
    dispatch(removeChangeIndicatorByPath(filePath))
    cleanupTimeouts.delete(filePath)
  }, timeoutMs)

  // Track the timeout
  cleanupTimeouts.set(filePath, timeoutId)
}

/**
 * Cancel cleanup for a specific file path
 */
export function cancelIndicatorCleanup(filePath: string): void {
  const timeout = cleanupTimeouts.get(filePath)
  if (timeout) {
    clearTimeout(timeout)
    cleanupTimeouts.delete(filePath)
  }
}

/**
 * Clear all scheduled cleanups (useful when stopping file watching)
 */
export function clearAllIndicatorCleanups(): void {
  cleanupTimeouts.forEach(timeout => clearTimeout(timeout))
  cleanupTimeouts.clear()
  console.log('🧹 Cleared all visual indicator cleanup timers')
}

/**
 * Get count of active cleanup timers (for debugging)
 */
export function getActiveCleanupCount(): number {
  return cleanupTimeouts.size
}