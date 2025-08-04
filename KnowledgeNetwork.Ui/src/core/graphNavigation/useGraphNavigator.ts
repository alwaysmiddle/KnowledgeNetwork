import { useState, useEffect, useCallback, useRef } from 'react';
import { ReactFlowInstance } from 'reactflow';
import { GraphNavigator } from './GraphNavigator';
import { GraphState, DataFetcher } from './types';
import { hierarchicalLayout, radialLayout, forceDirectedLayout } from './layouts';
import { animateZoomIn, animateZoomOut, fadeTransition, sleep } from './animations';
import { LayoutSelector } from './LayoutSelector';

interface UseGraphNavigatorOptions {
  initialLayouts?: boolean;
  layoutRules?: boolean;
}

export const useGraphNavigator = (
  dataFetcher: DataFetcher,
  options: UseGraphNavigatorOptions = {}
) => {
  const [navigator] = useState(() => new GraphNavigator());
  const [currentState, setCurrentState] = useState<GraphState | null>(null);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [layoutSelector] = useState(() => new LayoutSelector());
  const reactFlowInstanceRef = useRef<ReactFlowInstance | null>(null);
  
  // Initialize layouts
  useEffect(() => {
    if (options.initialLayouts !== false) {
      navigator.registerLayout(hierarchicalLayout);
      navigator.registerLayout(radialLayout);
      navigator.registerLayout(forceDirectedLayout);
    }
    
    if (options.layoutRules !== false) {
      layoutSelector.addRule(LayoutSelector.createDenseGraphRule());
      layoutSelector.addRule(LayoutSelector.createOrganizationalRule());
      layoutSelector.addRule(LayoutSelector.createNetworkRule());
      layoutSelector.addRule(LayoutSelector.createLargeGraphRule());
    }
  }, [navigator, layoutSelector, options]);
  
  const setReactFlowInstance = useCallback((instance: ReactFlowInstance) => {
    reactFlowInstanceRef.current = instance;
  }, []);
  
  const navigateToNode = useCallback(async (nodeId: string, nodeLabel?: string) => {
    if (isTransitioning) return;
    
    setIsTransitioning(true);
    
    try {
      // Handle initial load case differently
      if (nodeId === '__initial__' || !reactFlowInstanceRef.current) {
        // Just navigate without animation
        const newState = await navigator.navigateToNode(nodeId, dataFetcher, nodeLabel);
        setCurrentState(newState);
        
        // Wait for React to render
        await sleep(100);
        
        // Fit view to new nodes if ReactFlow is ready
        if (reactFlowInstanceRef.current) {
          reactFlowInstanceRef.current.fitView({
            padding: 0.2,
            duration: 800
          });
        }
      } else {
        // Find the clicked node
        const nodes = reactFlowInstanceRef.current.getNodes();
        const targetNode = nodes.find(n => n.id === nodeId);
        
        if (targetNode) {
          // Store current viewport
          const viewport = reactFlowInstanceRef.current.getViewport();
          const currentStateId = currentState?.id;
          if (currentStateId) {
            navigator.getCurrentState()!.metadata.viewBox = viewport;
          }
          
          // Zoom into clicked node
          await animateZoomIn(targetNode, reactFlowInstanceRef.current);
        }
        
        // Fade transition
        await fadeTransition();
        
        // Navigate and update state - pass the node label
        const label = nodeLabel || targetNode?.data?.displayText || targetNode?.data?.label;
        const newState = await navigator.navigateToNode(nodeId, dataFetcher, label);
        setCurrentState(newState);
        
        // Wait for React to render
        await sleep(100);
        
        // Fit view to new nodes
        if (reactFlowInstanceRef.current) {
          reactFlowInstanceRef.current.fitView({
            padding: 0.2,
            duration: 800
          });
        }
      }
    } catch (error) {
      console.error('Navigation error:', error);
    } finally {
      setIsTransitioning(false);
    }
  }, [navigator, currentState, isTransitioning, dataFetcher]);
  
  const navigateBack = useCallback(async () => {
    if (isTransitioning || !reactFlowInstanceRef.current) return;
    
    setIsTransitioning(true);
    
    try {
      // Zoom out animation
      await animateZoomOut(reactFlowInstanceRef.current);
      
      // Fade transition
      await fadeTransition();
      
      // Navigate back
      const previousState = await navigator.navigateBack();
      if (previousState) {
        setCurrentState(previousState);
        
        // Wait for React to render
        await sleep(100);
        
        // Restore viewport or fit view
        if (previousState.metadata.viewBox && reactFlowInstanceRef.current) {
          reactFlowInstanceRef.current.setViewport(previousState.metadata.viewBox);
        } else if (reactFlowInstanceRef.current) {
          reactFlowInstanceRef.current.fitView({
            padding: 0.2,
            duration: 800
          });
        }
      }
    } catch (error) {
      console.error('Navigation back error:', error);
    } finally {
      setIsTransitioning(false);
    }
  }, [navigator, isTransitioning]);
  
  const canNavigateBack = useCallback(() => {
    return navigator.getCurrentState() !== null && 
           navigator.getCurrentState()!.metadata.depth > 0;
  }, [navigator]);
  
  const clearCache = useCallback(() => {
    navigator.clearCache();
  }, [navigator]);
  
  const getNavigationHistory = useCallback((): GraphState[] => {
    return navigator.getHistory();
  }, [navigator]);
  
  const navigateToDepth = useCallback(async (targetDepth: number) => {
    if (isTransitioning) return;
    
    const currentDepth = navigator.getCurrentIndex();
    if (targetDepth === currentDepth) return;
    
    setIsTransitioning(true);
    
    try {
      // Navigate back to target depth
      const stepsBack = currentDepth - targetDepth;
      for (let i = 0; i < stepsBack; i++) {
        await navigator.navigateBack();
      }
      
      const targetState = navigator.getCurrentState();
      if (targetState) {
        setCurrentState(targetState);
        
        if (reactFlowInstanceRef.current) {
          reactFlowInstanceRef.current.fitView({
            padding: 0.2,
            duration: 800
          });
        }
      }
    } catch (error) {
      console.error('Navigation to depth error:', error);
    } finally {
      setIsTransitioning(false);
    }
  }, [navigator, isTransitioning]);
  
  return {
    currentState,
    isTransitioning,
    navigateToNode,
    navigateBack,
    canNavigateBack,
    clearCache,
    setReactFlowInstance,
    navigator,
    layoutSelector,
    getNavigationHistory,
    navigateToDepth
  };
};