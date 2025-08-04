import { useCallback, useEffect, useState } from "react";
import {
  Background,
  Controls,
  ReactFlow,
  Node,
  Edge,
  OnConnect,
  addEdge,
  useEdgesState,
  NodeMouseHandler,
} from "reactflow";
import "reactflow/dist/style.css";

import { useGraphNavigator, GraphData } from "../../core/graphNavigation";
import { GraphApiService } from "../../services/graphApiService";
import { nodeTypes } from "../../nodes";
import { edgeTypes } from "../../edges";
import KnowledgeNavigatorToolbars from "./KnowledgeNavigatorToolbar";
import GraphBreadcrumb from "./GraphBreadcrumb";
import { useNavigationContext } from "../../context/NavigationContext";

interface GraphNavigatorProps {
  onNodeClick?: (nodeId: string) => void;
}

function GraphNavigator({ onNodeClick }: GraphNavigatorProps) {
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [hasInitialized, setHasInitialized] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { setNavigationState, setSelectedNodeId } = useNavigationContext();
  
  // Data fetcher for the graph navigator
  const fetchNodeData = useCallback(async (nodeId: string): Promise<GraphData> => {
    try {
      // Handle initial load case
      if (nodeId === '__initial__') {
        return await GraphApiService.fetchInitialGraph();
      }
      
      // Fetch subgraph for specific node
      return await GraphApiService.fetchNodeSubgraph(nodeId);
    } catch (error) {
      console.error('Error fetching graph data:', error);
      throw error;
    }
  }, []);
  
  const {
    currentState,
    isTransitioning,
    navigateToNode,
    navigateBack,
    canNavigateBack,
    setReactFlowInstance,
    getNavigationHistory,
    navigateToDepth
  } = useGraphNavigator(fetchNodeData);
  
  // Initialize with initial graph
  useEffect(() => {
    if (!hasInitialized && !currentState) {
      const loadInitialGraph = async () => {
        try {
          setInitialLoading(true);
          setError(null);
          
          // Use a special initial node ID with a descriptive label
          await navigateToNode('__initial__', 'Root');
          setHasInitialized(true);
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Failed to load initial graph');
        } finally {
          setInitialLoading(false);
        }
      };
      
      loadInitialGraph();
    }
  }, [currentState, hasInitialized, navigateToNode]);
  
  // Update navigation context whenever navigation changes
  useEffect(() => {
    if (currentState) {
      const history = getNavigationHistory();
      const depth = history.length - 1;
      setNavigationState(history, depth);
    }
  }, [currentState, getNavigationHistory, setNavigationState]);
  
  const onConnect: OnConnect = useCallback(
    (connection) => setEdges((edges) => addEdge(connection, edges)),
    [setEdges]
  );
  
  const handleNodeClick: NodeMouseHandler = useCallback((event, node) => {
    // Single click now just selects the node for preview
    setSelectedNodeId(node.id);
    onNodeClick?.(node.id);
  }, [setSelectedNodeId, onNodeClick]);
  
  const onNodeDoubleClick: NodeMouseHandler = useCallback((_, node) => {
    // Double click navigates into the node
    if (!isTransitioning) {
      const nodeLabel = node.data?.displayText || node.data?.label || `Node ${node.id}`;
      navigateToNode(node.id, nodeLabel);
    }
  }, [navigateToNode, isTransitioning]);
  
  // Handle zoom out button
  const handleZoomOut = useCallback(() => {
    if (canNavigateBack()) {
      navigateBack();
    }
  }, [navigateBack, canNavigateBack]);
  
  // Handle node hover/selection for descendant preview
  const handleNodeMouseEnter: NodeMouseHandler = useCallback((event, node) => {
    // Optional: You can still show preview on hover if desired
    // Comment out the next line to disable hover preview
    // setSelectedNodeId(node.id);
  }, [setSelectedNodeId]);
  
  const handleNodeMouseLeave: NodeMouseHandler = useCallback(() => {
    // Don't clear selection on mouse leave - selection is now controlled by click
  }, []);
  
  // Show loading state
  if (initialLoading && !hasInitialized) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        fontSize: '18px'
      }}>
        Loading nodes from API...
      </div>
    );
  }
  
  // Show error state
  if (error && !hasInitialized) {
    return (
      <div style={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        fontSize: '18px',
        color: 'red'
      }}>
        <div>Error loading nodes: {error}</div>
        <div style={{ fontSize: '14px', marginTop: '10px', color: '#666' }}>
          Make sure your .NET API is running on localhost:5000
        </div>
      </div>
    );
  }
  
  // Show empty state if no nodes
  if (!currentState || currentState.nodes.length === 0) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100%',
        fontSize: '18px',
        color: '#666'
      }}>
        No nodes found. Try adding some nodes through the API.
      </div>
    );
  }
  
  return (
    <div style={{ width: '100%', height: '100%', position: 'relative' }}>
      {/* Breadcrumb Navigation */}
      <GraphBreadcrumb 
        navigationHistory={getNavigationHistory()}
        currentDepth={getNavigationHistory().length - 1}
        onNavigateToDepth={navigateToDepth}
      />
      
      <ReactFlow
        nodes={currentState.nodes}
        nodeTypes={nodeTypes}
        edges={currentState.edges}
        edgeTypes={edgeTypes}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onNodeClick={handleNodeClick}
        onNodeDoubleClick={onNodeDoubleClick}
        onNodeMouseEnter={handleNodeMouseEnter}
        onNodeMouseLeave={handleNodeMouseLeave}
        onInit={setReactFlowInstance}
        fitView
      >
        <Background />
        <Controls />
        <KnowledgeNavigatorToolbars />
      </ReactFlow>
      
      {/* Zoom Out Button */}
      {canNavigateBack() && (
        <button
          onClick={handleZoomOut}
          disabled={isTransitioning}
          style={{
            position: 'absolute',
            top: '10px',
            left: '10px',
            padding: '8px 16px',
            backgroundColor: '#4CAF50',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: isTransitioning ? 'not-allowed' : 'pointer',
            opacity: isTransitioning ? 0.5 : 1,
            fontSize: '14px',
            fontWeight: 'bold',
            boxShadow: '0 2px 4px rgba(0,0,0,0.2)'
          }}
        >
          ← Zoom Out
        </button>
      )}
      
      {/* Transition overlay */}
      <div
        style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          backgroundColor: 'white',
          opacity: isTransitioning ? 0.6 : 0,
          transition: 'opacity 0.3s ease-in-out',
          pointerEvents: isTransitioning ? 'all' : 'none',
          zIndex: isTransitioning ? 1000 : -1
        }}
      />
    </div>
  );
}

export default GraphNavigator;