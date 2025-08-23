import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { HomePage } from './pages/HomePage';
import { CodeAnalysisPage } from './pages/CodeAnalysisPage';
import { Navigation } from './components/Navigation';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/code-analysis" element={<CodeAnalysisPage />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App
