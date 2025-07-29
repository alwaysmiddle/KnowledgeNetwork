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
      console.log('🚀 useNodes: Starting fetch process...');
      setLoading(true);
      setError(null);
      
      // Check if API is healthy first
      console.log('🏥 useNodes: Checking API health...');
      const isHealthy = await ApiService.checkHealth();
      if (!isHealthy) {
        throw new Error('API is not available');
      }
      console.log('✅ useNodes: API is healthy');
      
      const fetchedNodes = await ApiService.fetchNodes();
      console.log('📋 useNodes: Setting nodes, count:', fetchedNodes.length);
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