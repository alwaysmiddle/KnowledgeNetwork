import { type ReactNode } from 'react';
import { PanelLeft } from 'lucide-react';
import clsx from 'clsx';

interface AppLayoutProps {
  children: ReactNode;
  sidebar?: ReactNode;
  sidebarOpen?: boolean;
  onToggleSidebar?: () => void;
}

export function AppLayout({ 
  children, 
  sidebar, 
  sidebarOpen = true, 
  onToggleSidebar 
}: AppLayoutProps) {
  return (
    <div className="flex h-screen bg-gray-900">
      {/* Sidebar */}
      {sidebar && (
        <div className={clsx(
          'bg-gray-800 border-r border-gray-700 transition-all duration-300 ease-in-out',
          sidebarOpen ? 'w-64' : 'w-0 overflow-hidden'
        )}>
          {sidebar}
        </div>
      )}
      
      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Toolbar */}
        {sidebar && (
          <div className="bg-gray-800 border-b border-gray-700 px-4 py-2 flex items-center">
            <button
              onClick={onToggleSidebar}
              className="p-2 rounded-lg hover:bg-gray-700 text-gray-300 hover:text-white transition-colors"
              title={sidebarOpen ? 'Close sidebar' : 'Open sidebar'}
            >
              <PanelLeft size={18} />
            </button>
          </div>
        )}
        
        {/* Content Area */}
        <div className="flex-1 overflow-auto">
          {children}
        </div>
      </div>
    </div>
  );
}