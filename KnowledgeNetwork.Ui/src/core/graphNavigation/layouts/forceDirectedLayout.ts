import { LayoutPlugin, GraphNode } from '../types';
import { Edge } from 'reactflow';

interface ForceDirectedOptions {
  iterations?: number;
  nodeRepulsion?: number;
  edgeDistance?: number;
  centerForce?: number;
}

interface Vector {
  x: number;
  y: number;
}

export const forceDirectedLayout: LayoutPlugin = {
  name: 'force-directed',
  calculate: (nodes: GraphNode[], edges: Edge[], options: ForceDirectedOptions = {}) => {
    const {
      iterations = 100,
      nodeRepulsion = 500,
      edgeDistance = 100,
      centerForce = 0.01
    } = options;
    
    // Initialize positions randomly if not set
    const positions = new Map<string, Vector>();
    nodes.forEach(node => {
      positions.set(node.id, {
        x: node.position?.x || (Math.random() - 0.5) * 300,
        y: node.position?.y || (Math.random() - 0.5) * 300
      });
    });
    
    // Create edge map for quick lookup
    const edgeMap = new Map<string, Set<string>>();
    edges.forEach(edge => {
      if (!edgeMap.has(edge.source)) edgeMap.set(edge.source, new Set());
      if (!edgeMap.has(edge.target)) edgeMap.set(edge.target, new Set());
      edgeMap.get(edge.source)!.add(edge.target);
      edgeMap.get(edge.target)!.add(edge.source);
    });
    
    // Force simulation
    for (let iter = 0; iter < iterations; iter++) {
      const forces = new Map<string, Vector>();
      
      // Initialize forces
      nodes.forEach(node => {
        forces.set(node.id, { x: 0, y: 0 });
      });
      
      // Repulsion forces between all nodes
      for (let i = 0; i < nodes.length; i++) {
        for (let j = i + 1; j < nodes.length; j++) {
          const node1 = nodes[i];
          const node2 = nodes[j];
          const pos1 = positions.get(node1.id)!;
          const pos2 = positions.get(node2.id)!;
          
          const dx = pos2.x - pos1.x;
          const dy = pos2.y - pos1.y;
          const distance = Math.sqrt(dx * dx + dy * dy);
          
          if (distance > 0) {
            const force = nodeRepulsion / (distance * distance);
            const fx = (dx / distance) * force;
            const fy = (dy / distance) * force;
            
            const force1 = forces.get(node1.id)!;
            const force2 = forces.get(node2.id)!;
            
            force1.x -= fx;
            force1.y -= fy;
            force2.x += fx;
            force2.y += fy;
          }
        }
      }
      
      // Attraction forces for connected nodes
      edges.forEach(edge => {
        const pos1 = positions.get(edge.source)!;
        const pos2 = positions.get(edge.target)!;
        
        const dx = pos2.x - pos1.x;
        const dy = pos2.y - pos1.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        if (distance > 0) {
          const force = (distance - edgeDistance) * 0.1;
          const fx = (dx / distance) * force;
          const fy = (dy / distance) * force;
          
          const force1 = forces.get(edge.source)!;
          const force2 = forces.get(edge.target)!;
          
          force1.x += fx;
          force1.y += fy;
          force2.x -= fx;
          force2.y -= fy;
        }
      });
      
      // Center force
      nodes.forEach(node => {
        const pos = positions.get(node.id)!;
        const force = forces.get(node.id)!;
        force.x -= pos.x * centerForce;
        force.y -= pos.y * centerForce;
      });
      
      // Apply forces
      nodes.forEach(node => {
        const pos = positions.get(node.id)!;
        const force = forces.get(node.id)!;
        pos.x += force.x;
        pos.y += force.y;
      });
    }
    
    // Return nodes with updated positions
    return nodes.map(node => ({
      ...node,
      position: positions.get(node.id)!
    }));
  }
};