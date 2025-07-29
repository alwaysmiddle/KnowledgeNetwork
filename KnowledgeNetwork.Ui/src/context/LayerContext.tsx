import React, {
  createContext,
  useContext,
  ReactNode,
  useCallback,
  useState,
} from "react";
import {
  NodeChange,
  useNodesState,
  useReactFlow,
  getNodesBounds,
} from "reactflow";
import { type KnowledgeNode } from "../types/KnowledgeNode";

interface LayerContextType {
  upperLayer: KnowledgeNode[];
  setUpperLayer: React.Dispatch<React.SetStateAction<KnowledgeNode[]>>;
  currentLayer: KnowledgeNode[];
  setCurrentLayer: React.Dispatch<React.SetStateAction<KnowledgeNode[]>>;
  lowerLayer: KnowledgeNode[];
  setLowerLayer: React.Dispatch<React.SetStateAction<KnowledgeNode[]>>;
  getUpperLayerNodes: (nodeId: string) => KnowledgeNode[];
  getLowerLayerNodes: (nodeId: string) => KnowledgeNode[];
  triggerZoomIn: () => void;
  triggerZoomOut: () => void;
  onCurrentLayerNodesChange: (changes: NodeChange[]) => void;
}

const LayerContext = createContext<LayerContextType | undefined>(undefined);

export const LayerContextProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [currentLayer, setCurrentLayer, onCurrentLayerNodesChange] = useNodesState([]);
  const { getNodes, setCenter, fitView, fitBounds } = useReactFlow();
  const [upperLayer, setUpperLayer] = useState<KnowledgeNode[]>([]);
  const [lowerLayer, setLowerLayer] = useState<KnowledgeNode[]>([]);

  const getLowerLayer = useCallback(
    (_nodeId: string) => {
      // TODO: Replace with API call to get child nodes
      return [];
    },
    []
  );

  const getUpperLayer = useCallback((_nodeId: string) => {
    // TODO: Replace with API call to get parent nodes
    return [];
  }, []);

  const sleep = (ms: number) =>
    new Promise((resolve) => setTimeout(resolve, ms));

  const triggerZoomInCall = useCallback(async () => {
    const zoomInAnimationMs: number = 600;
    const selectedNode = getNodes().find((node) => node.selected);
    if (selectedNode) {
      const nextLayer = getLowerLayer(selectedNode.id);
      const { x, y } = selectedNode.positionAbsolute || { x: 0, y: 0 };
      const centerX = x + (selectedNode.width ? selectedNode.width / 2 : 0);
      const centerY = y + (selectedNode.height ? selectedNode.height / 2 : 0);

      setCenter(centerX, centerY, { duration: zoomInAnimationMs, zoom: 5000 });
      await sleep(zoomInAnimationMs);

      setCurrentLayer(nextLayer);
      triggerNodesLoadedAnimation(nextLayer);
    }
  }, [getNodes, setCenter]);

  const triggerZoomOutCall = useCallback(async () => {
    const zoomOutAnimationMs: number = 600;

    const nextLayer =  upperLayer ;
    console.log("I ran");

    setCenter(0, 0, { duration: zoomOutAnimationMs, zoom: 0.01 });
    await sleep(zoomOutAnimationMs);

    setCurrentLayer(nextLayer);
    triggerNodesLoadedAnimation(nextLayer);
    
  }, [getNodes, setCenter, getUpperLayer]);

  const triggerNodesLoadedAnimation = useCallback(
    async (nodes: KnowledgeNode[]) => {
      await sleep(200);

      if (nodes.length > 0) {
        const bounds = getNodesBounds(nodes);
        fitBounds(bounds, { duration: 0, padding: 5 });
      }

      await sleep(10);

      fitView({
        padding: 0.5,
        includeHiddenNodes: false,
        minZoom: 0.5,
        maxZoom: 5,
        duration: 600,
        nodes: nodes,
      });
    },
    [setCenter]
  );

  return (
    <LayerContext.Provider
      value={{
        upperLayer,
        setUpperLayer,
        currentLayer,
        setCurrentLayer,
        lowerLayer,
        setLowerLayer,
        onCurrentLayerNodesChange,
        getLowerLayerNodes: getLowerLayer,
        getUpperLayerNodes: getUpperLayer,
        triggerZoomIn: triggerZoomInCall,
        triggerZoomOut: triggerZoomOutCall,
      }}
    >
      {children}
    </LayerContext.Provider>
  );
};

export const useLayerContext = () => {
  const context = useContext(LayerContext);
  if (!context) {
    throw new Error(
      "useLayerContext must be used within an LayerContextProvider"
    );
  }
  return context;
};
