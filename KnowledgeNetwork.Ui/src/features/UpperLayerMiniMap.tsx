import React, { useMemo } from 'react';
import ReactFlow, { 
  Node, 
  Edge, 
  MiniMap, 
  Controls, 
  Background,
  NodeMouseHandler,
  ReactFlowProvider
} from 'reactflow';
import 'reactflow/dist/style.css';
import { useNavigationContext } from '../context/NavigationContext';
import { GraphState } from '../core/graphNavigation/types';

const nodeStyles = {
  normal: {
    background: '#fff',
    color: '#333',
    border: '1px solid #ddd',
    borderRadius: '6px',
    padding: '6px',
    fontSize: '10px',
    width: 120,
    textAlign: 'center' as const
  },
  highlighted: {
    background: '#e3f2fd',
    color: '#1976d2',
    border: '2px solid #1976d2',
    borderRadius: '6px',
    padding: '6px',
    fontSize: '10px',
    width: 120,
    fontWeight: 'bold' as const,
    textAlign: 'center' as const,
    boxShadow: '0 0 8px rgba(25, 118, 210, 0.4)'
  }
};

interface UpperLayerMiniMapProps {}

function UpperLayerMiniMapContent() {
  const { navigationHistory, currentDepth } = useNavigationContext();
  
  // Get the parent state (one level up)
  const parentState: GraphState | null = useMemo(() => {
    if (currentDepth > 0 && navigationHistory[currentDepth - 1]) {
      return navigationHistory[currentDepth - 1];
    }
    return null;
  }, [navigationHistory, currentDepth]);
  
  // Get the current state to find which node was clicked
  const currentState: GraphState | null = useMemo(() => {
    if (currentDepth >= 0 && navigationHistory[currentDepth]) {
      return navigationHistory[currentDepth];
    }
    return null;
  }, [navigationHistory, currentDepth]);
  
  // Prepare nodes with highlighting
  const displayNodes: Node[] = useMemo(() => {
    if (!parentState) return [];
    
    const clickedNodeId = currentState?.metadata?.parentNodeId;
    
    return parentState.nodes.map(node => ({
      ...node,
      style: node.id === clickedNodeId ? nodeStyles.highlighted : nodeStyles.normal,
      data: {
        ...node.data,
        label: node.data?.displayText || node.data?.label || node.id
      }
    }));
  }, [parentState, currentState]);
  
  const displayEdges: Edge[] = useMemo(() => {
    if (!parentState) return [];
    return parentState.edges;
  }, [parentState]);
  
  const handleNodeClick: NodeMouseHandler = (event, node) => {
    // For now, clicking nodes in the parent layer doesn't navigate
    // This could be enhanced to navigate back to parent and then to the clicked node
    console.log('Parent layer node clicked:', node.id);
  };
  
  if (!parentState || currentDepth === 0) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        color: '#666',
        fontSize: '14px',
        fontStyle: 'italic'
      }}>
        No parent layer available
      </div>
    );
  }
  
  return (
    <div style={{ 
      width: '100%', 
      height: '100%',
      border: '3px solid #ccc',
      borderRadius: '8px',
      overflow: 'hidden',
      position: 'relative',
      boxShadow: '0 2px 8px rgba(0, 0, 0, 0.1)'
    }}>
      <div style={{
        position: 'absolute',
        top: '10px',
        left: '10px',
        background: 'rgba(255, 255, 255, 0.9)',
        padding: '4px 8px',
        borderRadius: '4px',
        fontSize: '11px',
        fontWeight: 'bold',
        zIndex: 10,
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)'
      }}>
        Parent Layer
      </div>
      <ReactFlow
        nodes={displayNodes}
        edges={displayEdges}
        onNodeClick={handleNodeClick}
        fitView
        fitViewOptions={{ 
          padding: 0.6,  // Even more padding for better overview
          includeHiddenNodes: false,
          minZoom: 0.1,  // Allow significant zoom out
          maxZoom: 0.8   // Further limit zoom in for mini-map view
        }}
        nodesDraggable={false}
        nodesConnectable={false}
        elementsSelectable={false}
        zoomOnScroll={false}
        zoomOnPinch={false}
        panOnDrag={false}
        panOnScroll={false}
        preventScrolling={false}
        proOptions={{ hideAttribution: true }}
      >
        <Background />
      </ReactFlow>
    </div>
  );
}

// Wrap in ReactFlowProvider since this is a separate instance
function UpperLayerMiniMap() {
  return (
    <ReactFlowProvider>
      <UpperLayerMiniMapContent />
    </ReactFlowProvider>
  );
}

export default UpperLayerMiniMap;