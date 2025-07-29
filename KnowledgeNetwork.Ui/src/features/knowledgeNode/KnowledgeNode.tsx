import { useState,  useRef } from "react";
import { Handle, NodeProps, Position } from "reactflow";
import NodeToolbar from "../nodeToolbar/NodeToolbar";
import { KnowledgeNodeData } from "../../types/KnowledgeNode";
import './KnowledgeNode.css';
import { useLayerContext } from "../../context/LayerContext";

function KnowledgeNode({
  id,
  selected,
  data,
  isConnectable,
}: NodeProps<KnowledgeNodeData>) {
  const {currentLayer, setCurrentLayer, onCurrentLayerNodesChange} = useLayerContext();
  const [displayText, setDisplayText] = useState(data.displayText);
  const nodeRef = useRef<HTMLDivElement>(null);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setDisplayText(event.target.value);
  };

  const handleBlur = () => {
    data.isEditing = false;
    data.displayText = displayText;
    data.onEditTextComplete(id);
  };

  return (
    <>
      {selected && nodeRef.current && (
        <NodeToolbar
          nodeHeight={nodeRef.current.offsetHeight}
          nodeWidth={nodeRef.current.offsetWidth}
        />
      )}
      <div
        ref={nodeRef}
        className="knowledge-node"
      >
        {data.isEditing ? (
          <input
            id="toro_input"
            type="text"
            value={displayText}
            onChange={handleInputChange}
            onBlur={handleBlur}
            autoFocus
            className="knowledge-node-input"
          />
        ) : (
          <div className="knowledge-node-display">
            {displayText ? displayText : "New Node"}
          </div>
        )}
        <Handle
          type="target"
          position={Position.Bottom}
          isConnectable={isConnectable}
        />
        <Handle
          type="source"
          position={Position.Top}
          isConnectable={isConnectable}
        />
      </div>
    </>
  );
}

export default KnowledgeNode;