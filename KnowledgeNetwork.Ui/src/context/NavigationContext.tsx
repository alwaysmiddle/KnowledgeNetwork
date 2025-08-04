import React, { createContext, useContext, useState, ReactNode } from 'react';
import { GraphState } from '../core/graphNavigation/types';

interface NavigationContextType {
  navigationHistory: GraphState[];
  currentDepth: number;
  setNavigationState: (history: GraphState[], depth: number) => void;
  selectedNodeId: string | null;
  setSelectedNodeId: (nodeId: string | null) => void;
}

const NavigationContext = createContext<NavigationContextType | undefined>(undefined);

export const NavigationProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [navigationHistory, setNavigationHistory] = useState<GraphState[]>([]);
  const [currentDepth, setCurrentDepth] = useState(0);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  
  const setNavigationState = (history: GraphState[], depth: number) => {
    setNavigationHistory(history);
    setCurrentDepth(depth);
  };
  
  return (
    <NavigationContext.Provider 
      value={{ 
        navigationHistory, 
        currentDepth, 
        setNavigationState,
        selectedNodeId,
        setSelectedNodeId
      }}
    >
      {children}
    </NavigationContext.Provider>
  );
};

export const useNavigationContext = () => {
  const context = useContext(NavigationContext);
  if (!context) {
    throw new Error('useNavigationContext must be used within a NavigationProvider');
  }
  return context;
};