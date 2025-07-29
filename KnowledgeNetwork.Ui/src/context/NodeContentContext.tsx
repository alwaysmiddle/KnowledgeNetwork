import { ReactNode, createContext, useContext } from "react";

interface NodeContentContextType {
  
}

const NodeContentContext = createContext<
  NodeContentContextType | undefined
>(undefined);


export const NodeContentContextProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {

  return (
    <NodeContentContext.Provider 
      value={{
        
      }}>
      {children}
    </NodeContentContext.Provider>
  );
};

export const useNodeContentContext = () =>{
  const context = useContext(NodeContentContext);
  if (!context) {
    throw new Error("useLayerContext must be used within an NodeContentContextProvider");
  }
  return context;
};