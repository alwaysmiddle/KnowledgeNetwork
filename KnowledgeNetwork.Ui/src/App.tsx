import "reactflow/dist/style.css";
import './App.css'

import AppContextProvider from "./context/AppContextProvider";
import GraphNavigator from "./features/knowledgeNavigator/GraphNavigator";
import NodeContent from "./features/nodeContent/NodeContent";
import DescendantNavigator from "./features/DescendantNavigator";
import UpperLayerMiniMap from "./features/UpperLayerMiniMap";
import HeaderToolbar from "./features/headerToolbar/HeaderToolbar";

export default function App() {
  return (
    <AppContextProvider>
      <div className="container">
        <div className="header-toolbar">
          <HeaderToolbar />
        </div>
        <div className="menu">
          Menu
        </div>
        <div className="node-content">
          <NodeContent />
        </div>
        <div className="graph-navigator">
          <GraphNavigator />
        </div>
        <div className="minimap-a">
          <UpperLayerMiniMap />
        </div>
        <div className="minimap-b">
          <DescendantNavigator />
        </div>
      </div>
    </AppContextProvider>
  );
}
