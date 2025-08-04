import { LayoutPlugin, GraphNode } from '../types';
import { Edge } from 'reactflow';

interface HierarchicalOptions {
  direction?: 'TB' | 'BT' | 'LR' | 'RL';
  nodeSpacing?: number;
  levelSpacing?: number;
}

function buildAdjacencyList(nodes: GraphNode[], edges: Edge[]): Map<string, string[]> {
  const graph = new Map<string, string[]>();
  
  nodes.forEach(node => {
    graph.set(node.id, []);
  });
  
  edges.forEach(edge => {
    const neighbors = graph.get(edge.source) || [];
    neighbors.push(edge.target);
    graph.set(edge.source, neighbors);
  });
  
  return graph;
}

function calculateLevels(graph: Map<string, string[]>, nodes: GraphNode[]): Map<string, number> {
  const levels = new Map<string, number>();
  const visited = new Set<string>();
  
  // Find root nodes (nodes with no incoming edges)
  const incomingCount = new Map<string, number>();
  nodes.forEach(node => incomingCount.set(node.id, 0));
  
  graph.forEach((targets) => {
    targets.forEach(target => {
      incomingCount.set(target, (incomingCount.get(target) || 0) + 1);
    });
  });
  
  const roots = nodes.filter(node => incomingCount.get(node.id) === 0);
  
  // BFS from each root
  const queue: { id: string; level: number }[] = [];
  roots.forEach(root => {
    queue.push({ id: root.id, level: 0 });
    visited.add(root.id);
  });
  
  while (queue.length > 0) {
    const { id, level } = queue.shift()!;
    levels.set(id, level);
    
    const neighbors = graph.get(id) || [];
    neighbors.forEach(neighbor => {
      if (!visited.has(neighbor)) {
        visited.add(neighbor);
        queue.push({ id: neighbor, level: level + 1 });
      }
    });
  }
  
  // Handle disconnected nodes
  nodes.forEach(node => {
    if (!levels.has(node.id)) {
      levels.set(node.id, 0);
    }
  });
  
  return levels;
}

export const hierarchicalLayout: LayoutPlugin = {
  name: 'hierarchical',
  calculate: (nodes: GraphNode[], edges: Edge[], options: HierarchicalOptions = {}) => {
    const { 
      direction = 'TB', 
      nodeSpacing = 100, 
      levelSpacing = 150 
    } = options;
    
    const graph = buildAdjacencyList(nodes, edges);
    const levels = calculateLevels(graph, nodes);
    
    // Group nodes by level
    const nodesByLevel = new Map<number, string[]>();
    let maxLevel = 0;
    
    levels.forEach((level, nodeId) => {
      if (!nodesByLevel.has(level)) {
        nodesByLevel.set(level, []);
      }
      nodesByLevel.get(level)!.push(nodeId);
      maxLevel = Math.max(maxLevel, level);
    });
    
    // Calculate positions
    return nodes.map(node => {
      const level = levels.get(node.id) || 0;
      const levelNodes = nodesByLevel.get(level) || [];
      const index = levelNodes.indexOf(node.id);
      const levelWidth = levelNodes.length * nodeSpacing;
      
      let x: number, y: number;
      
      switch (direction) {
        case 'TB':
          x = -levelWidth / 2 + index * nodeSpacing;
          y = level * levelSpacing;
          break;
        case 'BT':
          x = -levelWidth / 2 + index * nodeSpacing;
          y = -level * levelSpacing;
          break;
        case 'LR':
          x = level * levelSpacing;
          y = -levelWidth / 2 + index * nodeSpacing;
          break;
        case 'RL':
          x = -level * levelSpacing;
          y = -levelWidth / 2 + index * nodeSpacing;
          break;
        default:
          x = -levelWidth / 2 + index * nodeSpacing;
          y = level * levelSpacing;
      }
      
      return {
        ...node,
        position: { x, y }
      };
    });
  }
};