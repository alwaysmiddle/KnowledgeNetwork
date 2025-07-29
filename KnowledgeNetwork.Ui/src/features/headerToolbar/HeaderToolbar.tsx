import "./HeaderToolbar.css"

type Props = {
  
};

function HeaderToolbar(props: Props) {
  return (
    <div className="header-toolbar-sub-container">
      <div className="header-toolbar-recent-nodes">
        Recent Nodes
      </div>
      <div className="header-toolbar-planned-nodes">
        Lesson Plan
      </div>
    </div>
  );
};

export default HeaderToolbar;