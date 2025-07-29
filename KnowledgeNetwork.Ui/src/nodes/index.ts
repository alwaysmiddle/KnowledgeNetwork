import type { NodeTypes } from "reactflow";
import KnowledgeNodeComponent from "../features/knowledgeNode/KnowledgeNode";

// Node types configuration for React Flow
export const nodeTypes = {
  "knowledge-node" : KnowledgeNodeComponent,
  // Add any of your custom nodes here!
} satisfies NodeTypes;
