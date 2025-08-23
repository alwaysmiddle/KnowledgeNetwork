export function CodeAnalysisPage() {
  return (
    <div className="min-h-screen bg-gray-900 text-white p-8">
      <div className="max-w-6xl mx-auto">
        <h1 className="text-3xl font-bold text-blue-400 mb-4">Code Analysis</h1>
        <p className="text-gray-400 text-lg mb-8">
          File explorer and code analysis will appear here
        </p>
        
        <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
          <h2 className="text-xl font-semibold mb-4">Coming Soon</h2>
          <ul className="space-y-2 text-gray-300">
            <li>📁 File tree navigation</li>
            <li>🔍 Code analysis results</li>
            <li>📊 Control flow graphs</li>
            <li>⚡ Real-time file watching</li>
          </ul>
        </div>
      </div>
    </div>
  );
}