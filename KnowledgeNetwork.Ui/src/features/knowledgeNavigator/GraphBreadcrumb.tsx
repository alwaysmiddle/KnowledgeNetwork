import React from 'react';
import { GraphState } from '../../core/graphNavigation/types';

interface GraphBreadcrumbProps {
  navigationHistory: GraphState[];
  currentDepth: number;
  onNavigateToDepth: (depth: number) => void;
}

const GraphBreadcrumb: React.FC<GraphBreadcrumbProps> = ({
  navigationHistory,
  currentDepth,
  onNavigateToDepth
}) => {
  if (navigationHistory.length === 0) return null;
  
  // Helper to get a label for each navigation state
  const getStateLabel = (state: GraphState, index: number): string => {
    // First check if we have a label in metadata
    if (state.metadata?.label) {
      return state.metadata.label;
    }
    
    // For initial state
    if (index === 0 || state.id === '__initial__') {
      return 'Root';
    }
    
    // Use parent node label if available
    if (state.metadata?.parentNodeLabel) {
      return state.metadata.parentNodeLabel;
    }
    
    // If the state has a single central node, use its label
    if (state.nodes.length === 1) {
      return state.nodes[0].data?.displayText || state.nodes[0].data?.label || `Node ${state.id}`;
    }
    
    // Fallback to generic label
    return `Level ${index}`;
  };
  
  return (
    <div style={{
      position: 'absolute',
      top: '10px',
      left: '50%',
      transform: 'translateX(-50%)',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      padding: '8px 16px',
      borderRadius: '20px',
      boxShadow: '0 2px 8px rgba(0,0,0,0.15)',
      display: 'flex',
      alignItems: 'center',
      gap: '8px',
      fontSize: '14px',
      zIndex: 10,
      maxWidth: '80%',
      overflow: 'hidden'
    }}>
      {navigationHistory.map((state, index) => {
        const label = getStateLabel(state, index);
        return (
          <React.Fragment key={state.id}>
            {index > 0 && <span style={{ color: '#999' }}>→</span>}
            <button
              onClick={() => onNavigateToDepth(index)}
              disabled={index === currentDepth}
              title={label}
              style={{
                background: 'none',
                border: 'none',
                padding: '4px 8px',
                borderRadius: '4px',
                cursor: index === currentDepth ? 'default' : 'pointer',
                color: index === currentDepth ? '#333' : '#0066cc',
                fontWeight: index === currentDepth ? 'bold' : 'normal',
                textDecoration: index === currentDepth ? 'none' : 'underline',
                transition: 'all 0.2s',
                maxWidth: '150px',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap'
              }}
              onMouseEnter={(e) => {
                if (index !== currentDepth) {
                  e.currentTarget.style.backgroundColor = '#f0f0f0';
                }
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.backgroundColor = 'transparent';
              }}
            >
              {label}
            </button>
          </React.Fragment>
        );
      })}
    </div>
  );
};

export default GraphBreadcrumb;