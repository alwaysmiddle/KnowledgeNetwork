import { GraphState, GraphData, LayoutPlugin, DataFetcher } from './types';

export class GraphNavigator {
  private history: GraphState[] = [];
  private currentIndex: number = -1;
  private layouts: Map<string, LayoutPlugin> = new Map();
  private cache: Map<string, GraphData> = new Map();
  
  async navigateToNode(nodeId: string, fetchData: DataFetcher, nodeLabel?: string): Promise<GraphState> {
    if (this.currentIndex >= 0) {
      this.saveCurrentState();
    }
    
    const cacheKey = `node-${nodeId}`;
    let graphData: GraphData;
    
    if (this.cache.has(cacheKey)) {
      graphData = this.cache.get(cacheKey)!;
    } else {
      graphData = await fetchData(nodeId);
      this.cache.set(cacheKey, graphData);
    }
    
    const layout = this.selectLayout(graphData);
    const positionedNodes = layout.calculate(graphData.nodes, graphData.edges);
    
    // Find the parent node label if we're navigating from a current state
    let parentNodeLabel: string | undefined;
    if (this.getCurrentState()) {
      const parentNode = this.getCurrentState()!.nodes.find(n => n.id === nodeId);
      parentNodeLabel = parentNode?.data?.displayText || parentNode?.data?.label || nodeLabel;
    }
    
    const newState: GraphState = {
      id: nodeId,
      nodes: positionedNodes,
      edges: graphData.edges,
      layout: layout.name,
      metadata: {
        parentNodeId: nodeId,  // The ID of the node that was clicked to get here
        parentNodeLabel: parentNodeLabel,
        label: nodeLabel || parentNodeLabel || `Node ${nodeId}`,
        depth: (this.getCurrentState()?.metadata.depth || 0) + 1,
        timestamp: Date.now()
      }
    };
    
    this.history = this.history.slice(0, this.currentIndex + 1);
    this.history.push(newState);
    this.currentIndex++;
    
    return newState;
  }
  
  async navigateBack(): Promise<GraphState | null> {
    if (this.currentIndex > 0) {
      this.currentIndex--;
      return this.history[this.currentIndex];
    }
    return null;
  }
  
  navigateForward(): GraphState | null {
    if (this.currentIndex < this.history.length - 1) {
      this.currentIndex++;
      return this.history[this.currentIndex];
    }
    return null;
  }
  
  getCurrentState(): GraphState | null {
    if (this.currentIndex >= 0 && this.currentIndex < this.history.length) {
      return this.history[this.currentIndex];
    }
    return null;
  }
  
  registerLayout(layout: LayoutPlugin): void {
    this.layouts.set(layout.name, layout);
  }
  
  clearCache(): void {
    this.cache.clear();
  }
  
  getHistory(): GraphState[] {
    return this.history.slice(0, this.currentIndex + 1);
  }
  
  getCurrentIndex(): number {
    return this.currentIndex;
  }
  
  private saveCurrentState(): void {
    const currentState = this.getCurrentState();
    if (currentState) {
      const cacheKey = `node-${currentState.id}`;
      this.cache.set(cacheKey, {
        nodes: currentState.nodes,
        edges: currentState.edges,
        metadata: { layoutHint: currentState.layout }
      });
    }
  }
  
  private selectLayout(data: GraphData): LayoutPlugin {
    if (data.metadata?.layoutHint && this.layouts.has(data.metadata.layoutHint)) {
      return this.layouts.get(data.metadata.layoutHint)!;
    }
    
    if (this.isHierarchical(data)) {
      return this.layouts.get('hierarchical') || this.getDefaultLayout();
    } else if (this.isDense(data)) {
      return this.layouts.get('force-directed') || this.getDefaultLayout();
    } else {
      return this.layouts.get('radial') || this.getDefaultLayout();
    }
  }
  
  private getDefaultLayout(): LayoutPlugin {
    return this.layouts.values().next().value || {
      name: 'default',
      calculate: (nodes) => nodes
    };
  }
  
  private isHierarchical(data: GraphData): boolean {
    // Simple heuristic: check if most nodes have only one parent
    const childCount = new Map<string, number>();
    data.edges.forEach(edge => {
      childCount.set(edge.target, (childCount.get(edge.target) || 0) + 1);
    });
    
    const singleParentNodes = Array.from(childCount.values()).filter(count => count === 1).length;
    return singleParentNodes > childCount.size * 0.7;
  }
  
  private isDense(data: GraphData): boolean {
    // Dense if edges > nodes * 1.5
    return data.edges.length > data.nodes.length * 1.5;
  }
}