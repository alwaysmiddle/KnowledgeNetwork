import "./KnowledgeNavigatorToolbar.css";
import { Panel } from "reactflow";
import { useLayerContext } from "../../context/LayerContext";
import { useState } from "react";

function KnowledgeNavigatorToolbars() {
  const { triggerZoomOut: triggerZoomOut } = useLayerContext();
  const [showMainToolbarFlag, setShowMainToolbarFlag] =
    useState<boolean>(true);

  return (
    <>
      <Panel position="top-right">
        <div className="navigator-panel-container">
          {showMainToolbarFlag ? (
            <button onClick={()=>setShowMainToolbarFlag(false)}>
              <svg
                xmlns="http://www.w3.org/2000/svg"
                height="50px"
                viewBox="0 -960 960 960"
                width="50px"
                fill="green"
              >
                <path d="M280-240q-100 0-170-70T40-480q0-100 70-170t170-70h400q100 0 170 70t70 170q0 100-70 170t-170 70H280Zm0-66.67h400q72 0 122.67-50.66Q853.33-408 853.33-480t-50.66-122.67Q752-653.33 680-653.33H280q-72 0-122.67 50.66Q106.67-552 106.67-480t50.66 122.67Q208-306.67 280-306.67Zm400.63-66q44.7 0 76.04-31.29 31.33-31.3 31.33-76 0-44.71-31.29-76.04-31.3-31.33-76-31.33-44.71 0-76.04 31.29-31.34 31.3-31.34 76 0 44.71 31.3 76.04 31.29 31.33 76 31.33ZM480-480Z" />
              </svg>
            </button>
          ) : (
            <button onClick={()=>setShowMainToolbarFlag(true)}>
              <svg
                xmlns="http://www.w3.org/2000/svg"
                height="50px"
                viewBox="0 -960 960 960"
                width="50px"
                fill="darkred"
              >
                <path d="M280-240q-100 0-170-70T40-480q0-100 70-170t170-70h400q100 0 170 70t70 170q0 100-70 170t-170 70H280Zm0-66.67h400q72 0 122.67-50.66Q853.33-408 853.33-480t-50.66-122.67Q752-653.33 680-653.33H280q-72 0-122.67 50.66Q106.67-552 106.67-480t50.66 122.67Q208-306.67 280-306.67Zm-.71-66q44.71 0 76.04-31.29 31.34-31.3 31.34-76 0-44.71-31.3-76.04-31.29-31.33-76-31.33-44.7 0-76.04 31.29-31.33 31.3-31.33 76 0 44.71 31.29 76.04 31.3 31.33 76 31.33ZM480-480Z" />
              </svg>
            </button>
          )}
          <button onClick={() => triggerZoomOut()}>
            <svg
              xmlns="http://www.w3.org/2000/svg"
              height="42px"
              viewBox="0 -960 960 960"
              width="42px"
              fill="darkgreen"
            >
              <path d="M412-114v-470L210-383l-96-97 366-366 366 366-96 98-202-202v470H412Z" />
            </svg>
          </button>
        </div>
      </Panel>
      {
        showMainToolbarFlag ? 
        <Panel position="top-left">
          <div className="node-panel-container">
            HELLO TORO
          </div>
        </Panel> : <></>
      }
      
    </>
  );
}
export default KnowledgeNavigatorToolbars;
