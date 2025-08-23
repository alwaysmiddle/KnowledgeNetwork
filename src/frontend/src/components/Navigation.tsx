import { Link, useLocation } from 'react-router-dom';
import { Home, Code } from 'lucide-react';
import clsx from 'clsx';

export function Navigation() {
  const location = useLocation();
  
  return (
    <nav className="bg-gray-800 border-b border-gray-700 px-6 py-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-blue-400">Knowledge Network</h1>
        
        <div className="flex space-x-1">
          <Link
            to="/"
            className={clsx(
              'flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors',
              location.pathname === '/'
                ? 'bg-blue-600 text-white'
                : 'text-gray-300 hover:text-white hover:bg-gray-700'
            )}
          >
            <Home size={18} />
            Dashboard
          </Link>
          <Link
            to="/code-analysis"
            className={clsx(
              'flex items-center gap-2 px-4 py-2 rounded-lg font-medium transition-colors',
              location.pathname === '/code-analysis'
                ? 'bg-blue-600 text-white'
                : 'text-gray-300 hover:text-white hover:bg-gray-700'
            )}
          >
            <Code size={18} />
            Code Analysis
          </Link>
        </div>
      </div>
    </nav>
  );
}