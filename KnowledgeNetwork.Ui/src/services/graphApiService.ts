import { GraphData } from '../core/graphNavigation/types';
import { Edge } from 'reactflow';
import { UnionNode } from '../types/KnowledgeNode';

// API Response interfaces based on expected .NET API structure
interface ApiNodeResponse {
  id: number;
  title: string;
  content: string | null;
  nodeType: string;
  xPosition: number;
  yPosition: number;
  parentIds?: number[];
  childIds?: number[];
  createdAt: string;
}

interface ApiEdgeResponse {
  id: string;
  sourceId: number;
  targetId: number;
  relationshipType: string;
}

interface ApiGraphResponse {
  nodes: ApiNodeResponse[];
  edges: ApiEdgeResponse[];
  metadata?: {
    layout?: string;
    centerNodeId?: number;
  };
}

const API_BASE_URL = 'http://localhost:5000/api';

export class GraphApiService {
  // Fetch the initial graph (all top-level nodes)
  static async fetchInitialGraph(): Promise<GraphData> {
    try {
      // TODO: Replace with actual API endpoint when available
      // Expected endpoint: GET /api/graph/nodes/root or /api/graph/initial
      
      // For now, return mock data
      return this.getMockInitialGraph();
    } catch (error) {
      console.error('Error fetching initial graph:', error);
      throw error;
    }
  }
  
  // Fetch a node's subgraph (children and their relationships)
  static async fetchNodeSubgraph(nodeId: string): Promise<GraphData> {
    try {
      // TODO: Replace with actual API endpoint when available
      // Expected endpoint: GET /api/graph/nodes/{nodeId}/subgraph
      
      // For now, return mock data
      return this.getMockNodeSubgraph(nodeId);
    } catch (error) {
      console.error(`Error fetching subgraph for node ${nodeId}:`, error);
      throw error;
    }
  }
  
  // Fetch parent graph (for navigating back)
  static async fetchParentGraph(nodeId: string): Promise<GraphData> {
    try {
      // TODO: Replace with actual API endpoint when available
      // Expected endpoint: GET /api/graph/nodes/{nodeId}/parent-graph
      
      // For now, return mock data
      return this.getMockParentGraph(nodeId);
    } catch (error) {
      console.error(`Error fetching parent graph for node ${nodeId}:`, error);
      throw error;
    }
  }
  
  // Transform API response to React Flow format
  private static transformApiResponse(apiResponse: ApiGraphResponse): GraphData {
    const nodes = apiResponse.nodes.map(apiNode => ({
      id: apiNode.id.toString(),
      type: 'knowledge',
      position: { 
        x: apiNode.xPosition || 0,
        y: apiNode.yPosition || 0
      },
      data: {
        displayText: apiNode.title,
        documentId: apiNode.id.toString(),
        content: apiNode.content,
        isEditing: false,
        parentKnowledgeNode: [],
        relationship: [],
        upperLayer: [],
        lowerLayer: [],
        onEditTextComplete: () => {}
      }
    }));
    
    const edges: Edge[] = apiResponse.edges.map(apiEdge => ({
      id: apiEdge.id,
      source: apiEdge.sourceId.toString(),
      target: apiEdge.targetId.toString(),
      type: 'default',
      data: { relationshipType: apiEdge.relationshipType }
    }));
    
    return {
      nodes,
      edges,
      metadata: {
        layoutHint: apiResponse.metadata?.layout,
        centerNodeId: apiResponse.metadata?.centerNodeId?.toString()
      }
    };
  }
  
  // Mock data generators (to be removed when real API is ready)
  private static getMockInitialGraph(): GraphData {
    const nodes = [
      {
        id: '1',
        type: 'knowledge' as const,
        position: { x: 0, y: 0 },
        data: {
          displayText: 'Knowledge Management System',
          isEditing: false,
          parentKnowledgeNode: [],
          relationship: [],
          upperLayer: [],
          lowerLayer: [],
          onEditTextComplete: () => {}
        }
      },
      {
        id: '2',
        type: 'knowledge' as const,
        position: { x: 200, y: 0 },
        data: {
          displayText: 'React Architecture',
          isEditing: false,
          parentKnowledgeNode: [],
          relationship: [],
          upperLayer: [],
          lowerLayer: [],
          onEditTextComplete: () => {}
        }
      },
      {
        id: '3',
        type: 'knowledge' as const,
        position: { x: -200, y: 0 },
        data: {
          displayText: '.NET Backend',
          isEditing: false,
          parentKnowledgeNode: [],
          relationship: [],
          upperLayer: [],
          lowerLayer: [],
          onEditTextComplete: () => {}
        }
      }
    ];
    
    const edges: Edge[] = [
      { id: 'e1-2', source: '1', target: '2', type: 'default' },
      { id: 'e1-3', source: '1', target: '3', type: 'default' }
    ];
    
    return {
      nodes,
      edges,
      metadata: { layoutHint: 'hierarchical' }
    };
  }
  
  private static getMockNodeSubgraph(nodeId: string): GraphData {
    // Generate different subgraphs based on the node
    const subgraphs: Record<string, () => GraphData> = {
      '1': () => ({
        nodes: [
          {
            id: '1-1',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Core Concepts',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '1-2',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Implementation Details',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '1-3',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Best Practices',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          }
        ],
        edges: [],
        metadata: { layoutHint: 'radial', centerNodeId: '1-1' }
      }),
      '2': () => ({
        nodes: [
          {
            id: '2-1',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Component Structure',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '2-2',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'State Management',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '2-3',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Routing',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '2-4',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Performance',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          }
        ],
        edges: [
          { id: 'e2-1-2', source: '2-1', target: '2-2', type: 'default' },
          { id: 'e2-1-3', source: '2-1', target: '2-3', type: 'default' },
          { id: 'e2-2-4', source: '2-2', target: '2-4', type: 'default' }
        ],
        metadata: { layoutHint: 'hierarchical' }
      }),
      '3': () => ({
        nodes: [
          {
            id: '3-1',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'API Design',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '3-2',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Database Layer',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          },
          {
            id: '3-3',
            type: 'knowledge' as const,
            position: { x: 0, y: 0 },
            data: {
              displayText: 'Security',
              isEditing: false,
              parentKnowledgeNode: [],
              relationship: [],
              upperLayer: [],
              lowerLayer: [],
              onEditTextComplete: () => {}
            }
          }
        ],
        edges: [
          { id: 'e3-1-2', source: '3-1', target: '3-2', type: 'default' },
          { id: 'e3-1-3', source: '3-1', target: '3-3', type: 'default' }
        ],
        metadata: { layoutHint: 'hierarchical' }
      })
    };
    
    // Default subgraph for unknown nodes
    const defaultSubgraph = (): GraphData => ({
      nodes: [
        {
          id: `${nodeId}-1`,
          type: 'knowledge' as const,
          position: { x: 0, y: 0 },
          data: {
            displayText: `Child 1 of ${nodeId}`,
            isEditing: false,
            parentKnowledgeNode: [],
            relationship: [],
            upperLayer: [],
            lowerLayer: [],
            onEditTextComplete: () => {}
          }
        },
        {
          id: `${nodeId}-2`,
          type: 'knowledge' as const,
          position: { x: 0, y: 0 },
          data: {
            displayText: `Child 2 of ${nodeId}`,
            isEditing: false,
            parentKnowledgeNode: [],
            relationship: [],
            upperLayer: [],
            lowerLayer: [],
            onEditTextComplete: () => {}
          }
        }
      ],
      edges: [],
      metadata: { layoutHint: 'hierarchical' }
    });
    
    return subgraphs[nodeId]?.() || defaultSubgraph();
  }
  
  private static getMockParentGraph(nodeId: string): GraphData {
    // For mock data, just return the initial graph
    return this.getMockInitialGraph();
  }
}