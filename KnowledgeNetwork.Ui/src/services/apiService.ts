import { UnionNode } from "../types/KnowledgeNode";

// API Response interface based on .NET API
interface ApiNodeResponse {
  id: number;
  title: string;
  content: string | null;
  nodeType: string;
  xPosition: number;
  yPosition: number;
  createdAt: string;
}

// Base API URL - adjust if your API runs on a different port
const API_BASE_URL = 'http://localhost:5000/api';

export class ApiService {
  // Fetch nodes from the API
  static async fetchNodes(): Promise<UnionNode[]> {
    try {
      const response = await fetch(`${API_BASE_URL}/nodes`);
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      
      const apiNodes: ApiNodeResponse[] = await response.json();
      
      // Transform API response to match our React Flow node structure
      return apiNodes.map((apiNode, index) => ({
        id: apiNode.id.toString(),
        type: "knowledge-node",
        position: { 
          x: apiNode.xPosition || index * 150, // Fallback positioning if API doesn't provide positions
          y: apiNode.yPosition || 0 
        },
        data: {
          displayText: apiNode.title,
          documentId: apiNode.id.toString(),
          parentKnowledgeNode: [],
          relationship: [],
          upperLayer: [],
          lowerLayer: [],
          isEditing: false,
          onEditTextComplete: () => {
            console.log(`Edit text complete for node ${apiNode.id}`);
          },
        },
      }));
    } catch (error) {
      console.error('Error fetching nodes from API:', error);
      // Return empty array on error - you can enhance this later
      return [];
    }
  }

  // Health check to verify API is running
  static async checkHealth(): Promise<boolean> {
    try {
      const response = await fetch(`${API_BASE_URL}/health`);
      return response.ok;
    } catch (error) {
      console.error('API health check failed:', error);
      return false;
    }
  }
}