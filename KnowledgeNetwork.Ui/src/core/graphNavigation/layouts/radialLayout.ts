import { LayoutPlugin, GraphNode } from '../types';
import { Edge } from 'reactflow';

interface RadialOptions {
  centerNodeId?: string;
  radiusStep?: number;
  minRadius?: number;
}

function bfsLevels(startId: string, edges: Edge[]): Map<string, number> {
  const levels = new Map<string, number>();
  const adjacency = new Map<string, Set<string>>();
  
  // Build bidirectional adjacency list
  edges.forEach(edge => {
    if (!adjacency.has(edge.source)) adjacency.set(edge.source, new Set());
    if (!adjacency.has(edge.target)) adjacency.set(edge.target, new Set());
    adjacency.get(edge.source)!.add(edge.target);
    adjacency.get(edge.target)!.add(edge.source);
  });
  
  // BFS from start node
  const queue: string[] = [startId];
  const visited = new Set<string>([startId]);
  levels.set(startId, 0);
  
  while (queue.length > 0) {
    const current = queue.shift()!;
    const currentLevel = levels.get(current)!;
    
    const neighbors = adjacency.get(current) || new Set();
    neighbors.forEach(neighbor => {
      if (!visited.has(neighbor)) {
        visited.add(neighbor);
        levels.set(neighbor, currentLevel + 1);
        queue.push(neighbor);
      }
    });
  }
  
  return levels;
}

export const radialLayout: LayoutPlugin = {
  name: 'radial',
  calculate: (nodes: GraphNode[], edges: Edge[], options: RadialOptions = {}) => {
    const { radiusStep = 150, minRadius = 0 } = options;
    
    // Determine center node
    let centerNodeId = options.centerNodeId;
    if (!centerNodeId && nodes.length > 0) {
      // Find node with most connections as center
      const connectionCount = new Map<string, number>();
      nodes.forEach(node => connectionCount.set(node.id, 0));
      
      edges.forEach(edge => {
        connectionCount.set(edge.source, (connectionCount.get(edge.source) || 0) + 1);
        connectionCount.set(edge.target, (connectionCount.get(edge.target) || 0) + 1);
      });
      
      let maxConnections = 0;
      connectionCount.forEach((count, nodeId) => {
        if (count > maxConnections) {
          maxConnections = count;
          centerNodeId = nodeId;
        }
      });
    }
    
    if (!centerNodeId) {
      centerNodeId = nodes[0]?.id;
    }
    
    const levels = bfsLevels(centerNodeId, edges);
    
    // Group nodes by level
    const nodesByLevel = new Map<number, string[]>();
    let maxLevel = 0;
    
    // Assign levels to all nodes
    nodes.forEach(node => {
      if (!levels.has(node.id)) {
        // Disconnected nodes go to outermost level
        levels.set(node.id, maxLevel + 1);
      }
    });
    
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
      
      if (level === 0) {
        // Center node
        return {
          ...node,
          position: { x: 0, y: 0 }
        };
      }
      
      const nodesAtLevel = nodesByLevel.get(level) || [];
      const angleStep = (2 * Math.PI) / nodesAtLevel.length;
      const index = nodesAtLevel.indexOf(node.id);
      const radius = minRadius + level * radiusStep;
      const angle = index * angleStep;
      
      return {
        ...node,
        position: {
          x: radius * Math.cos(angle),
          y: radius * Math.sin(angle)
        }
      };
    });
  }
};