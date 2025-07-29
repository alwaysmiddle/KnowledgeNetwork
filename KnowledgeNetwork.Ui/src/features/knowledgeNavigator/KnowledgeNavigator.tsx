import type { NodeMouseHandler, OnConnect } from "reactflow";

import { useCallback, useEffect} from "react";
import {
  Background,
  Controls,
  ReactFlow,
  addEdge,
  useEdgesState,
} from "reactflow";

import "reactflow/dist/style.css";
import { useLayerContext } from "../../context/LayerContext";
import { nodeTypes } from "../../nodes";
import { initialEdges, edgeTypes } from "../../edges";
import { useNodes } from "../../hooks/useNodes";
import KnowledgeNavigatorToolbars from "./KnowledgeNavigatorToolbar";

function KnowledgeNavigator(){
  const {currentLayer, setCurrentLayer, onCurrentLayerNodesChange} = useLayerContext();
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);
  const { nodes: apiNodes, loading, error } = useNodes();

  useEffect(()=>{
    if(!currentLayer && apiNodes.length > 0){
      setCurrentLayer(apiNodes);
    }
  },
  [currentLayer, apiNodes, setCurrentLayer]);

  const onConnect: OnConnect = useCallback(
    (connection) => setEdges((edges) => addEdge(connection, edges)),
    [setEdges]
  );

  const onNodeDoubleClick: NodeMouseHandler = (_, node) => {
    setCurrentLayer((nds) =>
      nds.map((n) => {
        if (n.id === node.id) {
          return { ...n, data: { ...n.data, isEditing: true, onEditTextComplete: () => handleEditTextComplete } };
        }
        return n;
      })
    );
  };

  const handleEditTextComplete = (nodeId: string) => {
    setCurrentLayer((nds) =>
      nds.map((n) => {
        if (n.id === nodeId) {
          return { ...n, data: { ...n.data, isEditing: false } };
        }
        return n;
      })
    );
  };

  // Show loading state
  if (loading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
        fontSize: '18px'
      }}>
        Loading nodes from API...
      </div>
    );
  }

  // Show error state
  if (error) {
    return (
      <div style={{ 
        display: 'flex', 
        flexDirection: 'column',
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
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

  // Show empty state if no nodes loaded
  if (!currentLayer || currentLayer.length === 0) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh',
        fontSize: '18px',
        color: '#666'
      }}>
        No nodes found. Try adding some nodes through the API.
      </div>
    );
  }

  return (
    <ReactFlow
      nodes={currentLayer}
      nodeTypes={nodeTypes}
      onNodesChange={onCurrentLayerNodesChange}
      onNodeDoubleClick={onNodeDoubleClick}
      edges={edges}
      edgeTypes={edgeTypes}
      onEdgesChange={onEdgesChange}
      onConnect={onConnect}
      fitView
    >
      <Background />
      <Controls />
      <KnowledgeNavigatorToolbars />
    </ReactFlow>
  );
}

export default KnowledgeNavigator;
