import { useState, useEffect } from 'react';
import { UnionNode } from '../types/KnowledgeNode';
import { ApiService } from '../services/apiService';

interface UseNodesResult {
  nodes: UnionNode[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export const useNodes = (): UseNodesResult => {
  const [nodes, setNodes] = useState<UnionNode[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchNodes = async () => {
    try {
      setLoading(true);
      setError(null);
      
      // Check if API is healthy first
      const isHealthy = await ApiService.checkHealth();
      if (!isHealthy) {
        throw new Error('API is not available');
      }
      
      const fetchedNodes = await ApiService.fetchNodes();
      setNodes(fetchedNodes);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
      setError(errorMessage);
      console.error('Failed to fetch nodes:', err);
      
      // Set empty array on error
      setNodes([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchNodes();
  }, []);

  return {
    nodes,
    loading,
    error,
    refetch: fetchNodes,
  };
};