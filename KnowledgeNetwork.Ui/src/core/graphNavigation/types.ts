import { Node, Edge } from 'reactflow';

export interface GraphNode extends Node {
  data: {
    label: string;
    [key: string]: any;
  };
}

export interface GraphState {
  id: string;
  nodes: GraphNode[];
  edges: Edge[];
  layout: string;
  metadata: {
    parentNodeId?: string;
    parentNodeLabel?: string;
    label?: string;
    depth: number;
    timestamp: number;
    viewBox?: {
      x: number;
      y: number;
      zoom: number;
    };
  };
}

export interface GraphData {
  nodes: GraphNode[];
  edges: Edge[];
  metadata?: {
    layoutHint?: string;
    [key: string]: any;
  };
}

export interface LayoutPlugin {
  name: string;
  calculate: (
    nodes: GraphNode[],
    edges: Edge[],
    options?: any
  ) => GraphNode[];
  animateTransition?: (from: GraphNode[], to: GraphNode[]) => Animation;
}

export interface GraphContext {
  nodeCount: number;
  edgeCount: number;
  metadata: any;
  hasCenter?: boolean;
}

export interface LayoutRule {
  condition: (context: GraphContext) => boolean;
  layout: string;
}

export interface AnimateOptions {
  duration: number;
  onUpdate: (progress: number) => void;
}

export type DataFetcher = (nodeId: string) => Promise<GraphData>;