import { ReactNode } from "react";
import { ReactFlowProvider } from "reactflow";
import { LayerContextProvider } from "./LayerContext";

const AppContextProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  return (
    <ReactFlowProvider>
      <LayerContextProvider>{children}</LayerContextProvider>
    </ReactFlowProvider>
  );
};

export default AppContextProvider;
