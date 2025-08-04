import React, { useMemo, useEffect, useState } from 'react';
import ReactFlow, { 
  Node, 
  Edge, 
  Background,
  ReactFlowProvider
} from 'reactflow';
import 'reactflow/dist/style.css';
import { useNavigationContext } from '../context/NavigationContext';
import { GraphApiService } from '../services/graphApiService';
import { GraphData } from '../core/graphNavigation/types';
import { hierarchicalLayout, radialLayout, forceDirectedLayout } from '../core/graphNavigation/layouts';

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
  parent: {
    background: '#f5f5f5',
    color: '#666',
    border: '2px solid #999',
    borderRadius: '6px',
    padding: '6px',
    fontSize: '10px',
    width: 120,
    fontWeight: 'bold' as const,
    textAlign: 'center' as const
  }
};

interface DescendantNavigatorProps {}

function DescendantNavigatorContent() {
  const { selectedNodeId, navigationHistory, currentDepth } = useNavigationContext();
  const [childrenData, setChildrenData] = useState<GraphData | null>(null);
  const [loading, setLoading] = useState(false);
  
  // Get current state to find the selected node
  const currentState = useMemo(() => {
    if (currentDepth >= 0 && navigationHistory[currentDepth]) {
      return navigationHistory[currentDepth];
    }
    return null;
  }, [navigationHistory, currentDepth]);
  
  // Fetch children when selected node changes
  useEffect(() => {
    if (selectedNodeId) {
      setLoading(true);
      GraphApiService.fetchNodeSubgraph(selectedNodeId)
        .then(data => {
          setChildrenData(data);
          setLoading(false);
        })
        .catch(error => {
          console.error('Error fetching children:', error);
          setLoading(false);
        });
    } else {
      setChildrenData(null);
    }
  }, [selectedNodeId]);
  
  // Prepare nodes with layout
  const { displayNodes, displayEdges } = useMemo(() => {
    if (!childrenData || !selectedNodeId) {
      return { displayNodes: [], displayEdges: [] };
    }
    
    // Find the selected node from current state
    const selectedNode = currentState?.nodes.find(n => n.id === selectedNodeId);
    
    // Apply layout based on the metadata hint
    let layoutedNodes = childrenData.nodes;
    if (childrenData.metadata?.layoutHint === 'radial') {
      layoutedNodes = radialLayout.calculate(childrenData.nodes, childrenData.edges, {
        centerNodeId: childrenData.nodes[0]?.id
      });
    } else if (childrenData.metadata?.layoutHint === 'force-directed') {
      layoutedNodes = forceDirectedLayout.calculate(childrenData.nodes, childrenData.edges);
    } else {
      layoutedNodes = hierarchicalLayout.calculate(childrenData.nodes, childrenData.edges);
    }
    
    // Include the parent node at the top
    const nodes: Node[] = [];
    if (selectedNode) {
      nodes.push({
        id: `parent-${selectedNodeId}`,
        type: 'default',
        position: { x: 0, y: -100 },
        style: nodeStyles.parent,
        data: {
          label: selectedNode.data?.displayText || selectedNode.data?.label || selectedNodeId
        }
      });
    }
    
    // Add children nodes
    nodes.push(...layoutedNodes.map(node => ({
      ...node,
      style: nodeStyles.normal,
      data: {
        ...node.data,
        label: node.data?.displayText || node.data?.label || node.id
      }
    })));
    
    // Create edges from parent to children
    const edges: Edge[] = [];
    if (selectedNode) {
      layoutedNodes.forEach(child => {
        edges.push({
          id: `edge-parent-${child.id}`,
          source: `parent-${selectedNodeId}`,
          target: child.id,
          type: 'default',
          style: { stroke: '#999', strokeDasharray: '5 5' }
        });
      });
    }
    
    // Add children edges
    edges.push(...childrenData.edges);
    
    return { displayNodes: nodes, displayEdges: edges };
  }, [childrenData, selectedNodeId, currentState]);
  
  if (!selectedNodeId) {
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
        Hover over a node to preview its children
      </div>
    );
  }
  
  if (loading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        color: '#666',
        fontSize: '14px'
      }}>
        Loading children...
      </div>
    );
  }
  
  if (!childrenData || displayNodes.length === 0) {
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
        No children available
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
        Child Nodes Preview
      </div>
      <ReactFlow
        nodes={displayNodes}
        edges={displayEdges}
        fitView
        fitViewOptions={{ 
          padding: 0.6,
          includeHiddenNodes: false,
          minZoom: 0.1,
          maxZoom: 0.8
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
function DescendantNavigator(props: DescendantNavigatorProps) {
  return (
    <ReactFlowProvider>
      <DescendantNavigatorContent {...props} />
    </ReactFlowProvider>
  );
}

export default DescendantNavigator;