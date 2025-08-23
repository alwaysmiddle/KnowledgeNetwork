import { type ReactNode } from 'react';

interface SidebarProps {
  title?: string;
  children: ReactNode;
}

export function Sidebar({ title = "Explorer", children }: SidebarProps) {
  return (
    <div className="h-full flex flex-col">
      {/* Sidebar Header */}
      <div className="p-4 border-b border-gray-700">
        <h2 className="text-lg font-semibold text-white">{title}</h2>
      </div>
      
      {/* Sidebar Content */}
      <div className="flex-1 overflow-auto">
        {children}
      </div>
    </div>
  );
}