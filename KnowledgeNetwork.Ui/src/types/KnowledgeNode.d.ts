import { Node , NodeProps } from "reactflow";
import KnowledgeNode from "../features/knowledgeNode/KnowledgeNode";

export interface KnowledgeNodeData{
  displayText?: string;
  documentId?: string;
  isEditing: boolean;
  parentKnowledgeNode: KnowledgeNode[];
  relationship: string[];
  upperLayer: KnowledgeNode[];
  lowerLayer: KnowledgeNode[];
  onEditTextComplete: (nodeId: string) => void;
}

type KnowledgeNode = Node<KnowledgeNodeData>;
type UnionNode = KnowledgeNode | Node;

export type {KnowledgeNode, UnionNode};