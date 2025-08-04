import { ReactNode } from "react";
import { ReactFlowProvider } from "reactflow";
import { LayerContextProvider } from "./LayerContext";
import { NavigationProvider } from "./NavigationContext";

const AppContextProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  return (
    <ReactFlowProvider>
      <NavigationProvider>
        <LayerContextProvider>{children}</LayerContextProvider>
      </NavigationProvider>
    </ReactFlowProvider>
  );
};

export default AppContextProvider;
