import { type ReactNode } from 'react';
import { PanelLeft } from 'lucide-react';
import clsx from 'clsx';

interface AppLayoutProps {
  children: ReactNode;
  sidebar?: ReactNode;
  sidebarOpen?: boolean;
  onToggleSidebar?: () => void;
  codeViewer?: ReactNode;
  codeViewerVisible?: boolean;
}

export function AppLayout({ 
  children, 
  sidebar, 
  sidebarOpen = true, 
  onToggleSidebar,
  codeViewer,
  codeViewerVisible = false
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
        
        {/* Content Area with Split View */}
        <div className="flex-1 flex overflow-hidden">
          {/* Left content */}
          <div className={clsx(
            "transition-all duration-300 overflow-auto",
            codeViewerVisible && codeViewer ? "flex-1" : "w-full"
          )}>
            {children}
          </div>

          {/* Code viewer (right panel) */}
          {codeViewerVisible && codeViewer && (
            <div className="w-1/2 border-l border-gray-700">
              {codeViewer}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}