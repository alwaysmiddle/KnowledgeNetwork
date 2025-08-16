import { useState } from 'react'

function App() {
  const [connected, setConnected] = useState(false)

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Header */}
      <header className="bg-gray-800 border-b border-gray-700 px-6 py-4">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-blue-400">Knowledge Network</h1>
          <div className="flex items-center space-x-4">
            <div className={`w-3 h-3 rounded-full ${connected ? 'bg-green-500' : 'bg-red-500'}`}></div>
            <span className="text-sm text-gray-300">
              {connected ? 'Connected' : 'Disconnected'}
            </span>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="container mx-auto px-6 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Status Panel */}
          <div className="lg:col-span-1">
            <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
              <h2 className="text-lg font-semibold mb-4">System Status</h2>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-gray-300">Frontend</span>
                  <span className="text-green-400">✓ Ready</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-300">Backend API</span>
                  <span className="text-yellow-400">⟳ Connecting</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-300">G6 Visualization</span>
                  <span className="text-green-400">✓ Loaded</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-300">Redux Store</span>
                  <span className="text-green-400">✓ Active</span>
                </div>
              </div>
            </div>
          </div>

          {/* Visualization Area */}
          <div className="lg:col-span-2">
            <div className="bg-gray-800 rounded-lg border border-gray-700 h-96 flex items-center justify-center">
              <div className="text-center">
                <div className="text-6xl mb-4">🕸️</div>
                <h3 className="text-xl font-semibold mb-2">Knowledge Graph Visualization</h3>
                <p className="text-gray-400 mb-4">G6 visualization will render here</p>
                <button 
                  onClick={() => setConnected(!connected)}
                  className="bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg font-medium transition-colors"
                >
                  {connected ? 'Disconnect' : 'Connect to Backend'}
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* Architecture Info */}
        <div className="mt-8 bg-gray-800 rounded-lg p-6 border border-gray-700">
          <h2 className="text-lg font-semibold mb-4">Architecture Status</h2>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-sm">
            <div className="text-center">
              <div className="text-blue-400 font-medium">Frontend</div>
              <div className="text-gray-300">React 19 + Vite</div>
              <div className="text-gray-300">Tailwind CSS</div>
              <div className="text-gray-300">G6 Visualization</div>
            </div>
            <div className="text-center">
              <div className="text-green-400 font-medium">Backend</div>
              <div className="text-gray-300">.NET 9 API</div>
              <div className="text-gray-300">Clean Architecture</div>
              <div className="text-gray-300">Roslyn Analysis</div>
            </div>
            <div className="text-center">
              <div className="text-purple-400 font-medium">State</div>
              <div className="text-gray-300">Redux Toolkit</div>
              <div className="text-gray-300">TypeScript</div>
              <div className="text-gray-300">Real-time Updates</div>
            </div>
            <div className="text-center">
              <div className="text-yellow-400 font-medium">Data</div>
              <div className="text-gray-300">PostgreSQL</div>
              <div className="text-gray-300">Graph Storage</div>
              <div className="text-gray-300">Apache AGE</div>
            </div>
          </div>
        </div>
      </main>
    </div>
  )
}

export default App
